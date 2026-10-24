"""Authenticated web routes for dashboard setup and library management."""

from __future__ import annotations

from collections import defaultdict, deque
import copy
from functools import wraps
import hashlib
import hmac
import json
import os
from pathlib import Path
import secrets
import tempfile
import threading
import time
from typing import Any, Callable, Deque, Dict, Optional

from flask import Flask, jsonify, redirect, render_template, request, session, url_for

from plex_autoplay.application.manual_marker_service import ManualMarkerService, MarkerConflictError
from plex_autoplay.application.scan_coordinator import ScanCoordinator
from plex_autoplay.infrastructure.media.library_repository import LibraryRepository


PASSWORD_HASH_ITERATIONS = 600_000
SESSION_LIFETIME_SECONDS = 12 * 60 * 60
LOGIN_WINDOW_SECONDS = 5 * 60
MAX_LOGIN_ATTEMPTS = 5


class AdminStore:
    def __init__(self, config_dir: str | Path):
        self.config_dir = Path(config_dir)
        self.credentials_path = self.config_dir / "dashboard-admin.json"
        self.secret_path = self.config_dir / "dashboard-session.key"
        self._lock = threading.Lock()

    @property
    def configured(self) -> bool:
        return self.credentials_path.is_file()

    def session_secret(self) -> str:
        self.config_dir.mkdir(parents=True, exist_ok=True)
        try:
            return self.secret_path.read_text(encoding="ascii").strip()
        except FileNotFoundError:
            secret = secrets.token_hex(32)
            self._write_atomic(self.secret_path, secret.encode("ascii"))
            return secret

    def create(self, username: str, password: str) -> None:
        if not username.strip() or len(password) < 8:
            raise ValueError("Informe um usuário e uma senha com pelo menos 8 caracteres.")
        with self._lock:
            if self.configured:
                raise ValueError("A conta administrativa já foi configurada.")
            salt = secrets.token_bytes(16)
            password_hash = hashlib.pbkdf2_hmac(
                "sha256",
                password.encode("utf-8"),
                salt,
                PASSWORD_HASH_ITERATIONS,
            )
            payload = {
                "username": username.strip(),
                "salt": salt.hex(),
                "password_hash": password_hash.hex(),
                "iterations": PASSWORD_HASH_ITERATIONS,
            }
            self._write_atomic(
                self.credentials_path,
                (json.dumps(payload, ensure_ascii=False) + "\n").encode("utf-8"),
            )

    def verify(self, username: str, password: str) -> bool:
        if not self.configured:
            return False
        with self.credentials_path.open("r", encoding="utf-8") as credentials_file:
            payload = json.load(credentials_file)
        stored_username = payload.get("username")
        if not isinstance(stored_username, str) or not hmac.compare_digest(stored_username, username):
            return False
        salt = bytes.fromhex(payload["salt"])
        iterations = payload.get("iterations")
        if not isinstance(iterations, int) or iterations < 100_000:
            raise ValueError(f"Hash de senha com configuração inválida: {self.credentials_path}")
        expected_hash = bytes.fromhex(payload["password_hash"])
        candidate_hash = hashlib.pbkdf2_hmac("sha256", password.encode("utf-8"), salt, iterations)
        return hmac.compare_digest(candidate_hash, expected_hash)

    @staticmethod
    def _write_atomic(path: Path, contents: bytes) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        temporary_path: Optional[Path] = None
        try:
            with tempfile.NamedTemporaryFile(dir=path.parent, prefix=f".{path.name}.", delete=False) as temp_file:
                temporary_path = Path(temp_file.name)
                temp_file.write(contents)
                temp_file.flush()
                os.fsync(temp_file.fileno())
            os.chmod(temporary_path, 0o600)
            os.replace(temporary_path, path)
        finally:
            if temporary_path is not None and temporary_path.exists():
                temporary_path.unlink()


class LoginLimiter:
    def __init__(self):
        self._attempts: Dict[str, Deque[float]] = defaultdict(deque)
        self._lock = threading.Lock()

    def allowed(self, identity: str, now: Optional[float] = None) -> bool:
        current = time.monotonic() if now is None else now
        with self._lock:
            attempts = self._attempts[identity]
            while attempts and current - attempts[0] >= LOGIN_WINDOW_SECONDS:
                attempts.popleft()
            return len(attempts) < MAX_LOGIN_ATTEMPTS

    def record_failure(self, identity: str, now: Optional[float] = None) -> None:
        current = time.monotonic() if now is None else now
        with self._lock:
            attempts = self._attempts[identity]
            while attempts and current - attempts[0] >= LOGIN_WINDOW_SECONDS:
                attempts.popleft()
            attempts.append(current)

    def clear(self, identity: str) -> None:
        with self._lock:
            self._attempts.pop(identity, None)


