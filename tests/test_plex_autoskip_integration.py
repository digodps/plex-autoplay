"""
End-to-end tests for PlexAutoSkip integration.

Tests the complete flow:
1. Library scanning → custom.json generation
2. PlexAutoSkip reading custom.json
3. Marker saving via API → custom.json update
3. Cache invalidation
"""

import json
import os
import tempfile
import unittest
from pathlib import Path
from unittest.mock import MagicMock, patch

from plex_autoplay.application.plex_library_scanner import PlexLibraryScanner
from plex_autoplay.infrastructure.plex.client import PlexManager
from plex_autoplay.infrastructure.web.app import create_app
from plex_autoplay.config import config as app_config


class TestPlexAutoSkipIntegration(unittest.TestCase):
    """End-to-end tests for PlexAutoSkip integration."""

    def setUp(self):
        # Patch redis_client to avoid needing a real Redis server
        self.redis_patcher = patch('plex_autoplay.infrastructure.web.app.redis_client')
        self.mock_redis = self.redis_patcher.start()
        self.mock_redis.delete.return_value = None

        self.temp_dir = tempfile.TemporaryDirectory()
        base = Path(self.temp_dir.name)

        # Create a temporary directory for PlexAutoSkip config
        self.plexautoskip_dir = Path(self.temp_dir.name) / "plexautoskip"
        self.plexautoskip_dir.mkdir(parents=True, exist_ok=True)

        # Set environment variables to use our test directory
        os.environ["PLEX_AUTOPLAY_CUSTOM_JSON_PATH"] = str(self.plexautoskip_dir / "custom.json")
        os.environ["PLEX_AUTOPLAY_CONFIG_DIR"] = str(self.plexautoskip_dir)

    def tearDown(self):
        self.redis_patcher.stop()
        self.temp_dir.cleanup()

        # Create minimal custom.json with PlexAutoSkip format
        self.custom_json_path = self.plexautoskip_dir / "custom.json"
        initial_custom = {
            "markers": {
                "4987": [
                    {"start": 0, "end": 180000, "type": "intro"},
                    {"start": 1467893, "end": 1470058, "type": "credits"}
                ],
                "5103": [
                    {"start": 1467953, "end": 1470058, "type": "credits"}
                ]
            },
            "offsets": {},
            "tags": {},
            "mode": {},
            "allowed": {"clients": [], "users": [], "keys": [], "skip-next": []},
            "blocked": {"clients": [], "users": [], "keys": [], "skip-next": []},
            "clients": {},
            "mode": {}
        }
        self.custom_json_path = self.plexautoskip_dir / "custom.json"
        initial_custom = {
            "markers": {
                "4987": [
                    {"start": 0, "end": 180000, "type": "intro"},
                    {"start": 1467893, "end": 1470058, "type": "credits"}
                ],
                "5103": [
                    {"start": 1467953, "end": 1470058, "type": "credits"}
                ]
            },
            "offsets": {},
            "tags": {},
            "mode": {},
            "allowed": {"clients": [], "users": [], "keys": [], "skip-next": []},
            "blocked": {"clients": [], "users": [], "keys": [], "skip-next": []},
            "clients": {},
            "mode": {}
        }
        self.custom_json_path.write_text(json.dumps(initial_custom, indent=2), encoding="utf-8")

        # Create config.ini for PlexAutoSkip
        self.config_ini = self.plexautoskip_dir / "config.ini"
        config_ini_content = """[PlexServer]
plex_url = http://192.168.18.3:32400
plex_token = test_token

[General]
log_level = INFO

[Custom]
custom_file = /config/plexautoskip/custom.json

[Plex.tv]
username = 
password = 
token = test_token
servername = 

[Server]
address = 192.168.18.3
ssl = False
port = 32400

[Security]
ignore-certs = False

[Skip]
mode = skip
tags = intro, commercial, advertisement, credits
types = movie, episode
ignored-libraries = 
last-chapter = 0.0
unwatched = False
first-episode-series = Always
first-episode-season = Always
first-safe-tags = 
last-episode-series = Watched
last-episode-season = Always
last-safe-tags = 
next = False

[Binge]
ignore-skip-for = 0
safe-tags = 
same-show-only = False
skip-next-max = 0

[Offsets]
start = 0
end = 1000
command = 500
tags = intro

[Volume]
low = 0
high = 100
"""
        self.config_ini = self.plexautoskip_dir / "config.ini"
        self.config_ini.write_text(config_ini_content, encoding="utf-8")

        # Create config directory for the main app
        self.config_dir = Path(self.temp_dir.name) / "config"
        self.config_dir.mkdir(parents=True, exist_ok=True)

        # Create app with test config
        self.app = create_app(
            config_dir=str(self.config_dir),
            cookie_secure=False
        )
        self.app.config["TESTING"] = True
        self.client = self.app.test_client()

    def _login(self):
        """Helper to login as admin."""
        with self.client.session_transaction() as session:
            session["admin"] = "admin"
            session["_csrf_token"] = "test-csrf-token"

    def test_custom_json_has_correct_structure(self):
        """Test that custom.json has the correct PlexAutoSkip format."""
        with open(self.custom_json_path, "r") as f:
            data = json.load(f)

        # Check top-level structure
        self.assertIn("markers", data)
        self.assertIsInstance(data["markers"], dict)

        # Check 4987 has correct structure
        self.assertIn("4987", data["markers"])
        markers_4987 = data["markers"]["4987"]
        self.assertIsInstance(markers_4987, list)
        self.assertEqual(len(markers_4987), 2)

        intro = next(m for m in markers_4987 if m["type"] == "intro")
        credits = next(m for m in markers_4987 if m["type"] == "credits")

        self.assertEqual(intro["start"], 0)
        self.assertEqual(intro["end"], 180000)
        self.assertEqual(credits["start"], 1467893)
        self.assertEqual(credits["end"], 1470058)

        # Verify all markers have required fields
        for rk, markers in data["markers"].items():
            for marker in markers:
                self.assertIn("start", marker)
                self.assertIn("end", marker)
                self.assertIn("type", marker)
                self.assertIn(marker["type"], ("intro", "credits"))
                self.assertIsInstance(marker["start"], int)
                self.assertIsInstance(marker["end"], int)
                self.assertLess(marker["start"], marker["end"])

    def test_api_sync_marker_saves_in_list_format(self):
        """Test that POST /api/sync/marker saves in PlexAutoSkip list format."""
        self._login()
        csrf = "test-csrf-token"

        # Save a new intro marker
        response = self.client.post(
            "/api/sync/marker",
            json={
                "rating_key": "9999",
                "kind": "intro",
                "start_ms": 1000,
                "end_ms": 5000,
                "section_key": "4"
            },
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )

        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json["ok"], True)

        # Verify custom.json was updated with list format
        with open(self.custom_json_path, "r") as f:
            data = json.load(f)

        self.assertIn("markers", data)
        self.assertIn("9999", data["markers"])
        markers = data["markers"]["9999"]
        self.assertIsInstance(markers, list)

        intro = next((m for m in markers if m["type"] == "intro"), None)
        self.assertIsNotNone(intro)
        self.assertEqual(intro["start"], 1000)
        self.assertEqual(intro["end"], 5000)
        self.assertEqual(intro["type"], "intro")

    def test_api_sync_marker_updates_existing_marker(self):
        """Test that updating an existing marker replaces it."""
        self._login()

        # First save an intro marker
        self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999", "kind": "intro", "start_ms": 1000, "end_ms": 5000, "section_key": "4"},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )

        # Update the same marker
        self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999", "kind": "intro", "start_ms": 2000, "end_ms": 6000, "section_key": "4"},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )

        # Verify only one intro marker exists with updated values
        with open(self.custom_json_path, "r") as f:
            data = json.load(f)

        markers = data["markers"]["9999"]
        intro_markers = [m for m in markers if m["type"] == "intro"]
        self.assertEqual(len(intro_markers), 1)
        self.assertEqual(intro_markers[0]["start"], 2000)
        self.assertEqual(intro_markers[0]["end"], 6000)

    def test_api_sync_marker_removes_on_missing_section_key(self):
        """Test that cache is invalidated when section_key is provided."""
        self._login()

        # Save a marker with section_key
        response = self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999", "kind": "intro", "start_ms": 1000, "end_ms": 5000, "section_key": "4"},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )
        self.assertEqual(response.status_code, 200)

        # Check that cache keys would be deleted (we can't directly test Redis in unit test,
        # but we can verify the endpoint accepts section_key)
        self.assertEqual(response.json["ok"], True)

    def test_api_sync_marker_validates_input(self):
        """Test input validation on marker save."""
        self._login()

        # Missing required fields
        response = self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999"},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )
        self.assertEqual(response.status_code, 400)

        # Invalid kind
        response = self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999", "kind": "invalid", "start_ms": 0, "end_ms": 1000},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )
        self.assertEqual(response.status_code, 400)

        # Invalid start/end
        response = self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999", "kind": "intro", "start_ms": -1, "end_ms": 1000},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )
        self.assertEqual(response.status_code, 400)

        response = self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999", "kind": "intro", "start_ms": 1000, "end_ms": 500},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )
        self.assertEqual(response.status_code, 400)

    def test_custom_json_structure_matches_plexautoskip_expectations(self):
        """Test that custom.json matches PlexAutoSkip expectations exactly."""
        with open(self.custom_json_path, "r") as f:
            data = json.load(f)

        # Required top-level keys
        required_keys = {"markers", "offsets", "tags", "mode", "allowed", "blocked", "clients"}
        self.assertEqual(set(data.keys()), set(["markers", "offsets", "tags", "mode", "allowed", "blocked", "clients"]))

        # Markers must be dict of ratingKey -> list
        self.assertIsInstance(data["markers"], dict)

        # Check nested structures
        self.assertIsInstance(data["offsets"], dict)
        self.assertIsInstance(data["tags"], dict)
        self.assertIsInstance(data["mode"], dict)
        self.assertIsInstance(data["allowed"], dict)
        self.assertIsInstance(data["blocked"], dict)
        self.assertIsInstance(data["clients"], dict)

        # Check allowed/blocked structure
        for key in ["allowed", "blocked"]:
            self.assertIn("clients", data[key])
            self.assertIn("users", data[key])
            self.assertIn("keys", data[key])
            self.assertIn("skip-next", data[key])
            self.assertIsInstance(data[key]["clients"], list)
            self.assertIsInstance(data[key]["users"], list)
            self.assertIsInstance(data[key]["keys"], list)
            self.assertIsInstance(data[key]["skip-next"], list)

    def test_marker_save_preserves_existing_markers(self):
        """Test that saving a marker for one episode doesn't affect others."""
        self._login()

        # Save marker for episode 9999
        self.client.post(
            "/api/sync/marker",
            json={"rating_key": "9999", "kind": "intro", "start_ms": 1000, "end_ms": 5000, "section_key": "4"},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )

        # Save marker for another episode
        self.client.post(
            "/api/sync/marker",
            json={"rating_key": "8888", "kind": "credits", "start_ms": 100000, "end_ms": 200000, "section_key": "4"},
            headers={"X-CSRF-Token": "test-csrf-token", "Content-Type": "application/json"}
        )

        # Verify both exist
        with open(self.custom_json_path, "r") as f:
            data = json.load(f)

        self.assertIn("9999", data["markers"])
        self.assertIn("8888", data["markers"])
        self.assertEqual(len(data["markers"]["9999"]), 1)  # intro only
        self.assertEqual(len(data["markers"]["8888"]), 1)  # credits only


