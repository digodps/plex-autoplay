"""Authenticated web routes for dashboard setup and library management."""

from __future__ import annotations

from collections import defaultdict, deque
import copy
from functools import wraps
import hashlib
import hmac
import json
import logging
import os
import queue
import redis
from pathlib import Path
import secrets
import tempfile

redis_client = redis.from_url(os.getenv("REDIS_URL", "redis://127.0.0.1:6379/0"), decode_responses=True)
import threading
import time
from typing import Any, Callable, Deque, Dict, Optional

from flask import Flask, jsonify, redirect, render_template, request, session, url_for, Response, stream_with_context

from plex_autoplay.application.manual_marker_service import ManualMarkerService, MarkerConflictError
from plex_autoplay.infrastructure.web.process_metrics import ProcessMetricsSampler
from plex_autoplay.infrastructure.plex.client import PlexManager
from plex_autoplay.application.plex_library_scanner import PlexLibraryScanner, ScanResult
from plex_autoplay.config import config as app_config
import threading
import uuid
import hmac
import hashlib


PASSWORD_HASH_ITERATIONS = 600_000
SESSION_LIFETIME_SECONDS = 12 * 60 * 60
LOGIN_WINDOW_SECONDS = 5 * 60
MAX_LOGIN_ATTEMPTS = 5
logger = logging.getLogger("plex-autoplay.dashboard")


class SetupTokenError(ValueError):
    """Raised when the one-time dashboard bootstrap token is missing or invalid."""


class ConfigStore:
    """Persist application configuration (Plex URL, token, autoplay options) as JSON."""

    # Keys that we manage; order matches Settings attributes
    KEYS = [
        "plex_url",
        "plex_token",
        "auto_skip_intro",
        "auto_next_episode",
        "next_episode_threshold_seconds",
        "poll_interval_seconds",
        "allowed_client_ids",
        "allowed_usernames",
        "default_intro_skip_seconds",
        "scan_interval_minutes",
        "webhook_secret",
        "redis_url",
        "enable_fs_watcher",
    ]

    def __init__(self, config_dir: str | Path):
        self.config_dir = Path(config_dir)
        self.path = self.config_dir / "app-config.json"
        self._lock = threading.Lock()

    def _env_defaults(self) -> dict:
        """Build a dict from the global Settings (environment/.env)."""
        return {k: getattr(app_config, k) for k in self.KEYS}

    def load(self) -> dict:
        with self._lock:
            # Start with environment defaults
            data = self._env_defaults()
            if self.path.is_file():
                try:
                    saved = json.loads(self.path.read_text(encoding="utf-8"))
                    # Overlay saved values on top of env defaults
                    data.update(saved)
                    return data
                except Exception:
                    logger.exception("Falha ao ler configuração; usando padrões do ambiente.")
            return data

    def save(self, data: dict) -> None:
        # Merge incoming data over current loaded config (which already includes env defaults)
        merged = {**self.load(), **data}
        self.config_dir.mkdir(parents=True, exist_ok=True)
        tmp = self.path.with_suffix(".tmp")
        try:
            tmp.write_text(json.dumps(merged, ensure_ascii=False, indent=2), encoding="utf-8")
            os.chmod(tmp, 0o600)
            os.replace(tmp, self.path)
        finally:
            if tmp.exists():
                try:
                    tmp.unlink()
                except OSError:
                    pass


