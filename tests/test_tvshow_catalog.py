import json
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import Mock, patch

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

            with patch("plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates") as anilist, patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times"
            ) as aniskip:
                result = TVShowCatalog(libraries, markers).build()

            self.assertEqual(len(result[0].episodes), 1)
            anilist.assert_not_called()
            aniskip.assert_not_called()

    def test_series_coverage_uses_native_plex_markers_and_keeps_them_after_refresh(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            season = media_root / "Series" / "Ordinary Show" / "Season 1"
            season.mkdir(parents=True)
            (season / "Ordinary.Show.S01E01.mkv").write_bytes(b"video")
            libraries = LibraryRepository(media_root, base / "libraries.json")
            libraries.add_library("Series", "series")
            markers_path = base / "markers.json"
            markers_path.write_text(
                json.dumps(
                    {"Ordinary Show": {"season_1": {"1": {"start": 1.0, "end": 9.0, "credits": 60.0}}}}
                ),
                encoding="utf-8",
            )
            native_episode = SimpleNamespace(
                seasonNumber=1,
                episodeNumber=1,
                markers=[
                    SimpleNamespace(type="intro", start=12_000, end=32_000),
                    SimpleNamespace(type="credits", start=1_200_000, end=1_300_000),
                ],
            )
            lookup = Mock(return_value=[native_episode])
            catalog = TVShowCatalog(
                libraries,
                JsonMarkerRepository(str(markers_path), default_intro_skip_seconds=0),
                plex_episode_lookup=lookup,
            )
            events = []

            result = catalog.build(progress_callback=events.append)[0]

            self.assertEqual(result.episodes[0].intro_start_ms, 12_000)
            self.assertEqual(result.episodes[0].intro_end_ms, 32_000)
            self.assertEqual(result.episodes[0].credits_ms, 1_200_000)
            lookup.assert_called_once_with("Series", "Ordinary Show")
            self.assertTrue(any(event["stage"] == "plex-markers" for event in events))
            catalog.refresh_markers([result])
            self.assertEqual(result.episodes[0].intro_start_ms, 12_000)
            self.assertEqual(result.episodes[0].credits_ms, 1_200_000)

    def test_anime_coverage_prefers_native_plex_markers_over_generated_markers(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            season = media_root / "Anime" / "Anime Show" / "Season 1"
            season.mkdir(parents=True)
            (season / "Anime.Show.S01E01.mkv").write_bytes(b"video")
            libraries = LibraryRepository(media_root, base / "libraries.json")
            libraries.add_library("Anime", "anime")
            markers_path = base / "markers.json"
            markers_path.write_text(
                json.dumps(
                    {"Anime Show": {"season_1": {"1": {"start": 1.0, "end": 20.0, "credits": 60.0}}}}
                ),
                encoding="utf-8",
            )
            plex_episode = SimpleNamespace(
                seasonNumber=1,
                episodeNumber=1,
                markers=[SimpleNamespace(type="intro", start=11_000, end=31_000)],
            )
            lookup = Mock(return_value=[plex_episode])

            catalog = TVShowCatalog(
                libraries,
                JsonMarkerRepository(str(markers_path), default_intro_skip_seconds=0),
                plex_episode_lookup=lookup,
            )
            result = catalog.build()[0]

            self.assertEqual(result.episodes[0].intro_start_ms, 11_000)
            self.assertEqual(result.episodes[0].intro_end_ms, 31_000)
            self.assertEqual(result.episodes[0].credits_ms, 60_000)
            lookup.assert_called_once_with("Anime", "Anime Show")

    def test_plex_marker_lookup_is_not_used_without_plex_gateway(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            season = media_root / "Anime" / "Anime Show" / "Season 1"
            season.mkdir(parents=True)
            (season / "Anime.Show.S01E01.mkv").write_bytes(b"video")
            libraries = LibraryRepository(media_root, base / "libraries.json")
            libraries.add_library("Anime", "anime")
            result = TVShowCatalog(
                libraries,
                JsonMarkerRepository(str(base / "markers.json")),
            ).build()[0]

            self.assertEqual(len(result.episodes), 1)

    def test_reports_inventory_progress_for_each_library_directory(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            season = media_root / "Example" / "Season 1"
            season.mkdir(parents=True)
            (season / "Example.S01E01.mkv").write_bytes(b"video")
            libraries = LibraryRepository(media_root, base / "libraries.json")
            libraries.add_library("Example", "series")
            catalog = TVShowCatalog(libraries, JsonMarkerRepository(str(base / "markers.json")))
            events = []

            catalog.build(progress_callback=events.append)

            self.assertEqual(events[-1]["completed_libraries"], 1)
            self.assertEqual(events[-1]["total_libraries"], 1)
            self.assertGreaterEqual(events[-1]["directories_scanned"], 2)
            self.assertEqual(events[-1]["files_scanned"], 1)


if __name__ == "__main__":
    unittest.main()
