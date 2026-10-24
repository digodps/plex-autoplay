import unittest
from pathlib import Path
from unittest.mock import patch

from plex_autoplay.infrastructure.web.process_metrics import ProcessMetricsSampler


class TestProcessMetricsSampler(unittest.TestCase):
    def test_reads_container_network_counters_and_excludes_loopback(self):
        network_stats = (
            "Inter-| Receive | Transmit\n"
            " face |bytes packets errs drop fifo frame compressed multicast|bytes packets errs drop fifo colls carrier compressed\n"
            "  lo: 100 1 0 0 0 0 0 0 200 2 0 0 0 0 0 0\n"
            " eth0: 3000 3 0 0 0 0 0 0 4000 4 0 0 0 0 0 0\n"
        )

        with patch.object(Path, "read_text", return_value=network_stats):
            received, sent, interface_count = ProcessMetricsSampler._read_network_counters()

        self.assertEqual((received, sent, interface_count), (3000, 4000, 1))

    def test_calculates_download_and_upload_rates_between_samples(self):
        sampler = ProcessMetricsSampler()
        sampler._last_wall_time = 0
        sampler._last_cpu_time = 0
        with (
            patch("plex_autoplay.infrastructure.web.process_metrics.time.monotonic", side_effect=[0.5, 2.5]),
            patch.object(sampler, "_read_network_counters", side_effect=[(1000, 2000, 1), (2000, 2500, 1)]),
        ):
            first_sample = sampler.sample()
            second_sample = sampler.sample()

        self.assertIsNone(first_sample["network_download_bytes_per_second"])
        self.assertIsNone(first_sample["network_upload_bytes_per_second"])
        self.assertEqual(second_sample["network_download_bytes_per_second"], 500.0)
        self.assertEqual(second_sample["network_upload_bytes_per_second"], 250.0)
        self.assertEqual(second_sample["network_received_bytes_total"], 2000)
        self.assertEqual(second_sample["network_sent_bytes_total"], 2500)
        self.assertEqual(second_sample["network_interface_count"], 1)


if __name__ == "__main__":
    unittest.main()
