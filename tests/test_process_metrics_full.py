"""Extended tests for ProcessMetricsSampler to achieve 100% coverage."""

from unittest.mock import patch

import pytest

from plex_autoplay.infrastructure.web.process_metrics import ProcessMetricsSampler


class TestProcessMetricsSamplerFull:
    """Extended tests for ProcessMetricsSampler covering edge cases."""

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    @patch("plex_autoplay.infrastructure.web.process_metrics.os.sysconf")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.monotonic")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.process_time")
    def test_sample_memory_read_error(self, mock_process_time, mock_monotonic, mock_sysconf, mock_read_text):
        """Test sample handles memory read error (line 37)."""
        mock_monotonic.return_value = 100.0
        mock_process_time.return_value = 50.0
        mock_sysconf.return_value = 4096
        mock_read_text.side_effect = [FileNotFoundError("No /proc/self/statm")]

        sampler = ProcessMetricsSampler()
        with pytest.raises(FileNotFoundError):
            sampler.sample()

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    @patch("plex_autoplay.infrastructure.web.process_metrics.os.sysconf")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.monotonic")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.process_time")
    def test_sample_insufficient_memory_fields(self, mock_process_time, mock_monotonic, mock_sysconf, mock_read_text):
        """Test sample handles insufficient memory fields (line 37)."""
        mock_monotonic.return_value = 100.0
        mock_process_time.return_value = 50.0
        mock_sysconf.return_value = 4096
        mock_read_text.side_effect = ["1"]  # Only one field

        sampler = ProcessMetricsSampler()
        with pytest.raises(ValueError, match="memória"):
            sampler.sample()

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    @patch("plex_autoplay.infrastructure.web.process_metrics.os.sysconf")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.monotonic")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.process_time")
    def test_sample_network_download_counter_wraparound(self, mock_process_time, mock_monotonic, mock_sysconf, mock_read_text):
        """Test sample handles network download counter wrap (line 51)."""
        mock_monotonic.side_effect = [100.0, 101.0]  # First call for CPU, second for network
        mock_process_time.return_value = 50.0
        mock_sysconf.return_value = 4096
        # First sample (statm), second sample (net/dev)
        mock_read_text.side_effect = [
            "100 50 10 10 10 10 10",  # /proc/self/statm
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "  eth0: 1000    100    0    0    0     0          0         0  2000    100    0    0    0     0       0          0\n",
            "100 50 10 10 10 10 10",  # Second call statm
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "  eth0: 500    100    0    0    0     0          0         0  2500    100    0    0    0     0       0          0\n",  # Counter wrapped (500 < 1000)
        ]

        sampler = ProcessMetricsSampler()
        # First sample
        result1 = sampler.sample()
        # Second sample - download counter wrapped
        result2 = sampler.sample()
        # Should be None due to wrap
        assert result2["network_download_bytes_per_second"] is None
        assert result2["network_upload_bytes_per_second"] == 5000.0  # (2500-2000)/0.1 = 5000

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    @patch("plex_autoplay.infrastructure.web.process_metrics.os.sysconf")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.monotonic")
    @patch("plex_autoplay.infrastructure.web.process_metrics.time.process_time")
    def test_sample_network_upload_counter_wraparound(self, mock_process_time, mock_monotonic, mock_sysconf, mock_read_text):
        """Test sample handles network upload counter wrap (line 58)."""
        mock_monotonic.side_effect = [100.0, 101.0]
        mock_process_time.return_value = 50.0
        mock_sysconf.return_value = 4096
        mock_read_text.side_effect = [
            "100 50 10 10 10 10 10",
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "  eth0: 1000    100    0    0    0     0          0         0  2000    100    0    0    0     0       0          0\n",
            "100 50 10 10 10 10 10",
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "  eth0: 1500    100    0    0    0     0          0         0  1500    100    0    0    0     0       0          0\n",  # Upload wrapped (1500 < 2000)
        ]

        sampler = ProcessMetricsSampler()
        result1 = sampler.sample()
        result2 = sampler.sample()
        assert result2["network_upload_bytes_per_second"] is None
        assert result2["network_download_bytes_per_second"] == 5000.0

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    def test_read_network_counters_skips_loopback(self, mock_read_text):
        """Test _read_network_counters skips 'lo' interface (line 85-86)."""
        mock_read_text.return_value = (
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "    lo: 1000    100    0    0    0     0          0         0  2000    100    0    0    0     0       0          0\n"
            "  eth0: 5000    100    0    0    0     0          0         0  6000    100    0    0    0     0       0          0\n"
        )
        sampler = ProcessMetricsSampler()
        received, sent, count = sampler._read_network_counters()
        assert received == 5000
        assert sent == 6000
        assert count == 1  # Only eth0 counted

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    def test_read_network_counters_invalid_counters(self, mock_read_text):
        """Test _read_network_counters raises on invalid counters (line 89)."""
        mock_read_text.return_value = (
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "  eth0: 1000    100\n"  # Not enough values
        )
        sampler = ProcessMetricsSampler()
        with pytest.raises(ValueError, match="Contadores de rede inválidos"):
            sampler._read_network_counters()

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    def test_read_network_counters_empty_lines(self, mock_read_text):
        """Test _read_network_counters handles empty lines gracefully."""
        mock_read_text.return_value = (
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "\n"
            "  eth0: 1000    100    0    0    0     0          0         0  2000    100    0    0    0     0       0          0\n"
        )
        sampler = ProcessMetricsSampler()
        received, sent, count = sampler._read_network_counters()
        assert received == 1000
        assert sent == 2000
        assert count == 1

    @patch("plex_autoplay.infrastructure.web.process_metrics.Path.read_text")
    def test_read_network_counters_no_separator(self, mock_read_text):
        """Test _read_network_counters skips lines without ':' (line 82-83)."""
        mock_read_text.return_value = (
            "Inter-|   Receive                                                |  Transmit\n"
            " face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed\n"
            "invalid line without colon\n"
            "  eth0: 1000    100    0    0    0     0          0         0  2000    100    0    0    0     0       0          0\n"
        )
        sampler = ProcessMetricsSampler()
        received, sent, count = sampler._read_network_counters()
        assert received == 1000
        assert sent == 2000
        assert count == 1