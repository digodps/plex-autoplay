import tempfile
import unittest
from pathlib import Path

from plex_autoplay.application.scan_coordinator import ScanCoordinator
from plex_autoplay.infrastructure.media.scan_report_repository import ScanReportRepository


class TestScanReportRepository(unittest.TestCase):
    def test_rejects_invalid_report_without_replacing_last_valid_version(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            repository = ScanReportRepository(Path(temp_dir) / "scan.json")
            previous = {"status": "completed", "result": {"episodes": 4}}
            repository.save(previous)

            with self.assertRaises(ValueError):
                repository.save({"status": "completed", "invalid": {1, 2, 3}})

            self.assertEqual(repository.load(), previous)

    def test_reports_corrupt_json_as_an_explicit_error(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            report_path = Path(temp_dir) / "scan.json"
            report_path.write_text("{", encoding="utf-8")

            with self.assertRaisesRegex(ValueError, "Não foi possível ler"):
                ScanReportRepository(report_path).load()

    def test_coordinator_persists_results_and_restores_latest_report(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            repository = ScanReportRepository(Path(temp_dir) / "scan.json")
            coordinator = ScanCoordinator(lambda: {"libraries": [{"episodes": 2}]}, repository)
            self.assertTrue(coordinator.start())

            result = coordinator.wait(2)
            restored = ScanCoordinator(lambda: {}, repository).snapshot()

            self.assertEqual(result.status, "completed")
            self.assertEqual(repository.load()["result"]["libraries"][0]["episodes"], 2)
            self.assertEqual(restored.status, "completed")
            self.assertEqual(restored.result, {"libraries": [{"episodes": 2}]})


if __name__ == "__main__":
    unittest.main()