def _update_report_markers(
    report: Dict[str, Any],
    series_title: str,
    season: int,
    episode_number: int,
    values: Dict[str, float],
) -> Optional[Dict[str, Any]]:
    changed = False
    libraries = report.get("libraries")
    if not isinstance(libraries, list):
        return None
    for library in libraries:
        if not isinstance(library, dict):
            continue
        episodes = library.get("episodes")
        if not isinstance(episodes, list):
            continue
        library_changed = False
        for episode in episodes:
            if (
                not isinstance(episode, dict)
                or not isinstance(episode.get("series"), str)
                or episode["series"].strip().casefold() != series_title.strip().casefold()
                or episode.get("season") != season
                or episode.get("episode") != episode_number
            ):
                continue
            episode["intro"] = {
                "start_ms": int(values["start"] * 1000) if "start" in values else None,
                "end_ms": int(values["end"] * 1000) if "end" in values else None,
            }
            episode["credits_ms"] = int(values["credits"] * 1000) if "credits" in values else None
            library_changed = True
            changed = True
        if library_changed:
            library["with_intro"] = sum(
                isinstance(item, dict)
                and isinstance(item.get("intro"), dict)
                and item["intro"].get("start_ms") is not None
                and item["intro"].get("end_ms") is not None
                for item in episodes
            )
            library["with_credits"] = sum(
                isinstance(item, dict) and item.get("credits_ms") is not None for item in episodes
            )
            missing = []
            for item in episodes:
                if not isinstance(item, dict):
                    continue
                intro = item.get("intro")
                intro = intro if isinstance(intro, dict) else {}
                fields = []
                if intro.get("start_ms") is None:
                    fields.append("start")
                if intro.get("end_ms") is None:
                    fields.append("end")
                if item.get("credits_ms") is None:
                    fields.append("credits")
                if fields:
                    missing.append(
                        {
                            "series": item.get("series"),
                            "season": item.get("season"),
                            "episode": item.get("episode"),
                            "path": item.get("path"),
                            "fields": fields,
                        }
                    )
            library["missing"] = missing
    return report if changed else None