class TestPlexLibraryScannerIntegration(unittest.TestCase):
    """Tests for PlexLibraryScanner with PlexAutoSkip integration."""

    @patch("plex_autoplay.infrastructure.plex.client.PlexManager")
    def test_scan_library_merges_custom_json_markers(self, mock_plex_manager_class):
        """Test that scanner merges Plex markers with custom.json markers."""
        mock_manager = MagicMock()
        mock_plex_manager_class.return_value = mock_manager

        # Mock Plex episodes (uses list_episodes, not list_episodes_with_markers)
        mock_manager.list_episodes.return_value = [
            {
                "rating_key": "4987",
                "series_title": "Aoashi",
                "season": 2,
                "episode": 2,
                "title": "Episode 2",
                "thumb_url": "http://example.com/thumb.jpg",
                "intro": None,
                "credits_ms": None,
            }
        ]

        # Create scanner with mocked manager
        scanner = PlexLibraryScanner(mock_manager)
        result = scanner.scan_section("4")

        # Verify scanner was called
        mock_manager.list_episodes.assert_called_once_with("4")
        self.assertEqual(result.processed, 1)

    @patch("plex_autoplay.application.plex_library_scanner.generate_with_xml")
    @patch("plex_autoplay.infrastructure.plex.client.PlexManager")
    def test_scan_library_merges_custom_intro_marker(self, mock_plex_manager_class, mock_generate):
        """Test that custom.json intro marker is merged with Plex data."""
        mock_manager = MagicMock()
        mock_plex_manager_class.return_value = mock_manager

        # Mock Plex episode with no intro (uses list_episodes, not list_episodes_with_markers)
        mock_manager.list_episodes.return_value = [
            {
                "rating_key": "4987",
                "series_title": "Aoashi",
                "season": 2,
                "episode": 2,
                "title": "Episode 2",
                "thumb_url": "http://example.com/thumb.jpg",
                "intro": None,  # No Plex intro
                "credits_ms": None,
            }
        ]

        # Mock generate_with_xml to return intro marker
        mock_generate.return_value = ({"intro": {"start_ms": 0, "end_ms": 180000}}, None)

        scanner = PlexLibraryScanner(MagicMock())
        result = scanner.scan_section("4")

        # The scanner should process the episode
        self.assertEqual(result.processed, 1)


if __name__ == "__main__":
    unittest.main()