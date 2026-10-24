import threading
import unittest

from plex_autoplay.application.scan_coordinator import ScanCoordinator


class TestScanCoordinator(unittest.TestCase):
    def test_runs_once_at_a_time_and_publishes_completed_state(self):
        started = threading.Event()
        release = threading.Event()
        calls = []

        def scan():
            calls.append("scan")
            started.set()
            self.assertTrue(release.wait(2))
            return {"episodes": 3}

        coordinator = ScanCoordinator(scan)
        self.assertTrue(coordinator.start())
        self.assertTrue(started.wait(2))
        self.assertEqual(coordinator.snapshot().status, "running")
        self.assertFalse(coordinator.start())
        release.set()

        result = coordinator.wait(2)

        self.assertEqual(result.status, "completed")
        self.assertEqual(result.result, {"episodes": 3})
        self.assertEqual(len(calls), 1)

    def test_publishes_failed_state_and_allows_retry(self):
        calls = []

        def scan():
            calls.append(None)
            if len(calls) == 1:
                raise RuntimeError("scan failed")
            return {"ok": True}

        coordinator = ScanCoordinator(scan)
        self.assertTrue(coordinator.start())

        failed = coordinator.wait(2)

        self.assertEqual(failed.status, "failed")
        self.assertEqual(failed.error, "scan failed")
        self.assertTrue(coordinator.start())
        self.assertEqual(coordinator.wait(2).status, "completed")


if __name__ == "__main__":
    unittest.main()