def create_app(
    library_repository: LibraryRepository,
    config_dir: str | Path,
    *,
    cookie_secure: bool = False,
    scan_coordinator: Optional[ScanCoordinator] = None,
    manual_marker_service: Optional[ManualMarkerService] = None,
    marker_reload: Optional[Callable[[], None]] = None,
) -> Flask:
    app = Flask(__name__)
    admin_store = AdminStore(config_dir)
    app.secret_key = admin_store.session_secret()
    app.permanent_session_lifetime = SESSION_LIFETIME_SECONDS
    app.config.update(
        SESSION_COOKIE_HTTPONLY=True,
        SESSION_COOKIE_SAMESITE="Lax",
        SESSION_COOKIE_SECURE=cookie_secure,
    )
    limiter = LoginLimiter()

    def csrf_token() -> str:
        token = session.get("_csrf_token")
        if not token:
            token = secrets.token_urlsafe(32)
            session["_csrf_token"] = token
        return token

    @app.before_request
    def validate_csrf() -> Optional[Any]:
        if request.method not in {"POST", "PUT", "PATCH", "DELETE"}:
            return None
        expected = session.get("_csrf_token")
        provided = request.headers.get("X-CSRF-Token") or request.form.get("csrf_token")
        if not isinstance(expected, str) or not isinstance(provided, str) or not hmac.compare_digest(
            expected, provided
        ):
            return jsonify(error="Token CSRF inválido ou ausente."), 400
        return None

    def authenticated(view: Callable[..., Any]) -> Callable[..., Any]:
        @wraps(view)
        def wrapped(*args: Any, **kwargs: Any) -> Any:
            if not session.get("admin"):
                if request.path.startswith("/api/"):
                    return jsonify(error="Autenticação necessária."), 401
                return redirect(url_for("login"))
            return view(*args, **kwargs)

        return wrapped

    @app.context_processor
    def inject_csrf_token() -> Dict[str, Callable[[], str]]:
        return {"csrf_token": csrf_token}

    @app.get("/setup")
    def setup() -> Any:
        if admin_store.configured:
            return redirect(url_for("login"))
        return render_template("auth.html", title="Criar administrador", setup=True)

    @app.post("/setup")
    def create_admin() -> Any:
        if admin_store.configured:
            return jsonify(error="A conta administrativa já foi configurada."), 409
        try:
            admin_store.create(request.form.get("username", ""), request.form.get("password", ""))
        except ValueError as error:
            return jsonify(error=str(error)), 400
        session.clear()
        session["admin"] = request.form["username"].strip()
        session.permanent = True
        csrf_token()
        return redirect(url_for("dashboard"))

    @app.get("/login")
    def login() -> Any:
        if not admin_store.configured:
            return redirect(url_for("setup"))
        return render_template("auth.html", title="Entrar", setup=False)

    @app.post("/login")
    def authenticate() -> Any:
        if not admin_store.configured:
            return redirect(url_for("setup"))
        identity = request.remote_addr or "unknown"
        if not limiter.allowed(identity):
            return jsonify(error="Muitas tentativas. Aguarde antes de tentar novamente."), 429
        username = request.form.get("username", "")
        password = request.form.get("password", "")
        if not admin_store.verify(username, password):
            limiter.record_failure(identity)
            return jsonify(error="Usuário ou senha inválidos."), 401
        limiter.clear(identity)
        session.clear()
        session["admin"] = username
        session.permanent = True
        csrf_token()
        return redirect(url_for("dashboard"))

    @app.post("/logout")
    @authenticated
    def logout() -> Any:
        session.clear()
        return redirect(url_for("login"))

    @app.get("/")
    @authenticated
    def dashboard() -> Any:
        snapshot = (
            scan_coordinator.snapshot().to_dict()
            if scan_coordinator is not None
            else {"status": "idle", "started_at": None, "completed_at": None, "result": None, "error": None}
        )
        result = snapshot.get("result") if isinstance(snapshot.get("result"), dict) else {}
        report_libraries = result.get("libraries", [])
        if not isinstance(report_libraries, list):
            report_libraries = []
        reports_by_path = {
            library["path"]: library
            for library in report_libraries
            if isinstance(library, dict) and isinstance(library.get("path"), str)
        }
        libraries = copy.deepcopy(report_libraries)
        for configured_library in library_repository.list_libraries():
            if configured_library.path in reports_by_path:
                continue
            libraries.append(
                {
                    "name": configured_library.name,
                    "path": configured_library.path,
                    "kind": configured_library.kind,
                    "episode_count": 0,
                    "with_intro": 0,
                    "with_credits": 0,
                    "episodes": [],
                    "missing": [],
                    "ambiguities": [],
                    "pending": [],
                    "errors": [],
                }
            )
        for library in libraries:
            by_series: Dict[str, Dict[int, list[dict[str, Any]]]] = {}
            for episode in library.get("episodes", []):
                series = episode.get("series", "Desconhecida")
                season = episode.get("season", 0)
                by_series.setdefault(series, {}).setdefault(season, []).append(episode)
            library["series_groups"] = [
                {
                    "title": title,
                    "seasons": [
                        {"number": number, "episodes": episodes}
                        for number, episodes in sorted(seasons.items())
                    ],
                }
                for title, seasons in sorted(by_series.items(), key=lambda item: item[0].casefold())
            ]
        totals = {
            "episodes": sum(library.get("episode_count", 0) for library in libraries),
            "intros": sum(library.get("with_intro", 0) for library in libraries),
            "credits": sum(library.get("with_credits", 0) for library in libraries),
            "missing": sum(len(library.get("missing", [])) for library in libraries),
        }
        return render_template(
            "dashboard.html",
            snapshot=snapshot,
            libraries=libraries,
            totals=totals,
        )

    @app.get("/libraries")
    @authenticated
    def library_manager() -> Any:
        return render_template("library_manager.html")

    @app.get("/api/directories")
    @authenticated
    def directories() -> Any:
        relative_path = request.args.get("path", "")
        try:
            return jsonify(directories=library_repository.list_directories(relative_path))
        except ValueError as error:
            return jsonify(error=str(error)), 400

    @app.get("/api/status")
    @authenticated
    def status() -> Any:
        if scan_coordinator is None:
            return jsonify(error="Serviço de varredura indisponível."), 503
        return jsonify(scan_coordinator.snapshot().to_dict())

    @app.post("/api/scan")
    @authenticated
    def start_scan() -> Any:
        if scan_coordinator is None:
            return jsonify(error="Serviço de varredura indisponível."), 503
        try:
            started = scan_coordinator.start()
        except Exception as error:
            app.logger.exception("Não foi possível iniciar a varredura.")
            return jsonify(error=f"Não foi possível iniciar a varredura: {error}"), 500
        if not started:
            return jsonify(error="Já existe uma varredura em andamento.", status=scan_coordinator.snapshot().status), 409
        return jsonify(scan_coordinator.snapshot().to_dict()), 202

    @app.route("/api/libraries", methods=["GET", "POST", "DELETE"])
    @authenticated
    def libraries() -> Any:
        if request.method == "GET":
            return jsonify(
                libraries=[
                    {"name": library.name, "path": library.path, "kind": library.kind}
                    for library in library_repository.list_libraries()
                ]
            )
        payload = request.get_json(silent=False)
        if not isinstance(payload, dict):
            return jsonify(error="Corpo JSON inválido."), 400
        if request.method == "POST":
            relative_path = payload.get("path", "")
            kind = payload.get("kind", "")
            if not isinstance(relative_path, str) or not isinstance(kind, str):
                return jsonify(error="Informe um caminho e uma classificação válidos."), 400
            try:
                library = library_repository.add_library(relative_path, kind)
            except ValueError as error:
                return jsonify(error=str(error)), 400
            return jsonify(name=library.name, path=library.path, kind=library.kind), 201
        relative_path = payload.get("path", "")
        if not isinstance(relative_path, str):
            return jsonify(error="Informe um caminho de biblioteca válido."), 400
        try:
            removed = library_repository.remove_library(relative_path)
        except ValueError as error:
            return jsonify(error=str(error)), 400
        return ("", 204) if removed else (jsonify(error="Biblioteca não encontrada."), 404)

    @app.route("/api/markers", methods=["PUT", "DELETE"])
    @authenticated
    def markers() -> Any:
        if manual_marker_service is None:
            return jsonify(error="Serviço de marcadores indisponível."), 503
        payload = request.get_json(silent=False)
        if not isinstance(payload, dict):
            return jsonify(error="Corpo JSON inválido."), 400

        def response_with_refreshed_report(
            result: Dict[str, float],
            series: str,
            season: int,
            episode: int,
        ) -> Any:
            try:
                if marker_reload is not None:
                    marker_reload()
                effective_values = manual_marker_service.get_values(series, season, episode)
                panel_updated = (
                    scan_coordinator.update_result(
                        lambda report: _update_report_markers(
                            report,
                            series,
                            season,
                            episode,
                            effective_values,
                        )
                    )
                    if scan_coordinator is not None
                    else False
                )
            except (OSError, RuntimeError, TypeError, ValueError) as error:
                app.logger.exception("O marcador foi salvo, mas o relatório do painel não pôde ser atualizado.")
                return jsonify(
                    error=f"O marcador foi salvo, mas o relatório do painel não pôde ser atualizado: {error}",
                    report_update_failed=True,
                    markers=result,
                ), 500
            return jsonify(markers=result, panel_updated=panel_updated)

        try:
            if request.method == "PUT":
                values = payload.get("values")
                if not isinstance(values, dict):
                    raise ValueError("Informe os valores de marcadores em um objeto.")
                series = payload.get("series", "")
                season = payload.get("season")
                episode = payload.get("episode")
                result = manual_marker_service.update(
                    series,
                    season,
                    episode,
                    values,
                    confirm_replace=payload.get("confirm_replace", False) is True,
                )
                return response_with_refreshed_report(result, series, season, episode)
            fields = payload.get("fields")
            if not isinstance(fields, list) or not all(isinstance(field, str) for field in fields):
                raise ValueError("Informe os campos a remover como uma lista.")
            series = payload.get("series", "")
            season = payload.get("season")
            episode = payload.get("episode")
            result = manual_marker_service.remove_fields(
                series,
                season,
                episode,
                fields,
            )
            return response_with_refreshed_report(result, series, season, episode)
        except MarkerConflictError as error:
            return jsonify(error=str(error), confirmation_required=True), 409
        except (TypeError, ValueError) as error:
            return jsonify(error=str(error)), 400

    return app
