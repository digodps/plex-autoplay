"""Unit tests for marker_generator – mocks ffprobe/ffmpeg subprocess calls."""

import unittest
from unittest.mock import MagicMock, patch

from plex_autoplay.application.marker_generator import (
    MarkerResult,
    _build_plex_xml,
    _detect_silence_intervals,
    _get_duration,
    _run_ffprobe,
    generate,
    generate_markers,
    generate_with_xml,
)


class TestMarkerGenerator(unittest.TestCase):
    """Tests for marker detection logic with mocked ffprobe/ffmpeg."""

    @patch("plex_autoplay.application.marker_generator.subprocess.run")
    def test_run_ffprobe_success(self, mock_run):
        """Test _run_ffprobe returns parsed JSON on success."""
        mock_run.return_value = MagicMock(
            stdout='{"format": {"duration": "120.5"}, "streams": []}',
            stderr="",
            returncode=0,
        )
        result = _run_ffprobe("/fake/path/video.mkv")
        self.assertEqual(result["format"]["duration"], "120.5")
        mock_run.assert_called_once()

    @patch("plex_autoplay.application.marker_generator.subprocess.run")
    def test_run_ffprobe_failure(self, mock_run):
        """Test _run_ffprobe raises RuntimeError on failure."""
        mock_run.side_effect = Exception("ffprobe not found")
        with self.assertRaises(RuntimeError) as ctx:
            _run_ffprobe("/fake/path/video.mkv")
        self.assertIn("ffprobe error", str(ctx.exception))

    @patch("plex_autoplay.application.marker_generator._run_ffprobe")
    def test_get_duration_from_format(self, mock_ffprobe):
        """Test _get_duration reads from format.duration."""
        mock_ffprobe.return_value = {"format": {"duration": "150.0"}, "streams": []}
        duration = _get_duration("/fake/path/video.mkv")
        self.assertEqual(duration, 150.0)

    @patch("plex_autoplay.application.marker_generator._run_ffprobe")
    def test_get_duration_from_video_stream(self, mock_ffprobe):
        """Test _get_duration falls back to video stream duration."""
        mock_ffprobe.return_value = {
            "format": {},
            "streams": [{"codec_type": "video", "duration": "200.0"}],
        }
        duration = _get_duration("/fake/path/video.mkv")
        self.assertEqual(duration, 200.0)

    @patch("plex_autoplay.application.marker_generator._run_ffprobe")
    def test_get_duration_zero_when_unavailable(self, mock_ffprobe):
        """Test _get_duration returns 0 when no duration found."""
        mock_ffprobe.return_value = {"format": {}, "streams": []}
        duration = _get_duration("/fake/path/video.mkv")
        self.assertEqual(duration, 0.0)

    @patch("plex_autoplay.application.marker_generator.subprocess.run")
    def test_detect_silence_intervals_parses_output(self, mock_run):
        """Test _detect_silence_intervals parses ffmpeg silencedetect output."""
        # ffmpeg outputs silence info to stderr
        mock_run.return_value = MagicMock(
            stderr="""
[silencedetect @ 0x...] silence_start: 10.5
[silencedetect @ 0x...] silence_end: 13.2
[silencedetect @ 0x...] silence_start: 20.0
[silencedetect @ 0x...] silence_end: 22.5
            """,
            returncode=0,
        )
        intervals = _detect_silence_intervals("/fake/video.mkv", 0.0, 30.0)
        self.assertEqual(len(intervals), 2)
        self.assertAlmostEqual(intervals[0][0], 10.5)
        self.assertAlmostEqual(intervals[0][1], 13.2)
        self.assertAlmostEqual(intervals[1][0], 20.0)
        self.assertAlmostEqual(intervals[1][1], 22.5)

    @patch("plex_autoplay.application.marker_generator.subprocess.run")
    def test_detect_silence_intervals_empty_on_error(self, mock_run):
        """Test _detect_silence_intervals returns empty list on subprocess error."""
        mock_run.side_effect = Exception("ffmpeg failed")
        intervals = _detect_silence_intervals("/fake/video.mkv", 0.0, 30.0)
        self.assertEqual(intervals, [])

    @patch("plex_autoplay.application.marker_generator._get_duration")
    @patch("plex_autoplay.application.marker_generator._detect_silence_intervals")
    def test_generate_markers_detects_intro(self, mock_silence, mock_duration):
        """Test generate_markers finds intro silence in first 5 minutes."""
        mock_duration.return_value = 3600.0  # 1 hour
        # Silence at 30s-33s (3s silence) within first 5 min
        mock_silence.side_effect = [
            [(30.0, 33.0)],  # intro window
            [],  # credits window (last 10%)
        ]
        res = generate_markers("/fake/video.mkv")
        self.assertIsNotNone(res.intro_start)
        self.assertIsNotNone(res.intro_end)
        self.assertEqual(res.intro_start, 30000)  # ms
        self.assertEqual(res.intro_end, 33000)
        self.assertIsNone(res.credits_start)

    @patch("plex_autoplay.application.marker_generator._get_duration")
    @patch("plex_autoplay.application.marker_generator._detect_silence_intervals")
    def test_generate_markers_detects_credits(self, mock_silence, mock_duration):
        """Test generate_markers finds credits silence in last 10%."""
        mock_duration.return_value = 3600.0
        # Silence at 3300s-3305s (5s silence) in last 10%
        mock_silence.side_effect = [
            [],  # intro window
            [(3300.0, 3305.0)],  # credits window
        ]
        res = generate_markers("/fake/video.mkv")
        self.assertIsNone(res.intro_start)
        self.assertIsNotNone(res.credits_start)
        self.assertEqual(res.credits_start, 3300000)
        self.assertEqual(res.credits_end, 3305000)

    @patch("plex_autoplay.application.marker_generator._get_duration")
    @patch("plex_autoplay.application.marker_generator._detect_silence_intervals")
    def test_generate_markers_ignores_short_silence(self, mock_silence, mock_duration):
        """Test generate_markers ignores silence < 2 seconds."""
        mock_duration.return_value = 3600.0
        # Only 1 second silence - should be ignored
        mock_silence.side_effect = [
            [(10.0, 10.5)],  # 0.5s silence
            [(3300.0, 3301.0)],  # 1s silence
        ]
        res = generate_markers("/fake/video.mkv")
        self.assertIsNone(res.intro_start)
        self.assertIsNone(res.credits_start)

    @patch("plex_autoplay.application.marker_generator._get_duration")
    @patch("plex_autoplay.application.marker_generator._detect_silence_intervals")
    def test_generate_markers_empty_file_path(self, mock_silence, mock_duration):
        """Test generate_markers returns empty result for empty file path."""
        res = generate_markers("")
        self.assertIsNone(res.intro_start)
        self.assertIsNone(res.credits_start)

    @patch("plex_autoplay.application.marker_generator._get_duration")
    @patch("plex_autoplay.application.marker_generator._detect_silence_intervals")
    def test_generate_markers_zero_duration(self, mock_silence, mock_duration):
        """Test generate_markers returns empty when duration is 0."""
        mock_duration.return_value = 0.0
        res = generate_markers("/fake/video.mkv")
        self.assertIsNone(res.intro_start)
        self.assertIsNone(res.credits_start)

    @patch("plex_autoplay.application.marker_generator.generate_markers")
    def test_generate_returns_dict_format(self, mock_gen_markers):
        """Test generate() returns legacy dict format."""
        mock_gen_markers.return_value = MarkerResult(
            intro_start=10000,
            intro_end=20000,
            credits_start=3500000,
            credits_end=3600000,
        )
        result = generate("/fake/video.mkv")
        self.assertIn("intro", result)
        self.assertIn("credits", result)
        self.assertEqual(result["intro"]["start"], 10000)
        self.assertEqual(result["intro"]["end"], 20000)
        self.assertEqual(result["credits"]["start"], 3500000)
        self.assertEqual(result["credits"]["end"], 3600000)

    @patch("plex_autoplay.application.marker_generator.generate_markers")
    def test_generate_with_xml_returns_tuple(self, mock_gen_markers):
        """Test generate_with_xml returns (dict, xml_string) tuple."""
        mock_gen_markers.return_value = MarkerResult(
            intro_start=10000,
            intro_end=20000,
            credits_start=3500000,
            credits_end=3600000,
        )
        marker_dict, xml_str = generate_with_xml("/path/to/video.mkv")
        self.assertIsInstance(marker_dict, dict)
        self.assertIsInstance(xml_str, str)
        self.assertIn("<Media>", xml_str)
        self.assertIn('type="intro"', xml_str)
        self.assertIn('type="credits"', xml_str)
        self.assertIn("video.mkv", xml_str)

    def test_build_plex_xml_structure(self):
        """Test _build_plex_xml produces valid Plex-compatible XML."""
        res = MarkerResult(
            intro_start=10000,
            intro_end=20000,
            credits_start=3500000,
            credits_end=3600000,
        )
        xml = _build_plex_xml(res, "episode.mkv")
        self.assertIn("<Media>", xml)
        self.assertIn('file="episode.mkv"', xml)
        self.assertIn('type="intro"', xml)
        self.assertIn('start="10000"', xml)
        self.assertIn('end="20000"', xml)
        self.assertIn('type="credits"', xml)
        self.assertIn('start="3500000"', xml)
        self.assertIn('end="3600000"', xml)

    def test_build_plex_xml_only_intro(self):
        """Test _build_plex_xml with only intro marker."""
        res = MarkerResult(intro_start=10000, intro_end=20000)
        xml = _build_plex_xml(res, "test.mkv")
        self.assertIn('type="intro"', xml)
        self.assertNotIn('type="credits"', xml)

    def test_build_plex_xml_only_credits(self):
        """Test _build_plex_xml with only credits marker."""
        res = MarkerResult(credits_start=3500000, credits_end=3600000)
        xml = _build_plex_xml(res, "test.mkv")
        self.assertNotIn('type="intro"', xml)
        self.assertIn('type="credits"', xml)

    def test_build_plex_xml_no_markers(self):
        """Test _build_plex_xml with no markers."""
        res = MarkerResult()
        xml = _build_plex_xml(res, "test.mkv")
        self.assertIn("<Media>", xml)
        self.assertIn('file="test.mkv"', xml)
        self.assertNotIn("<Marker", xml)


if __name__ == "__main__":
    unittest.main()
