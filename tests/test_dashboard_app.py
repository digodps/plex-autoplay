import json
import re
import tempfile
import threading
import unittest
from pathlib import Path

from plex_autoplay.application.manual_marker_service import ManualMarkerService
from plex_autoplay.application.scan_coordinator import ScanCoordinator
from plex_autoplay.infrastructure.media.library_repository import LibraryRepository
from plex_autoplay.infrastructure.media.scan_report_repository import ScanReportRepository
from plex_autoplay.infrastructure.web.app import create_app


class TestDashboardApp(unittest.TestCase):
    def setUp(self):
        self.temp_dir = tempfile.TemporaryDirectory()
        base = Path(self.temp_dir.name)
        self.media_root = base / "media"
        (self.media_root / "Anime").mkdir(parents=True)
        self.repository = LibraryRepository(self.media_root, base / "config" / "libraries.json")
        self.markers_file = base / "config" / "markers.json"
        self.coordinator = ScanCoordinator(lambda _progress: {"libraries": []})
        self.app = create_app(
            self.repository,
            base / "config",
            scan_coordinator=self.coordinator,
            manual_marker_service=ManualMarkerService(self.markers_file),
        )
        self.app.config["TESTING"] = True
        self.client = self.app.test_client()

    def tearDown(self):
        self.temp_dir.cleanup()

    def _csrf(self, route):
        response = self.client.get(route)
        match = re.search(r'name="csrf_token" value="([^"]+)"', response.get_data(as_text=True))
        self.assertIsNotNone(match)
        return match.group(1)

    def _setup_admin(self):
        token = self._csrf("/setup")
        response = self.client.post(
            "/setup",
            data={"csrf_token": token, "username": "admin", "password": "valid-password"},
        )
        self.assertEqual(response.status_code, 302)

    def test_first_access_creates_hashed_admin_and_disables_setup(self):
        self.assertEqual(self.client.get("/").status_code, 302)
        token = self._csrf("/setup")
        response = self.client.post(
            "/setup",
            data={"csrf_token": token, "username": "admin", "password": "valid-password"},
        )

        self.assertEqual(response.status_code, 302)
        stored_credentials = (Path(self.temp_dir.name) / "config" / "dashboard-admin.json").read_text()
        self.assertNotIn("valid-password", stored_credentials)
        self.assertEqual(self.client.get("/setup").status_code, 302)
        self.assertTrue((Path(self.temp_dir.name) / "config" / "dashboard-session.key").is_file())

    def test_library_routes_require_authentication_and_csrf(self):
        self.assertEqual(self.client.get("/api/libraries").status_code, 401)
        self._setup_admin()
        self.assertEqual(self.client.get("/api/directories").json, {"directories": ["Anime"]})

        rejected = self.client.post("/api/libraries", json={"path": "Anime", "kind": "anime"})
        self.assertEqual(rejected.status_code, 400)

        with self.client.session_transaction() as session:
            token = session["_csrf_token"]
        invalid = self.client.post(
            "/api/libraries",
            json={"path": "../outside", "kind": "series"},
            headers={"X-CSRF-Token": token},
        )
        self.assertEqual(invalid.status_code, 400)
        created = self.client.post(
            "/api/libraries",
            json={"path": "Anime", "kind": "anime"},
            headers={"X-CSRF-Token": token},
        )
        self.assertEqual(created.status_code, 201)
        self.assertEqual(self.client.get("/api/libraries").json["libraries"][0]["kind"], "anime")

        removed = self.client.delete(
            "/api/libraries",
            json={"path": "Anime"},
            headers={"X-CSRF-Token": token},
        )
        self.assertEqual(removed.status_code, 204)
        self.assertEqual(self.client.get("/api/libraries").json["libraries"], [])
        self.assertTrue((self.media_root / "Anime").is_dir())

    def test_login_restores_session_and_logout_removes_it(self):
        self._setup_admin()
        self.client.post("/logout", headers={"X-CSRF-Token": self._csrf_from_session()})
        self.assertEqual(self.client.get("/api/libraries").status_code, 401)

        token = self._csrf("/login")
        response = self.client.post(
            "/login",
            data={"csrf_token": token, "username": "admin", "password": "valid-password"},
        )
        self.assertEqual(response.status_code, 302)
        self.assertEqual(self.client.get("/api/libraries").status_code, 200)
        self.assertTrue(self.app.config["SESSION_COOKIE_HTTPONLY"])
        self.assertEqual(self.app.config["SESSION_COOKIE_SAMESITE"], "Lax")
        self.assertEqual(self.app.permanent_session_lifetime.total_seconds(), 12 * 60 * 60)

    def test_secure_cookie_can_be_enabled_for_https_deployments(self):
        secure_app = create_app(
            self.repository,
            Path(self.temp_dir.name) / "config",
            cookie_secure=True,
        )
        self.assertTrue(secure_app.config["SESSION_COOKIE_SECURE"])

    def test_scan_endpoint_returns_progress_and_completion(self):
        self._setup_admin()
        response = self.client.post("/api/scan", headers={"X-CSRF-Token": self._csrf_from_session()})
        self.assertEqual(response.status_code, 202)

        completed = self.coordinator.wait(2)

        self.assertEqual(completed.status, "completed")
        self.assertEqual(self.client.get("/api/status").json["result"], {"libraries": []})
        resources = self.client.get("/api/status").json["resources"]
        self.assertGreater(resources["memory_bytes"], 0)
        script = self.client.get("/static/dashboard.js")
        self.assertEqual(script.status_code, 200)
        self.assertIn("/api/status", script.get_data(as_text=True))
        script.close()

    def test_dashboard_navigation_targets_exist_without_scan_results(self):
        self._setup_admin()

        response = self.client.get("/")
        page = response.get_data(as_text=True)

        self.assertEqual(response.status_code, 200)
        self.assertIn('href="#activity"', page)
        self.assertEqual(page.count('id="activity"'), 1)
        self.assertIn("Nenhuma pendência ou erro registrado", page)
        self.assertIn("CPU do serviço", page)
        self.assertIn("Eventos recentes", page)
        self.assertIn('id="activity-progress"', page)

    def test_dashboard_activity_updates_with_live_scan_progress(self):
        started = threading.Event()
        release = threading.Event()

        def scan(report_progress):
            report_progress(
                {
                    "stage": "inventory",
                    "message": "Varrendo arquivos em Anime/Temporada 1.",
                    "library": "Anime",
                    "current_directory": "Temporada 1",
                    "directories_scanned": 3,
                    "files_scanned": 8,
                    "completed_libraries": 0,
                    "total_libraries": 1,
                }
            )
            started.set()
            release.wait(2)
            return {"libraries": []}

        coordinator = ScanCoordinator(scan)
        app = create_app(
            self.repository,
            Path(self.temp_dir.name) / "config",
            scan_coordinator=coordinator,
        )
        client = app.test_client()
        token = re.search(r'name="csrf_token" value="([^"]+)"', client.get("/setup").get_data(as_text=True)).group(1)
        client.post(
            "/setup",
            data={"csrf_token": token, "username": "admin", "password": "valid-password"},
        )

        self.assertTrue(coordinator.start())
        try:
            self.assertTrue(started.wait(2))
            status = client.get("/api/status").json
        finally:
            release.set()
            coordinator.wait(2)

        self.assertEqual(status["progress"]["message"], "Varrendo arquivos em Anime/Temporada 1.")
        self.assertEqual(status["progress"]["files_scanned"], 8)
        self.assertTrue(status["progress"]["events"])
        self.assertGreater(status["resources"]["memory_bytes"], 0)
        page = client.get("/").get_data(as_text=True)
        self.assertIn("Varrendo arquivos em Anime/Temporada 1.", page)

    def test_dashboard_shows_configured_libraries_while_scan_is_running(self):
        self.repository.add_library("Anime", "anime")
        scan_gate = threading.Event()
        coordinator = ScanCoordinator(lambda _progress: (scan_gate.wait(2), {"libraries": []})[1])
        app = create_app(
            self.repository,
            Path(self.temp_dir.name) / "config",
            scan_coordinator=coordinator,
        )
        client = app.test_client()
        token = re.search(r'name="csrf_token" value="([^"]+)"', client.get("/setup").get_data(as_text=True)).group(1)
        client.post(
            "/setup",
            data={"csrf_token": token, "username": "admin", "password": "valid-password"},
        )
        self.assertTrue(coordinator.start())
        try:
            page = client.get("/").get_data(as_text=True)
        finally:
            scan_gate.set()
            coordinator.wait(2)

        self.assertIn("Anime", page)
        self.assertIn("0 episódios", page)
        self.assertNotIn("Nenhuma biblioteca cadastrada", page)

    def test_scan_endpoint_rejects_concurrent_requests(self):
        scan_gate = threading.Event()
        coordinator = ScanCoordinator(lambda _progress: (scan_gate.wait(3), {"libraries": []})[1])
        app = create_app(
            self.repository,
            Path(self.temp_dir.name) / "config",
            scan_coordinator=coordinator,
            manual_marker_service=ManualMarkerService(self.markers_file),
        )
        client = app.test_client()
        token = re.search(r'name="csrf_token" value="([^"]+)"', client.get("/setup").get_data(as_text=True)).group(1)
        client.post(
            "/setup",
            data={"csrf_token": token, "username": "admin", "password": "valid-password"},
        )
        with client.session_transaction() as session:
            csrf = session["_csrf_token"]

        first = client.post("/api/scan", headers={"X-CSRF-Token": csrf})
        second = client.post("/api/scan", headers={"X-CSRF-Token": csrf})
        scan_gate.set()

        self.assertEqual(first.status_code, 202)
        self.assertEqual(second.status_code, 409)
        self.assertIn("andamento", second.json["error"])
        self.assertEqual(coordinator.wait(2).status, "completed")

    def test_library_manager_is_authenticated_and_renders_controls(self):
        response = self.client.get("/libraries")
        self.assertEqual(response.status_code, 302)
        self.assertIn("/login", response.headers["Location"])
        self._setup_admin()

        response = self.client.get("/libraries")

        self.assertEqual(response.status_code, 200)
        page = response.get_data(as_text=True)
        self.assertIn("Adicionar pasta como biblioteca", page)
        self.assertIn("Bibliotecas cadastradas", page)
        self.assertIn("dashboard.js", page)

    def test_marker_api_requires_confirmation_and_supports_selected_removal(self):
        self._setup_admin()
        csrf = self._csrf_from_session()
        url = "/api/markers"
        created = self.client.put(
            url,
            json={"series": "Example", "season": 1, "episode": 2, "values": {"start": 4, "end": 24}},
            headers={"X-CSRF-Token": csrf},
        )
        self.assertEqual(created.status_code, 200)

        conflict = self.client.put(
            url,
            json={"series": "Example", "season": 1, "episode": 2, "values": {"start": 5}},
            headers={"X-CSRF-Token": csrf},
        )
        self.assertEqual(conflict.status_code, 409)
        self.assertTrue(conflict.json["confirmation_required"])

        removed = self.client.delete(
            url,
            json={"series": "Example", "season": 1, "episode": 2, "fields": ["end"]},
            headers={"X-CSRF-Token": csrf},
        )
        self.assertEqual(removed.status_code, 200)
        self.assertEqual(removed.json["markers"], {"start": 4.0})

    def test_manual_marker_edits_refresh_panel_and_remove_only_selected_fields(self):
        report = {
            "libraries": [
                {
                    "name": "TV",
                    "path": "TV",
                    "kind": "series",
                    "episode_count": 1,
                    "with_intro": 0,
                    "with_credits": 0,
                    "episodes": [
                        {
                            "series": "Example Show",
                            "season": 1,
                            "episode": 2,
                            "path": "/media/TV/Example Show/S01E02.mkv",
                            "intro": {"start_ms": None, "end_ms": None},
                            "credits_ms": None,
                        }
                    ],
                    "missing": [],
                    "ambiguities": [],
                    "pending": [],
                    "errors": [],
                }
            ]
        }
        report_file = Path(self.temp_dir.name) / "config" / "scan-report.json"
        coordinator = ScanCoordinator(lambda _progress: report, ScanReportRepository(report_file))
        coordinator.start()
        coordinator.wait(2)
        app = create_app(
            self.repository,
            Path(self.temp_dir.name) / "config",
            scan_coordinator=coordinator,
            manual_marker_service=ManualMarkerService(self.markers_file),
        )
        client = app.test_client()
        token = re.search(r'name="csrf_token" value="([^"]+)"', client.get("/setup").get_data(as_text=True)).group(1)
        client.post(
            "/setup",
            data={"csrf_token": token, "username": "admin", "password": "valid-password"},
        )
        with client.session_transaction() as session:
            csrf = session["_csrf_token"]

        saved = client.put(
            "/api/markers",
            json={
                "series": "Example Show",
                "season": 1,
                "episode": 2,
                "values": {"start": 2, "end": 20, "credits": 80},
            },
            headers={"X-CSRF-Token": csrf},
        )

        self.assertEqual(saved.status_code, 200)
        self.assertTrue(saved.json["panel_updated"])
        page = client.get("/").get_data(as_text=True)
        self.assertIn('id="intro-episodes">1</strong>', page)
        self.assertIn('id="credits-episodes">1</strong>', page)
        self.assertIn('name="start" type="number" min="0" step="any" value="2.000"', page)
        persisted = json.loads(report_file.read_text(encoding="utf-8"))
        self.assertEqual(persisted["result"]["libraries"][0]["with_credits"], 1)

        removed = client.delete(
            "/api/markers",
            json={"series": "Example Show", "season": 1, "episode": 2, "fields": ["credits"]},
            headers={"X-CSRF-Token": csrf},
        )

        self.assertEqual(removed.status_code, 200)
        self.assertEqual(removed.json["markers"], {"start": 2.0, "end": 20.0})
        page_after_removal = client.get("/").get_data(as_text=True)
        self.assertIn('id="intro-episodes">1</strong>', page_after_removal)
        self.assertIn('id="credits-episodes">0</strong>', page_after_removal)
        self.assertIn('id="missing-episodes">1</strong>', page_after_removal)
        persisted_after_removal = json.loads(report_file.read_text(encoding="utf-8"))
        self.assertEqual(persisted_after_removal["result"]["libraries"][0]["with_credits"], 0)

    def test_dashboard_renders_empty_state_and_responsive_styles(self):
        self._setup_admin()
        response = self.client.get("/")

        self.assertEqual(response.status_code, 200)
        page = response.get_data(as_text=True)
        self.assertIn("Nenhuma biblioteca cadastrada", page)
        self.assertIn("Plex Autoplay", page)
        self.assertNotIn("TV Library", page)
        self.assertIn("name=\"csrf-token\"", page)
        styles = self.client.get("/static/dashboard.css")
        self.assertEqual(styles.status_code, 200)
        self.assertIn("@media (max-width: 720px)", styles.get_data(as_text=True))
        styles.close()

    def test_dashboard_navigates_from_library_to_series_season_and_episode(self):
        report = {
            "libraries": [
                {
                    "name": "TV",
                    "path": "TV",
                    "kind": "series",
                    "episode_count": 1,
                    "with_intro": 1,
                    "with_credits": 0,
                    "episodes": [
                        {
                            "series": "Example Show",
                            "season": 2,
                            "episode": 4,
                            "path": "/media/TV/Example Show/Season 2/Show.S02E04.mkv",
                            "intro": {"start_ms": 1200, "end_ms": 9000},
                            "credits_ms": None,
                        }
                    ],
                    "missing": [{"fields": ["credits"]}],
                    "ambiguities": [],
                    "pending": [],
                    "errors": ["Falha ao ler pasta de mídia."],
                }
            ]
        }
        coordinator = ScanCoordinator(lambda _progress: report)
        coordinator.start()
        coordinator.wait(2)
        app = create_app(
            self.repository,
            Path(self.temp_dir.name) / "config",
            scan_coordinator=coordinator,
            manual_marker_service=ManualMarkerService(self.markers_file),
        )
        client = app.test_client()
        setup_token = re.search(r'name="csrf_token" value="([^"]+)"', client.get("/setup").get_data(as_text=True)).group(1)
        client.post(
            "/setup",
            data={"csrf_token": setup_token, "username": "admin", "password": "valid-password"},
        )

        page = client.get("/").get_data(as_text=True)

        self.assertIn("TV", page)
        self.assertIn("Example Show", page)
        self.assertIn("Temporada 2", page)
        self.assertIn("S02E04.mkv", page)
        self.assertIn("Com créditos", page)
        self.assertIn("Falha ao ler pasta de mídia.", page)

    def test_invalid_login_is_limited(self):
        self._setup_admin()
        self.client.post("/logout", headers={"X-CSRF-Token": self._csrf_from_session()})
        token = self._csrf("/login")
        last_response = None
        for _ in range(6):
            last_response = self.client.post(
                "/login",
                data={"csrf_token": token, "username": "admin", "password": "wrong-password"},
            )
        self.assertEqual(last_response.status_code, 429)

    def _csrf_from_session(self):
        with self.client.session_transaction() as session:
            return session["_csrf_token"]


if __name__ == "__main__":
    unittest.main()
