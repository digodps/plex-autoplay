import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.media.library_repository import LibraryRepository
from plex_autoplay.application.tvshow_catalog import TVShowCatalog


class TestTVShowCatalog(unittest.TestCase):
    def test_resolves_case_insensitive_marker_keys_and_reports_missing_fields(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            season = media_root / "The Example Show" / "Season 1"
            season.mkdir(parents=True)
            (season / "The.Example.Show.S01E01.mkv").write_bytes(b"video")
            (season / "The.Example.Show.S01E02.mkv").write_bytes(b"video")
            markers_path = base / "markers.json"
            markers_path.write_text(
                json.dumps(
                    {
                        "the example show": {
                            "season_1": {
                                "1": {"start": 1.5, "end": 20.0, "credits": 100.0},
                                "2": {"start": 2.0},
                            }
                        }
                    }
                ),
                encoding="utf-8",
            )
            library_repository = LibraryRepository(media_root, base / "libraries.json")
            library_repository.add_library("The Example Show", "series")
            catalog = TVShowCatalog(
                library_repository,
                JsonMarkerRepository(str(markers_path), default_intro_skip_seconds=0),
            )

            result = catalog.build()[0]

            self.assertTrue(result.episodes[0].has_intro)
            self.assertTrue(result.episodes[0].has_credits)
            self.assertEqual(result.episodes[0].intro_start_ms, 1500)
            self.assertFalse(result.episodes[1].has_intro)
            self.assertFalse(result.episodes[1].has_credits)
            self.assertEqual((result.episodes[1].intro_start_ms, result.episodes[1].intro_end_ms), (2000, None))

            report = catalog.build_report(added_by_library={"The Example Show": [{"episode": 3}]})
            library_report = report["libraries"][0]
            self.assertEqual(library_report["with_intro"], 1)
            self.assertEqual(library_report["with_credits"], 1)
            self.assertEqual(library_report["missing"][0]["fields"], ["end", "credits"])
            self.assertEqual(library_report["added"], [{"episode": 3}])

    def test_generic_series_inventory_does_not_call_anime_apis(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            season = media_root / "Ordinary Show" / "Season 1"
            season.mkdir(parents=True)
            (season / "Ordinary.Show.S01E01.mkv").write_bytes(b"video")
            libraries = LibraryRepository(media_root, base / "libraries.json")
            libraries.add_library("Ordinary Show", "series")
            markers = JsonMarkerRepository(str(base / "markers.json"))

            with patch("plex_autoplay.infrastructure.media.directory_monitor.search_jikan_candidates") as jikan, patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times"
            ) as aniskip:
                result = TVShowCatalog(libraries, markers).build()

            self.assertEqual(len(result[0].episodes), 1)
            jikan.assert_not_called()
            aniskip.assert_not_called()


if __name__ == "__main__":
    unittest.main()
