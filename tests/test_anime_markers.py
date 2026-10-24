import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch
from urllib.error import HTTPError

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
from plex_autoplay.main import main


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


class TestPublicApisAndCache(unittest.TestCase):
    def test_ambiguous_jikan_candidates_are_not_selected_automatically(self):
        candidates = [
            {"mal_id": 1, "title": "Example"},
            {"mal_id": 2, "title": "Example"},
        ]
        self.assertIsNone(select_jikan_candidate("Example", candidates))

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
