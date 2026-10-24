import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from plex_autoplay.domain.tvshow import TVLibrary
from plex_autoplay.infrastructure.media.tvshow_scanner import scan_tv_library


class TestTVLibraryScanner(unittest.TestCase):
    def test_discovers_multiple_series_recursively_and_reports_unrecognized_video(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir) / "TV"
            first = root / "Show One" / "Season 1"
            second = root / "Show Two" / "Temporada 2" / "Extras"
            first.mkdir(parents=True)
            second.mkdir(parents=True)
            (first / "Show.One.S01E02-E03.mkv").write_bytes(b"video")
            (second / "Show.Two.E05.mp4").write_bytes(b"video")
            (root / "Show One" / "special.mkv").write_bytes(b"video")
            library = TVLibrary(name="TV", path=str(root), kind="series")

            result = scan_tv_library(library)

            self.assertEqual(
                [(episode.season.series.title, episode.season.number, episode.number) for episode in result.episodes],
                [("Show One", 1, 2), ("Show One", 1, 3), ("Show Two", 2, 5)],
            )
            self.assertEqual(len(result.unrecognized_files), 1)
            self.assertEqual(result.errors, [])

    def test_records_season_conflict_without_stopping_scan(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            season = root / "Example Show" / "Season 1"
            season.mkdir(parents=True)
            (season / "Example.Show.S02E03.mkv").write_bytes(b"video")
            result = scan_tv_library(TVLibrary(name="Example", path=str(root), kind="anime"))

            self.assertEqual(result.episodes[0].season.number, 2)
            self.assertEqual(result.ambiguities.__len__(), 1)

    def test_records_directory_walk_errors_and_returns_other_results(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            original_walk = __import__("os").walk

            def walk_with_error(path, topdown, onerror, followlinks):
                onerror(PermissionError("permission denied"))
                return iter(original_walk(path, topdown=topdown, followlinks=followlinks))

            (root / "Show").mkdir()
            with patch("plex_autoplay.infrastructure.media.tvshow_scanner.os.walk", side_effect=walk_with_error):
                result = scan_tv_library(TVLibrary(name="TV", path=str(root), kind="series"))

            self.assertEqual(len(result.errors), 1)
            self.assertIn("permission denied", result.errors[0])

    def test_reports_directory_and_video_file_progress(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir) / "TV"
            season = root / "Show" / "Season 1"
            season.mkdir(parents=True)
            (season / "Show.S01E01.mkv").write_bytes(b"video")
            events = []

            scan_tv_library(
                TVLibrary(name="TV", path=str(root), kind="series"),
                progress_callback=lambda directory, count: events.append((directory, count)),
            )

            self.assertEqual(events[-1], ("Show/Season 1", 1))


if __name__ == "__main__":
    unittest.main()
