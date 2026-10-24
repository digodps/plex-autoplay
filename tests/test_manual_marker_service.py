import json
import tempfile
import unittest
from pathlib import Path

from plex_autoplay.application.manual_marker_service import ManualMarkerService, MarkerConflictError


class TestManualMarkerService(unittest.TestCase):
    def test_reads_episode_values_with_defaults_overridden_by_episode_fields(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            path = Path(temp_dir) / "markers.json"
            path.write_text(
                json.dumps(
                    {
                        "Example": {
                            "default": {"start": 1, "end": 10, "credits": 60},
                            "season_1": {"2": {"start": 3}},
                        }
                    }
                ),
                encoding="utf-8",
            )

            values = ManualMarkerService(path).get_values("example", 1, 2)

            self.assertEqual(values, {"start": 3.0, "end": 10.0, "credits": 60.0})

    def test_updates_only_supplied_fields_and_preserves_existing_format(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            path = Path(temp_dir) / "markers.json"
            path.write_text(
                json.dumps(
                    {
                        "Existing Show": {
                            "default": {"start": 1, "end": 3},
                            "season_1": {"1": {"start": 2, "end": 20, "credits": 80}},
                        },
                        "Unrelated Show": {"season_1": {"1": {"credits": 90}}},
                    }
                ),
                encoding="utf-8",
            )
            service = ManualMarkerService(path)
            with self.assertRaises(MarkerConflictError):
                service.update("existing show", 1, 1, {"start": 4})

            updated = service.update("existing show", 1, 1, {"start": 4}, confirm_replace=True)
            data = json.loads(path.read_text(encoding="utf-8"))

            self.assertEqual(updated, {"start": 4.0, "end": 20.0, "credits": 80.0})
            self.assertEqual(data["Existing Show"]["default"], {"start": 1, "end": 3})
            self.assertEqual(data["Unrelated Show"], {"season_1": {"1": {"credits": 90}}})

    def test_validates_values_and_preserves_last_valid_file(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            path = Path(temp_dir) / "markers.json"
            path.write_text("{}", encoding="utf-8")
            service = ManualMarkerService(path)
            original = path.read_text(encoding="utf-8")
            for values in ({"start": True}, {"start": float("inf")}, {"start": -1}, {"start": 10, "end": 5}):
                with self.subTest(values=values), self.assertRaises(ValueError):
                    service.update("Example", 1, 1, values)
            self.assertEqual(path.read_text(encoding="utf-8"), original)

    def test_removes_only_selected_fields_and_fails_on_invalid_json(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            path = Path(temp_dir) / "markers.json"
            path.write_text(
                json.dumps({"Example": {"season_1": {"2": {"start": 1, "end": 2, "credits": 50}}}}),
                encoding="utf-8",
            )
            service = ManualMarkerService(path)

            remaining = service.remove_fields("Example", 1, 2, ["credits"])
            self.assertEqual(remaining, {"start": 1.0, "end": 2.0})
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual(data["Example"]["season_1"]["2"], {"start": 1, "end": 2})

            path.write_text("{", encoding="utf-8")
            with self.assertRaises(ValueError):
                service.update("Example", 1, 2, {"start": 3})
            self.assertEqual(path.read_text(encoding="utf-8"), "{")


if __name__ == "__main__":
    unittest.main()
