import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from plex_autoplay.application.dashboard_scan_service import DashboardScanService
from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.media.library_repository import LibraryRepository


class TestDashboardScanService(unittest.TestCase):
    def test_scans_all_libraries_and_enriches_only_anime_preserving_manual_fields(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            anime = media_root / "Anime"
            series = media_root / "Series"
            (anime / "Example Anime" / "Temporada 1").mkdir(parents=True)
            (series / "Ordinary Show" / "Season 1").mkdir(parents=True)
            (anime / "Example Anime" / "Temporada 1" / "Example Anime -01.mkv").write_bytes(b"video")
            (series / "Ordinary Show" / "Season 1" / "Ordinary.Show.S01E01.mkv").write_bytes(b"video")
            markers_file = base / "markers.json"
            markers_file.write_text(
                json.dumps({"Example Anime": {"season_1": {"1": {"start": 5.0}}}}),
                encoding="utf-8",
            )
            library_repository = LibraryRepository(media_root, base / "libraries.json")
            library_repository.add_library("Anime", "anime")
            library_repository.add_library("Series", "series")
            marker_repository = JsonMarkerRepository(str(markers_file))
            service = DashboardScanService(
                library_repository,
                marker_repository,
                markers_file,
                base / "cache.json",
                3600,
                auto_generate_anime_markers=True,
            )
            preview = {
                "series": {"Example Anime": {"season_1": {"1": {"start": 99.0, "end": 20.0, "credits": 80.0}}}},
                "pending": [],
                "assumptions": [{"title": "Example Anime", "reason": "estimativa"}],
            }

            with patch(
                "plex_autoplay.application.dashboard_scan_service.build_preview_for_root",
                return_value=preview,
            ) as build_preview:
                report = service.scan()

            self.assertEqual([call.args[0] for call in build_preview.call_args_list], [anime])
            self.assertEqual(len(report["libraries"]), 2)
            anime_report, series_report = report["libraries"]
            self.assertEqual(anime_report["added"][0]["fields"], ["credits", "end"])
            self.assertEqual(anime_report["preserved"][0]["fields"], ["start"])
            self.assertEqual(len(anime_report["assumptions"]), 1)
            self.assertEqual(series_report["added"], [])
            self.assertEqual(series_report["episode_count"], 1)
            markers = json.loads(markers_file.read_text(encoding="utf-8"))
            self.assertEqual(markers["Example Anime"]["season_1"]["1"]["start"], 5.0)

    def test_anime_enrichment_failure_does_not_prevent_series_inventory(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            base = Path(temp_dir)
            media_root = base / "media"
            anime = media_root / "Anime"
            series = media_root / "Series"
            (anime / "Anime Show" / "Season 1").mkdir(parents=True)
            (series / "TV Show" / "Season 1").mkdir(parents=True)
            (anime / "Anime Show" / "Season 1" / "Anime.Show.S01E01.mkv").write_bytes(b"video")
            (series / "TV Show" / "Season 1" / "TV.Show.S01E01.mkv").write_bytes(b"video")
            library_repository = LibraryRepository(media_root, base / "libraries.json")
            library_repository.add_library("Anime", "anime")
            library_repository.add_library("Series", "series")
            marker_repository = JsonMarkerRepository(str(base / "markers.json"))
            service = DashboardScanService(
                library_repository,
                marker_repository,
                base / "markers.json",
                base / "cache.json",
                3600,
                auto_generate_anime_markers=True,
            )

            with patch(
                "plex_autoplay.application.dashboard_scan_service.build_preview_for_root",
                side_effect=RuntimeError("provider unavailable"),
            ):
                report = service.scan()

            self.assertEqual(report["libraries"][0]["errors"][0], "Falha no enriquecimento Anime: provider unavailable")
            self.assertEqual(report["libraries"][1]["episode_count"], 1)
            self.assertEqual(report["libraries"][1]["errors"], [])


if __name__ == "__main__":
    unittest.main()