class AdminStore:
    """Authentication store that prefers environment variables.
    If ADMIN_USER / ADMIN_PASS are set in the container environment they are used
    directly (no file needed).  Otherwise we fall back to the legacy JSON file so
    existing deployments keep working.
    """

    PASSWORD_HASH_ITERATIONS = 600_000

    def __init__(self, config_dir: str | Path):
        self.config_dir = Path(config_dir)
        self.credentials_path = self.config_dir / "dashboard-admin.json"
        self.secret_path = self.config_dir / "dashboard-session.key"
        self._lock = threading.Lock()

        # --- read env once (they never change at runtime) -----------------
        self._env_user = os.getenv("ADMIN_USER")
        self._env_pass = os.getenv("ADMIN_PASS")

    # --------------------------------------------------------------------
    @property
    def configured(self) -> bool:
        """True when we have *any* source of credentials (env or file)."""
        if self._env_user and self._env_pass:
            return True
        return self.credentials_path.is_file()

    # --------------------------------------------------------------------
    def session_secret(self) -> str:
        self.config_dir.mkdir(parents=True, exist_ok=True)
        try:
            return (self.config_dir / "dashboard-session.key").read_text(encoding="ascii").strip()
        except (FileNotFoundError, PermissionError, OSError):
            secret = secrets.token_hex(32)
            self._write_atomic(self.config_dir / "dashboard-session.key", secret.encode("ascii"))
            return secret

    # --------------------------------------------------------------------
    def verify(self, username: str, password: str) -> bool:
        # 1️⃣  ENV‑var credentials (preferred)
        if self._env_user and self._env_pass:
            return hmac.compare_digest(self._env_user, username) and \
                   hmac.compare_digest(self._env_pass, password)

        # 2️⃣  Legacy JSON file (kept for backward compatibility)
        if not self.credentials_path.is_file():
            return False
        try:
            with self.credentials_path.open("r", encoding="utf-8") as f:
                payload = json.load(f)
        except Exception:
            return False

        stored_username = payload.get("username")
        if not isinstance(stored_username, str) or not hmac.compare_digest(stored_username, username):
            return False

        salt = bytes.fromhex(payload["salt"])
        iterations = payload.get("iterations")
        if not isinstance(iterations, int) or iterations < 100_000:
            return False
        expected_hash = bytes.fromhex(payload["password_hash"])
        candidate_hash = hashlib.pbkdf2_hmac("sha256", password.encode("utf-8"), salt, iterations)
        return hmac.compare_digest(candidate_hash, expected_hash)

    # --------------------------------------------------------------------
    @staticmethod
    def _write_atomic(path: Path, contents: bytes) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        tmp = path.with_suffix(".tmp")
        try:
            tmp.write_bytes(contents)
            tmp.replace(path)
        finally:
            if tmp.exists():
                try:
                    tmp.unlink()
                except OSError:
                    pass


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
    config_dir: str | Path,
    *,
    cookie_secure: bool = True,
    manual_marker_service: Optional[ManualMarkerService] = None,
    marker_reload: Optional[Callable[[], None]] = None,
) -> Flask:
    app = Flask(__name__)
    admin_store = AdminStore(config_dir)
    config_store = ConfigStore(config_dir)
    app.secret_key = admin_store.session_secret()
    if not admin_store.configured:
        admin_store.setup_token()
    app.permanent_session_lifetime = SESSION_LIFETIME_SECONDS
    app.config.update(
        SESSION_COOKIE_HTTPONLY=True,
        SESSION_COOKIE_SAMESITE="Lax",
        SESSION_COOKIE_SECURE=cookie_secure,
    )
    limiter = LoginLimiter()
    process_metrics = ProcessMetricsSampler()

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
        admin_store.setup_token()
        return render_template("auth.html", title="Criar administrador", setup=True)

    @app.post("/setup")
    def create_admin() -> Any:
        if admin_store.configured:
            return jsonify(error="A conta administrativa já foi configurada."), 409
        try:
            admin_store.create(
                request.form.get("username", ""),
                request.form.get("password", ""),
                request.form.get("setup_token", ""),
            )
        except SetupTokenError as error:
            return jsonify(error=str(error)), 403
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
            return render_template("auth.html", title="Entrar", setup=False,
                                   error="Muitas tentativas. Aguarde antes de tentar novamente."), 429
        username = request.form.get("username", "")
        password = request.form.get("password", "")
        if not admin_store.verify(username, password):
            limiter.record_failure(identity)
            return render_template("auth.html", title="Entrar", setup=False,
                                   error="Usuário ou senha inválidos."), 401
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

    @app.get("/config")
    @authenticated
    def config_page() -> Any:
        cfg = config_store.load()
        # Mask token for display
        masked = cfg.copy()
        token = masked.get("plex_token", "")
        masked["plex_token"] = token[:4] + "****" + token[-4:] if len(token) > 8 else "****"
        return render_template("config.html", config=masked, raw_config=cfg)

    @app.post("/config")
    @authenticated
    def save_config() -> Any:
        form = request.form
        data = {
            "plex_url": form.get("plex_url", "").rstrip("/"),
            "plex_token": form.get("plex_token", ""),
            "auto_skip_intro": form.get("auto_skip_intro") == "on",
            "auto_next_episode": form.get("auto_next_episode") == "on",
            "next_episode_threshold_seconds": int(form.get("next_episode_threshold_seconds", 15)),
            "poll_interval_seconds": int(form.get("poll_interval_seconds", 2)),
            "allowed_client_ids": [x.strip() for x in form.get("allowed_client_ids", "").split(",") if x.strip()],
            "allowed_usernames": [x.strip() for x in form.get("allowed_usernames", "").split(",") if x.strip()],
            "default_intro_skip_seconds": int(form.get("default_intro_skip_seconds", 0)),
            "scan_interval_minutes": int(form.get("scan_interval_minutes", 5)),
            "webhook_secret": form.get("webhook_secret", ""),
            "redis_url": form.get("redis_url", "redis://127.0.0.1:6379/0"),
            "enable_fs_watcher": form.get("enable_fs_watcher") == "on",
        }
        try:
            config_store.save(data)
        except Exception as error:
            logger.exception("Falha ao salvar configuração")
            return jsonify(error=f"Não foi possível salvar a configuração: {error}"), 500
        return redirect(url_for("config_page"))

    @app.get("/")
    @authenticated
    def dashboard() -> Any:
        # Use latest scan job as snapshot if available
        latest_job = None
        with _scan_jobs_lock:
            if _scan_jobs:
                latest_job = max(_scan_jobs.values(), key=lambda j: j.get("section_key", ""))
        snapshot = {
            "status": latest_job.get("status", "idle") if latest_job else "idle",
            "started_at": None,
            "completed_at": None,
            "result": None,
            "error": latest_job.get("error") if latest_job and latest_job.get("status") == "failed" else None,
        }
        # fetch Plex libraries for cards
        plex_libs = []
        cfg = config_store.load()
        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if plex_url and plex_token:
            try:
                manager = PlexManager(plex_url, plex_token)
                plex_libs = manager.list_library_sections()
            except Exception:
                app.logger.exception("Falha ao obter bibliotecas do Plex para dashboard")
                plex_libs = []
        # per-section sync status
        with _section_status_lock:
            sync_status = {lib.get("key"): _section_status.get(lib.get("key"), "idle") for lib in plex_libs}
        # collect show library keys for "scan all"
        show_keys = [lib.get("key") for lib in plex_libs if lib.get("type") == "show"]
        totals = {"episodes": 0, "intros": 0, "credits": 0, "missing": 0}
        return render_template(
            "dashboard.html",
            snapshot=snapshot,
            libraries=[],  # legacy, not used
            totals=totals,
            plex_libraries=plex_libs,
            sync_status=sync_status,
            show_keys=show_keys,
        )

    @app.get("/libraries")
    @authenticated
    def library_manager() -> Any:
        # fetch Plex libraries server-side to avoid layout shift
        plex_libs = []
        cfg = config_store.load()
        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if plex_url and plex_token:
            try:
                manager = PlexManager(plex_url, plex_token)
                plex_libs = manager.list_library_sections()
            except Exception:
                app.logger.exception("Falha ao obter bibliotecas do Plex para library_manager")
                plex_libs = []
        return render_template("library_manager.html", plex_libraries=plex_libs)

    @app.get("/sync")
    @authenticated
    def sync_page() -> Any:
        # fetch Plex libraries for sync page (only show type)
        plex_libs = []
        cfg = config_store.load()
        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if plex_url and plex_token:
            try:
                manager = PlexManager(plex_url, plex_token)
                plex_libs = manager.list_library_sections()
            except Exception:
                app.logger.exception("Falha ao obter bibliotecas do Plex para sync")
                plex_libs = []
        # per-section sync status
        with _section_status_lock:
            sync_status = {lib.get("key"): _section_status.get(lib.get("key"), "idle") for lib in plex_libs}
        return render_template(
            "sync.html",
            plex_libraries=plex_libs,
            sync_status=sync_status,
        )

    

    @app.get("/api/status")
    @authenticated
    def status() -> Any:
        # Return latest scan job status (most recent created_at)
        latest_job = None
        latest_job_id = None
        with _scan_jobs_lock:
            if _scan_jobs:
                # find job with max created_at
                latest_job_id = max(_scan_jobs.keys(), key=lambda k: _scan_jobs[k].get("created_at", 0))
                latest_job = _scan_jobs[latest_job_id]
        payload = {
            "status": latest_job.get("status", "idle") if latest_job else "idle",
            "started_at": None,
            "completed_at": None,
            "result": None,
            "error": latest_job.get("error") if latest_job and latest_job.get("status") == "failed" else None,
        }
        if latest_job_id:
            payload["job_id"] = latest_job_id
        try:
            payload["resources"] = process_metrics.sample()
        except (OSError, ValueError) as error:
            app.logger.exception("Não foi possível coletar métricas do processo.")
            payload["resources"] = {"error": str(error)}
        return jsonify(payload)

    @app.get("/api/scan/log")
    @authenticated
    def scan_log() -> Any:
        """Return logs of the most recent scan job (running or finished)."""
        latest_job = None
        with _scan_jobs_lock:
            if _scan_jobs:
                latest_job = max(_scan_jobs.values(), key=lambda j: j.get("created_at", 0))
        if not latest_job:
            return jsonify(logs=[])
        logs = latest_job.get("logs", [])
        return jsonify(logs=logs)

    @app.post("/api/scan")
    @authenticated
    def start_scan() -> Any:
        # legacy endpoint kept for compatibility; returns 501
        return jsonify(error="Endpoint legado desativado. Use /api/plex/scan."), 501

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

    # ---- Plex library scan jobs (simple in-memory) ----
    _scan_jobs: Dict[str, Dict[str, Any]] = {}
    _scan_jobs_lock = threading.Lock()
    # Per-section sync status: 'idle' | 'running' | 'success' | 'error'
    _section_status: Dict[str, str] = {}
    _section_status_lock = threading.Lock()
    # Lock for custom.json writes
    _CUSTOM_LOCK = threading.Lock()
    # Cancel events per job
    _cancel_events: Dict[str, threading.Event] = {}
    _cancel_events_lock = threading.Lock()

    def _run_scan_job(job_id: str, section_key: str, plex_url: str, plex_token: str):
        # Prepare per-job log queue for SSE streaming
        log_q = queue.Queue()
        class _QueueLogHandler(logging.Handler):
            def __init__(self, q):
                super().__init__()
                self.q = q
                self.setFormatter(logging.Formatter('%(asctime)s %(levelname)s %(name)s: %(message)s'))
            def emit(self, record):
                try:
                    self.q.put(self.format(record))
                except Exception:
                    pass
        handler = _QueueLogHandler(log_q)
        handler.setLevel(logging.INFO)
        scan_logger = logging.getLogger("plex-autoplay.scanner")
        scan_logger.addHandler(handler)

        # store queue and initial logs list
        with _scan_jobs_lock:
            _scan_jobs[job_id]["log_queue"] = log_q
            _scan_jobs[job_id]["logs"] = []

        # create cancel event for this job
        cancel_event = threading.Event()
        with _cancel_events_lock:
            _cancel_events[job_id] = cancel_event

        logger.info("Starting scan job %s for section %s", job_id, section_key)
        # mark as running
        with _scan_jobs_lock:
            _scan_jobs[job_id]["status"] = "running"
        try:
            manager = PlexManager(plex_url, plex_token)
            scanner = PlexLibraryScanner(manager, cancel_event)
            result: ScanResult = scanner.scan_section(section_key)
            # check for cancellation request
            with _scan_jobs_lock:
                if _scan_jobs[job_id].get("cancel_requested") or cancel_event.is_set():
                    _scan_jobs[job_id]["status"] = "cancelled"
                    return
                _scan_jobs[job_id].update({
                    "status": "completed",
                    "processed": result.processed,
                    "skipped": result.skipped,
                    "errors": result.errors,
                })
            with _section_status_lock:
                _section_status[section_key] = "success"
        except Exception as exc:
            logger.exception("Scan job %s failed", job_id)
            with _scan_jobs_lock:
                _scan_jobs[job_id].update({"status": "failed", "error": str(exc)})
            with _section_status_lock:
                _section_status[section_key] = "error"
        finally:
            scan_logger.removeHandler(handler)
            # clean up cancel event
            with _cancel_events_lock:
                _cancel_events.pop(job_id, None)
            # drain queue into logs list for polling endpoint compatibility
            with _scan_jobs_lock:
                logs = []
                while not log_q.empty():
                    logs.append(log_q.get_nowait())
                _scan_jobs[job_id]["logs"] = logs
                _scan_jobs[job_id]["log_queue"] = None

    @app.post("/api/plex/scan")
    @authenticated
    def start_plex_scan() -> Any:
        cfg = config_store.load()
        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if not plex_url or not plex_token:
            return jsonify(error="Plex URL ou Token não configurados."), 400
        data = request.get_json(silent=True) or {}
        section_key = data.get("section_key")
        if not section_key:
            return jsonify(error="section_key é obrigatório."), 400

        # determine total episodes for progress
        manager = PlexManager(plex_url, plex_token)
        episodes = manager.list_episodes(section_key)
        total = len(episodes)

        job_id = f"scan-{uuid.uuid4().hex[:8]}"
        with _scan_jobs_lock:
            _scan_jobs[job_id] = {
                "status": "queued",
                "section_key": section_key,
                "total": total,
                "processed": 0,
                "created_at": time.time(),
            }
        # set per-section status to running
        with _section_status_lock:
            _section_status[section_key] = "running"
        thread = threading.Thread(target=_run_scan_job, args=(job_id, section_key, plex_url, plex_token), daemon=True)
        thread.start()
        return jsonify(job_id=job_id, status="queued", total=total, processed=0), 202

    @app.get("/api/plex/scan/<job_id>")
    @authenticated
    def scan_job_status(job_id: str) -> Any:
        with _scan_jobs_lock:
            job = _scan_jobs.get(job_id)
        if not job:
            return jsonify(error="Job não encontrado."), 404
        # Return serializable fields only
        return jsonify({
            "job_id": job_id,
            "status": job.get("status"),
            "section_key": job.get("section_key"),
            "total": job.get("total"),
            "processed": job.get("processed"),
            "skipped": job.get("skipped"),
            "errors": job.get("errors"),
        })

    @app.get("/api/plex/scan/<job_id>/log")
    @authenticated
    def scan_job_log(job_id: str) -> Any:
        with _scan_jobs_lock:
            job = _scan_jobs.get(job_id)
        if not job:
            return jsonify(error="Job não encontrado."), 404
        logs = job.get("logs", [])
        return jsonify(logs=logs)

    @app.post("/api/plex/scan/<job_id>/cancel")
    @authenticated
    def cancel_scan(job_id: str):
        with _scan_jobs_lock:
            job = _scan_jobs.get(job_id)
            if not job:
                return jsonify(error="Job não encontrado."), 404
            job["cancel_requested"] = True
        # signal the scanner thread to stop as soon as possible
        with _cancel_events_lock:
            ev = _cancel_events.get(job_id)
            if ev:
                ev.set()
        return jsonify(status="cancel requested")

    @app.get("/api/plex/scan/<job_id>/log/stream")
    @authenticated
    def scan_job_log_stream(job_id: str):
        """Server‑Sent Events stream of log lines for a scan job."""
        with _scan_jobs_lock:
            job = _scan_jobs.get(job_id)
        if not job:
            return jsonify(error="Job não encontrado."), 404

        log_q = job.get("log_queue")
        # If job already finished, send buffered logs then close
        if log_q is None:
            def generate_finished():
                for line in job.get("logs", []):
                    yield f"data: {line}\n\n"
                yield "event: close\ndata: done\n\n"
            return Response(stream_with_context(generate_finished()), mimetype="text/event-stream")

        def generate_live():
            # send any lines already queued
            while not log_q.empty():
                yield f"data: {log_q.get_nowait()}\n\n"
            # then wait for new lines
            while True:
                with _scan_jobs_lock:
                    cur = _scan_jobs.get(job_id)
                    if cur and cur.get("status") in ("completed", "failed"):
                        # drain remaining
                        while not log_q.empty():
                            yield f"data: {log_q.get_nowait()}\n\n"
                        yield "event: close\ndata: done\n\n"
                        break
                try:
                    line = log_q.get(timeout=1.0)
                    yield f"data: {line}\n\n"
                except queue.Empty:
                    # heartbeat to keep connection alive
                    yield ": heartbeat\n\n"

        return Response(stream_with_context(generate_live()), mimetype="text/event-stream")

    @app.get("/api/plex/library/<section_key>/episodes")
    @authenticated
    def library_episodes(section_key: str) -> Any:
        cfg = config_store.load()
        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if not plex_url or not plex_token:
            return jsonify(error="Plex não configurado"), 400

        cache_key = f"plex:episodes:{section_key}"
        # try cache
        cached = redis_client.get(cache_key)
        if cached:
            return jsonify(json.loads(cached)), 200, {"X-Cache": "HIT"}

        try:
            manager = PlexManager(plex_url, plex_token)
            episodes = manager.list_episodes_with_markers(section_key)
            # group by series_title
            from collections import defaultdict
            grouped = defaultdict(list)
            for ep in episodes:
                grouped[ep["series_title"] or "Desconhecido"].append(ep)
            series_list = []
            for series_title, eps in grouped.items():
                seasons_map = defaultdict(list)
                for ep in eps:
                    seasons_map[ep["season"]].append(ep)
                season_list = []
                for season_num in sorted(seasons_map.keys()):
                    season_list.append({
                        "season": season_num,
                        "episodes": seasons_map[season_num]
                    })
                # use first episode thumb as series poster
                poster = eps[0]["thumb_url"] if eps else None
                series_list.append({
                    "title": series_title,
                    "poster": poster,
                    "seasons": season_list
                })
            payload = {"series": series_list}
            # store in redis with TTL 300 seconds (5 min)
            redis_client.setex(cache_key, 300, json.dumps(payload))
            return jsonify(payload), 200, {"X-Cache": "MISS"}
        except Exception as exc:
            logger.exception("Erro ao listar episódios da biblioteca %s", section_key)
            return jsonify(error=str(exc)), 500

    # ---------- Sync API ----------
    @app.get("/api/sync/library/<section_key>/tree")
    @authenticated
    def sync_library_tree(section_key: str):
        cfg = config_store.load()
        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if not plex_url or not plex_token:
            return jsonify(error="Plex não configurado"), 400

        cache_key = f"plex:sync_tree:{section_key}"
        cached = redis_client.get(cache_key)
        if cached:
            return jsonify(json.loads(cached)), 200, {"X-Cache": "HIT"}

        manager = PlexManager(plex_url, plex_token)
        episodes = manager.list_episodes_with_markers(section_key)

        from collections import defaultdict
        series_map = defaultdict(lambda: defaultdict(list))
        for ep in episodes:
            series_title = ep.get("series_title") or "Desconhecido"
            season_num = ep.get("season") or 0
            series_map[series_title][season_num].append({
                "rating_key": ep["rating_key"],
                "episode": ep["episode"],
                "title": ep["title"],
                "thumb": ep.get("thumb_url"),
                "intro": ep.get("intro"),
                "credits": ep.get("credits_ms"),
            })

        tree = []
        for s_title, seasons in series_map.items():
            season_list = []
            for s_num in sorted(seasons):
                season_list.append({
                    "season": s_num,
                    "episodes": sorted(seasons[s_num], key=lambda e: e["episode"])
                })
            tree.append({"title": s_title, "seasons": season_list})

        payload = {"section_key": section_key, "series": tree}
        redis_client.setex(cache_key, 300, json.dumps(payload))
        return jsonify(payload), 200, {"X-Cache": "MISS"}

    @app.post("/api/sync/marker")
    @authenticated
    def sync_save_marker():
        data = request.get_json(silent=True) or {}
        required = ("rating_key", "kind", "start_ms", "end_ms")
        if not all(k in data for k in required):
            return jsonify(error="Campos obrigatórios: rating_key, kind, start_ms, end_ms"), 400

        rk = str(data["rating_key"])
        kind = data["kind"]
        try:
            start = int(data["start_ms"])
            end = int(data["end_ms"])
        except (ValueError, TypeError):
            return jsonify(error="start_ms / end_ms devem ser inteiros (ms)"), 400

        if kind not in ("intro", "credits"):
            return jsonify(error="kind deve ser 'intro' ou 'credits'"), 400

        # write to custom.json (shared with PlexAutoSkip)
        CUSTOM_JSON = pathlib.Path("/config/plexautoskip/custom.json")
        with _CUSTOM_LOCK:
            data_json = {}
            if CUSTOM_JSON.is_file():
                try:
                    data_json = json.loads(CUSTOM_JSON.read_text(encoding="utf-8"))
                except json.JSONDecodeError:
                    data_json = {}
            entry = data_json.get(rk, {})
            entry[kind] = {"start_ms": start, "end_ms": end}
            data_json[rk] = entry
            tmp = CUSTOM_JSON.with_suffix(".tmp")
            tmp.write_text(json.dumps(data_json, ensure_ascii=False, indent=2), encoding="utf-8")
            tmp.replace(CUSTOM_JSON)

        return jsonify(ok=True)

    @app.get("/api/plex/stats")
    @authenticated
    def plex_stats() -> Any:
        """Aggregate marker stats across all show libraries."""
        cfg = config_store.load()
        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if not plex_url or not plex_token:
            return jsonify(error="Plex não configurado"), 400

        manager = PlexManager(plex_url, plex_token)
        sections = manager.list_library_sections()

        # Load custom.json (PlexAutoSkip markers)
        CUSTOM_JSON_PATH = Path("/config/plexautoskip/custom.json")
        custom_data = {}
        if CUSTOM_JSON_PATH.is_file():
            try:
                custom_data = json.loads(CUSTOM_JSON_PATH.read_text(encoding="utf-8"))
            except Exception:
                custom_data = {}

        total_eps = 0
        intro_cnt = 0
        credits_cnt = 0
        missing_cnt = 0

        for lib in sections:
            if lib.get("type") != "show":
                continue
            sec_key = lib.get("key")
            # try cache first
            cache_key = f"plex:episodes:{sec_key}"
            cached = redis_client.get(cache_key)
            if cached:
                data = json.loads(cached)
                episodes = []
                for series in data.get("series", []):
                    for season in series.get("seasons", []):
                        episodes.extend(season.get("episodes", []))
            else:
                episodes = manager.list_episodes_with_markers(sec_key)

            for ep in episodes:
                total_eps += 1
                rk = str(ep.get("rating_key"))
                custom_entry = custom_data.get(rk, {})
                has_intro = (ep.get("intro") is not None) or ("intro" in custom_entry)
                has_credits = (ep.get("credits_ms") is not None) or ("credits" in custom_entry)
                if has_intro:
                    intro_cnt += 1
                if has_credits:
                    credits_cnt += 1
                if not has_intro or not has_credits:
                    missing_cnt += 1

        return jsonify({
            "total_episodes": total_eps,
            "intro_episodes": intro_cnt,
            "credits_episodes": credits_cnt,
            "missing_episodes": missing_cnt,
        })

    @app.post("/plex/webhook")
    def plex_webhook() -> Any:
        # Optional signature verification
        cfg = config_store.load()
        secret = cfg.get("webhook_secret", "")
        if secret:
            signature = request.headers.get("X-Plex-Signature", "")
            if not signature.startswith("sha256="):
                return jsonify(error="Assinatura ausente"), 403
            expected = "sha256=" + hmac.new(secret.encode(), request.get_data(), hashlib.sha256).hexdigest()
            if not hmac.compare_digest(expected, signature):
                return jsonify(error="Assinatura inválida"), 403

        payload = request.get_json(silent=True)
        if not payload:
            return jsonify(error="Payload inválido"), 400

        # Expect Plex webhook format: payload is JSON string in form field 'payload'
        # Some implementations send JSON directly. Handle both.
        if isinstance(payload, str):
            import json as _json
            try:
                payload = _json.loads(payload)
            except Exception:
                pass

        # Extract ratingKeys from library.new / library.updated
        rating_keys = []
        event = payload.get("event") if isinstance(payload, dict) else None
        if event in ("library.new", "library.updated"):
            metadata = payload.get("Metadata") or {}
            rk = metadata.get("ratingKey")
            if rk:
                rating_keys.append(str(rk))
        # If multiple items (unlikely), also check 'items'
        for item in payload.get("items", []) if isinstance(payload, dict) else []:
            rk = item.get("ratingKey")
            if rk:
                rating_keys.append(str(rk))

        if not rating_keys:
            return "", 200

        plex_url = cfg.get("plex_url")
        plex_token = cfg.get("plex_token")
        if not plex_url or not plex_token:
            logger.warning("Webhook received but Plex not configured")
            return "", 200

        manager = PlexManager(plex_url, plex_token)
        scanner = PlexLibraryScanner(manager)
        for rk in rating_keys:
            # run inline (could be queued)
            try:
                scanner.process_episode(rk)
            except Exception:
                logger.exception("Webhook processing failed for %s", rk)
        return "", 200

    return app
