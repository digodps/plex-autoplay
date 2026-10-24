import json
import logging
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch
from urllib.error import HTTPError

from plex_autoplay.config import config
from plex_autoplay.infrastructure.media.directory_monitor import (
    ExternalApiError,
    _cache_get,
    _cache_set,
    build_preview_for_root,
    fetch_aniskip_times,
    get_media_duration_seconds,
    load_cache,
    save_cache,
    select_jikan_candidate,
)
from plex_autoplay.infrastructure.media.directory_monitor import (
    build_series_map,
    merge_markers_file,
    scan_media_root,
)
from plex_autoplay.main import _run_startup_anime_marker_generation, main


class TestAnimeMarkersDiscovery(unittest.TestCase):
    def test_scan_media_root_finds_valid_series_files(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            series_dir = root / "Kyouran Reijou Nia Liston"
            season_dir = series_dir / "Temporada 1"
            season_dir.mkdir(parents=True)
            valid_file = season_dir / "Kyouran Reijou Nia Liston Byoujaku Reijou ni Tensei shita Kamigoroshi no Bujin no Karei Naru Musouroku -01.mkv"
            valid_file.write_bytes(b"data")
            (season_dir / "notes.txt").write_text("ignored", encoding="utf-8")

            episodes = scan_media_root(root)

            self.assertEqual(len(episodes), 1)
            self.assertEqual(episodes[0].title, "Kyouran Reijou Nia Liston")
            self.assertEqual(episodes[0].season, 1)
            self.assertEqual(episodes[0].episode, 1)

    def test_scan_media_root_ignores_invalid_structure_and_extensions(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            series_dir = root / "Series Example"
            season_dir = series_dir / "Temporada 1"
            season_dir.mkdir(parents=True)
            (season_dir / "notes.txt").write_text("ignored", encoding="utf-8")
            (season_dir / "broken-file.mkv").write_bytes(b"data")
            (season_dir / "Series Example -a.mkv").write_bytes(b"data")

            self.assertEqual(scan_media_root(root), [])

    def test_scan_media_root_does_not_follow_directory_symlinks_outside_root(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            outside = Path(tmp_dir) / "outside" / "Example" / "Temporada 1"
            root.mkdir()
            outside.mkdir(parents=True)
            (outside / "Example -01.mkv").write_bytes(b"fake")
            try:
                (root / "Example").symlink_to(outside.parent, target_is_directory=True)
            except OSError as exc:
                self.skipTest(f"Symlinks unavailable: {exc}")

            self.assertEqual(scan_media_root(root), [])

    def test_build_series_map_groups_episodes_per_show(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            series_dir = root / "Series Example"
            season_dir = series_dir / "Temporada 2"
            season_dir.mkdir(parents=True)
            ep1 = season_dir / "Series Example -01.mkv"
            ep1.write_bytes(b"a")
            ep2 = season_dir / "Series Example -02.mkv"
            ep2.write_bytes(b"b")

            grouped = build_series_map(root)

            self.assertIn("Series Example", grouped)
            self.assertEqual([item.episode for item in grouped["Series Example"]], [1, 2])


class TestAnimeMarkersMerge(unittest.TestCase):
    def test_merge_markers_keeps_existing_manual_entries(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            target = Path(tmp_dir) / "markers.json"
            target.write_text(
                json.dumps(
                    {
                        "Example": {
                            "season_1": {
                                "1": {"start": 1.0, "end": 2.0, "credits": 5.0},
                            }
                        }
                    }
                ),
                encoding="utf-8",
            )

            merge_markers_file(target, {"Example": {"season_1": {"1": {"start": 10.0, "end": 20.0, "credits": 25.0}}}})
            data = json.loads(target.read_text(encoding="utf-8"))

            self.assertEqual(data["Example"]["season_1"]["1"]["start"], 1.0)
            self.assertEqual(data["Example"]["season_1"]["1"]["end"], 2.0)
            self.assertEqual(data["Example"]["season_1"]["1"]["credits"], 5.0)

    def test_merge_adds_missing_fields_without_replacing_manual_fields(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            target = Path(tmp_dir) / "markers.json"
            target.write_text(
                json.dumps({"Example": {"season_1": {"1": {"start": 2.5}}}}),
                encoding="utf-8",
            )

            merge_markers_file(
                target,
                {"Example": {"season_1": {"1": {"start": 10.0, "end": 20.0, "credits": 30.0}}}},
            )

            data = json.loads(target.read_text(encoding="utf-8"))
            self.assertEqual(data["Example"]["season_1"]["1"], {"start": 2.5, "end": 20.0, "credits": 30.0})

    def test_merge_matches_series_titles_without_case_sensitive_duplication(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            target = Path(tmp_dir) / "markers.json"
            target.write_text(
                json.dumps({"example": {"season_1": {"1": {"start": 3.0}}}}),
                encoding="utf-8",
            )

            merge_markers_file(target, {"Example": {"season_1": {"1": {"start": 30.0, "end": 40.0}}}})

            data = json.loads(target.read_text(encoding="utf-8"))
            self.assertEqual(list(data), ["example"])
            self.assertEqual(data["example"]["season_1"]["1"], {"start": 3.0, "end": 40.0})


class TestPublicApisAndCache(unittest.TestCase):
    def test_ambiguous_jikan_candidates_are_not_selected_automatically(self):
        candidates = [
            {"mal_id": 1, "title": "Example"},
            {"mal_id": 2, "title": "Example"},
        ]
        self.assertIsNone(select_jikan_candidate("Example", candidates))
        self.assertIsNone(select_jikan_candidate("Different title", [{"mal_id": 3, "title": "Unrelated"}]))

    def test_aniskip_rejects_interval_outside_episode_duration(self):
        payload = {
            "results": [
                {"skipType": "op", "interval": {"startTime": 2.0, "endTime": 20.0}},
                {"skipType": "ed", "interval": {"startTime": 90.0, "endTime": 100.0}},
            ]
        }
        with patch(
            "plex_autoplay.infrastructure.media.directory_monitor._http_json",
            return_value=payload,
        ):
            result = fetch_aniskip_times(10, 1, 95.0)
        self.assertEqual(result, {"op": {"start": 2.0, "end": 20.0}})

    def test_http_429_preserves_retry_after(self):
        headers = {"Retry-After": "30"}
        http_error = HTTPError("https://api.example", 429, "Too Many Requests", headers, None)
        with patch(
            "plex_autoplay.infrastructure.media.directory_monitor.request.urlopen",
            side_effect=http_error,
        ):
            with self.assertRaises(ExternalApiError) as raised:
                fetch_aniskip_times(10, 1, 100.0)
        self.assertEqual(raised.exception.status_code, 429)
        self.assertEqual(raised.exception.retry_after, "30")

    def test_http_404_is_reported_as_not_found(self):
        http_error = HTTPError("https://api.example", 404, "Not Found", {}, None)
        with patch(
            "plex_autoplay.infrastructure.media.directory_monitor.request.urlopen",
            side_effect=http_error,
        ):
            with self.assertRaises(ExternalApiError) as raised:
                fetch_aniskip_times(10, 1, 100.0)
        self.assertEqual(raised.exception.status_code, 404)

    def test_preview_pauses_remaining_aniskip_requests_after_connection_failure(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "Example" / "Temporada 1"
            season_dir.mkdir(parents=True)
            (season_dir / "Example -01.mkv").write_bytes(b"video")
            (season_dir / "Example -02.mkv").write_bytes(b"video")

            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_jikan_candidates",
                return_value=[{"mal_id": 42, "title": "Example"}],
            ), patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times",
                side_effect=ExternalApiError("Falha de conexão com API pública: Network is unreachable"),
            ) as aniskip:
                preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "cache.json",
                    auto_guess=True,
                    duration_reader=lambda _path: 1200,
                )

            self.assertEqual(aniskip.call_count, 1)
            self.assertEqual(len(preview["pending"]), 1)
            self.assertIn("Network is unreachable", preview["pending"][0]["reason"])
            self.assertEqual(preview["series"], {})

    def test_preview_pauses_remaining_jikan_searches_after_connection_failure(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            for title in ("First", "Second"):
                season_dir = root / title / "Temporada 1"
                season_dir.mkdir(parents=True)
                (season_dir / f"{title} -01.mkv").write_bytes(b"video")

            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_jikan_candidates",
                side_effect=ExternalApiError("Falha de conexão com API pública: Network is unreachable"),
            ) as jikan:
                preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "cache.json",
                    auto_guess=True,
                )

            self.assertEqual(jikan.call_count, 1)
            self.assertEqual(len(preview["pending"]), 1)
            self.assertIn("Network is unreachable", preview["pending"][0]["reason"])

    def test_cache_uses_ttl_and_persists(self):
        cache = {}
        _cache_set(cache, "lookup", ["result"], now=100, ttl_seconds=10)
        self.assertEqual(_cache_get(cache, "lookup", now=105), ["result"])
        self.assertIsNone(_cache_get(cache, "lookup", now=111))

        with tempfile.TemporaryDirectory() as tmp_dir:
            cache_path = Path(tmp_dir) / "metadata-cache.json"
            save_cache(cache_path, cache)
            self.assertEqual(load_cache(cache_path), cache)

    def test_cli_invalid_media_root_returns_failure_status(self):
        self.assertEqual(main(["--media-root", "/path/that/does/not/exist"]), 1)

    def test_cli_cancel_does_not_write_markers(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            root.mkdir()
            with patch(
                "plex_autoplay.main.build_preview_for_root",
                return_value={"series": {"Example": {"season_1": {"1": {"start": 10.0}}}}, "pending": []},
            ), patch("plex_autoplay.main.merge_markers_file") as merge, patch(
                "plex_autoplay.main.sys.stdin"
            ) as stdin, patch("builtins.input", return_value=""):
                stdin.isatty.return_value = True
                result = main(["--media-root", str(root), "--output", str(Path(tmp_dir) / "markers.json")])

            self.assertEqual(result, 0)
            merge.assert_not_called()

    def test_cli_yes_confirms_write_without_terminal(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            root.mkdir()
            target = Path(tmp_dir) / "markers.json"
            preview = {"series": {"Example": {"season_1": {"1": {"start": 10.0}}}}, "pending": []}
            with patch("plex_autoplay.main.build_preview_for_root", return_value=preview), patch(
                "plex_autoplay.main.merge_markers_file"
            ) as merge, patch("plex_autoplay.main.sys.stdin") as stdin:
                stdin.isatty.return_value = False
                result = main(["--media-root", str(root), "--output", str(target), "--yes", "--force"])

            self.assertEqual(result, 0)
            merge.assert_called_once_with(target, preview["series"], force=True)

    def test_preview_needs_confirmed_season_mapping_before_query(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "Example" / "Temporada 1"
            season_dir.mkdir(parents=True)
            (season_dir / "Example -01.mkv").write_bytes(b"fake")
            cache_file = Path(tmp_dir) / "metadata-cache.json"
            cache_file.write_text(
                json.dumps({"anime_mappings": {"example": 42}}),
                encoding="utf-8",
            )
            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.get_media_duration_seconds",
                return_value=100.0,
            ), patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times"
            ) as aniskip:
                preview = build_preview_for_root(root, cache_file=cache_file)

            self.assertEqual(preview["series"], {})
            self.assertIn("Mapeamento da temporada", preview["pending"][0]["reason"])
            aniskip.assert_not_called()

    def test_preview_uses_real_duration_and_reuses_cached_api_responses(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "Example" / "Temporada 1"
            season_dir.mkdir(parents=True)
            (season_dir / "Example -01.mkv").write_bytes(b"fake")
            cache_file = Path(tmp_dir) / "metadata-cache.json"
            candidates = [{"mal_id": 42, "title": "Example"}]
            times = {"op": {"start": 10.0, "end": 30.0}}

            def choose_offset(prompt: str) -> str:
                self.assertIn("offset", prompt.casefold())
                return "0"

            def read_duration(path: str | Path) -> float:
                self.assertTrue(Path(path).is_file())
                return 1423.75

            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_jikan_candidates",
                return_value=candidates,
            ) as jikan, patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times",
                return_value=times,
            ) as aniskip:
                first = build_preview_for_root(
                    root,
                    cache_file=cache_file,
                    chooser=choose_offset,
                    duration_reader=read_duration,
                )
                second = build_preview_for_root(
                    root,
                    cache_file=cache_file,
                    duration_reader=read_duration,
                )

            expected = {"Example": {"season_1": {"1": {"start": 10.0, "end": 30.0}}}}
            self.assertEqual(first["series"], expected)
            self.assertEqual(second["series"], expected)
            jikan.assert_called_once_with("Example")
            aniskip.assert_called_once_with(42, 1, 1423.75)

    def test_auto_guess_uses_top_candidate_and_zero_episode_offset(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "Example" / "Temporada 2"
            season_dir.mkdir(parents=True)
            (season_dir / "Example -03.mkv").write_bytes(b"fake")
            candidates = [
                {"mal_id": 42, "title": "Example Season 2"},
                {"mal_id": 77, "title": "Example TV"},
            ]
            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_jikan_candidates",
                return_value=candidates,
            ), patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times",
                return_value={"ed": {"start": 1200.0, "end": 1300.0}},
            ) as aniskip:
                preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "metadata-cache.json",
                    auto_guess=True,
                    duration_reader=lambda _path: 1400.0,
                )

            self.assertEqual(
                preview["series"],
                {"Example": {"season_2": {"3": {"credits": 1200.0}}}},
            )
            self.assertEqual(len(preview["assumptions"]), 2)
            aniskip.assert_called_once_with(42, 3, 1400.0)

    def test_startup_scans_anime_root_and_merges_generated_markers(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            anime_root = Path(tmp_dir) / "anime"
            dubbed_anime_root = Path(tmp_dir) / "anime-dubbed"
            series_root = Path(tmp_dir) / "series"
            second_series_root = Path(tmp_dir) / "series-2"
            anime_root.mkdir()
            dubbed_anime_root.mkdir()
            series_root.mkdir()
            second_series_root.mkdir()
            preview = {
                "series": {"Example": {"season_1": {"1": {"start": 10.0, "end": 30.0}}}},
                "pending": [],
                "assumptions": [],
            }
            dubbed_preview = {
                "series": {"Dubbed Example": {"season_1": {"1": {"start": 12.0, "end": 32.0}}}},
                "pending": [],
                "assumptions": [],
            }
            with patch.object(config, "auto_generate_anime_markers", True), patch.object(
                config, "anime_media_dirs", [str(anime_root), str(dubbed_anime_root)]
            ), patch.object(config, "series_media_dirs", [str(series_root), str(second_series_root)]), patch.object(
                config, "anime_markers_cache_file", str(Path(tmp_dir) / "metadata-cache.json")
            ), patch.object(config, "anime_markers_cache_ttl_hours", 168), patch.object(
                config, "custom_markers_file", str(Path(tmp_dir) / "markers.json")
            ), patch(
                "plex_autoplay.main.build_preview_for_root",
                side_effect=[preview, dubbed_preview],
            ) as build_preview, patch("plex_autoplay.main.merge_markers_file") as merge:
                _run_startup_anime_marker_generation(logging.getLogger("test"))

            self.assertEqual(
                [call.args[0] for call in build_preview.call_args_list],
                [anime_root, dubbed_anime_root],
            )
            self.assertEqual(
                build_preview.call_args_list[0].kwargs,
                {
                    "cache_file": str(Path(tmp_dir) / "metadata-cache.json"),
                    "cache_ttl_seconds": 168 * 60 * 60,
                    "auto_guess": True,
                },
            )
            expected_markers = {
                **preview["series"],
                **dubbed_preview["series"],
            }
            merge.assert_called_once_with(str(Path(tmp_dir) / "markers.json"), expected_markers)

    def test_cached_rate_limit_prevents_immediate_api_retry(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "Example" / "Temporada 1"
            season_dir.mkdir(parents=True)
            (season_dir / "Example -01.mkv").write_bytes(b"fake")
            cache_file = Path(tmp_dir) / "metadata-cache.json"
            cache_file.write_text(json.dumps({"api_retry_until": 200}), encoding="utf-8")
            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.time.time",
                return_value=100,
            ), patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_jikan_candidates"
            ) as jikan:
                preview = build_preview_for_root(root, cache_file=cache_file)

            self.assertEqual(preview["series"], {})
            self.assertIn("limite da API", preview["pending"][0]["reason"])
            jikan.assert_not_called()


class TestMediaDuration(unittest.TestCase):
    def test_missing_ffprobe_is_reported(self):
        with patch("plex_autoplay.infrastructure.media.directory_monitor.shutil.which", return_value=None):
            with self.assertRaisesRegex(RuntimeError, "ffprobe não está instalado"):
                get_media_duration_seconds("episode.mkv")


if __name__ == "__main__":
    unittest.main()
