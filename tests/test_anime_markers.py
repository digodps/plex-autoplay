import json
import logging
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch
from urllib.error import HTTPError

from plex_autoplay.config import config
from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.media.directory_monitor import (
    ExternalApiError,
    _cache_get,
    _cache_set,
    build_preview_for_root,
    fetch_aniskip_times,
    get_media_duration_seconds,
    load_cache,
    save_cache,
    search_anilist_candidates,
    select_anilist_candidate,
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
            self.assertEqual(episodes[0].directory_title, "Kyouran Reijou Nia Liston")

    def test_scan_media_root_parses_season_from_title_season_episode_filename(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            series_dir = root / "Ao no Hako"
            season_dir = series_dir / "Temporada 1"
            season_dir.mkdir(parents=True)
            filename = season_dir / "Ao no Hako 2 -01.mp4"
            filename.write_bytes(b"video")

            episodes = scan_media_root(root)

            self.assertEqual(len(episodes), 1)
            self.assertEqual(episodes[0].title, "Ao no Hako 2")
            self.assertEqual(episodes[0].filename_title, "Ao no Hako 2")
            self.assertEqual(episodes[0].directory_title, "Ao no Hako")
            self.assertEqual(episodes[0].season, 2)
            self.assertEqual(episodes[0].episode, 1)

    def test_scan_media_root_supports_title_season_episode_without_season_folder(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            series_dir = root / "Ao no Hako"
            series_dir.mkdir(parents=True)
            (series_dir / "Ao no Hako 2 -01.mp4").write_bytes(b"video")

            episodes = scan_media_root(root)

            self.assertEqual(len(episodes), 1)
            self.assertEqual((episodes[0].title, episodes[0].season, episodes[0].episode), ("Ao no Hako 2", 2, 1))

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
    def test_anilist_search_maps_titles_and_mal_id_for_aniskip(self):
        response = {
            "data": {
                "Page": {
                    "media": [
                        {
                            "idMal": 20,
                            "title": {
                                "romaji": "NARUTO",
                                "english": "Naruto",
                                "native": "NARUTO -ナルト-",
                            },
                        },
                        {"idMal": None, "title": {"romaji": "Unmapped", "english": None, "native": None}},
                    ]
                }
            }
        }
        with patch(
            "plex_autoplay.infrastructure.media.directory_monitor._http_json",
            return_value=response,
        ) as http_json:
            candidates = search_anilist_candidates("Naruto")

        self.assertEqual(
            candidates,
            [
                {
                    "mal_id": 20,
                    "title": "NARUTO",
                    "title_english": "Naruto",
                    "title_japanese": "NARUTO -ナルト-",
                }
            ],
        )
        request_body = json.loads(http_json.call_args.kwargs["data"])
        self.assertEqual(http_json.call_args.args, ("https://graphql.anilist.co",))
        self.assertEqual(request_body["variables"], {"search": "Naruto", "limit": 5})

    def test_preview_strips_season_suffix_and_uses_resolved_english_title_and_mal_id(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "Ao no Hako" / "Temporada 2"
            season_dir.mkdir(parents=True)
            (season_dir / "Ao no Hako 2 -01.mp4").write_bytes(b"video")
            candidates = [
                {
                    "mal_id": 57181,
                    "title": "Ao no Hako",
                    "title_english": "Blue Box",
                    "title_japanese": "アオのハコ",
                },
                {
                    "mal_id": 61323,
                    "title": "Ao no Hako Season 2",
                    "title_english": "Blue Box Season 2",
                    "title_japanese": "アオのハコ Season２",
                },
            ]
            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates",
                return_value=candidates,
            ) as anilist, patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times",
                return_value={"op": {"start": 5.583, "end": 93.086}},
            ) as aniskip:
                preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "metadata-cache.json",
                    auto_guess=True,
                    duration_reader=lambda _path: 1554.0,
                )
                cached_preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "metadata-cache.json",
                    auto_guess=True,
                    duration_reader=lambda _path: 1554.0,
                )

            anilist.assert_called_once_with("Ao no Hako 2")
            aniskip.assert_called_once_with(61323, 1, 1554.0)
            self.assertEqual(
                preview["series"],
                {"Ao no Hako": {"season_2": {"1": {"start": 5.583, "end": 93.086}}}},
            )
            self.assertEqual(preview["details"][0]["title_english"], "Blue Box Season 2")
            self.assertEqual(cached_preview["details"][0]["title_english"], "Blue Box Season 2")

            marker_file = Path(tmp_dir) / "markers.json"
            merge_markers_file(marker_file, preview["series"])
            repository = JsonMarkerRepository(str(marker_file))
            episode = SimpleNamespace(
                grandparentTitle="Ao no Hako",
                showTitle="Ao no Hako",
                seasonNumber=2,
                parentIndex=2,
                episodeNumber=1,
                index=1,
                markers=[],
            )
            self.assertEqual(repository.get_intro_markers(episode), (5583, 93086))

    def test_preview_falls_back_to_base_search_and_selects_matching_season(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "Ao no Hako" / "Temporada 2"
            season_dir.mkdir(parents=True)
            (season_dir / "Ao no Hako 2 -01.mp4").write_bytes(b"video")
            candidates = [
                {
                    "mal_id": 57181,
                    "title": "Ao no Hako",
                    "title_english": "Blue Box",
                    "title_japanese": "アオのハコ",
                },
                {
                    "mal_id": 61323,
                    "title": "Ao no Hako Season 2",
                    "title_english": "Blue Box Season 2",
                    "title_japanese": "アオのハコ Season２",
                },
            ]
            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates",
                side_effect=[[], candidates],
            ) as anilist, patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times",
                return_value={"op": {"start": 5.0, "end": 90.0}},
            ) as aniskip:
                preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "metadata-cache.json",
                    auto_guess=True,
                    duration_reader=lambda _path: 1500.0,
                )

            self.assertEqual(
                [call.args[0] for call in anilist.call_args_list],
                ["Ao no Hako 2", "Ao no Hako Season 2"],
            )
            aniskip.assert_called_once_with(61323, 1, 1500.0)
            self.assertEqual(preview["details"][0]["title_english"], "Blue Box Season 2")

    def test_preview_uses_series_directory_as_anilist_fallback(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            season_dir = root / "アオのハコ" / "Temporada 2"
            season_dir.mkdir(parents=True)
            (season_dir / "Ao no Hako 2 -01.mp4").write_bytes(b"video")
            candidates = [
                {
                    "mal_id": 61323,
                    "title": "アオのハコ Season２",
                    "title_english": "Blue Box Season 2",
                    "title_japanese": "アオのハコ Season２",
                }
            ]
            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates",
                side_effect=[[], candidates],
            ) as anilist, patch(
                "plex_autoplay.infrastructure.media.directory_monitor.fetch_aniskip_times",
                return_value={"op": {"start": 5.0, "end": 90.0}},
            ) as aniskip:
                preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "metadata-cache.json",
                    auto_guess=True,
                    duration_reader=lambda _path: 1500.0,
                )

            self.assertEqual(
                [call.args[0] for call in anilist.call_args_list],
                ["Ao no Hako 2", "アオのハコ Season 2"],
            )
            aniskip.assert_called_once_with(61323, 1, 1500.0)
            self.assertEqual(
                preview["series"],
                {"アオのハコ": {"season_2": {"1": {"start": 5.0, "end": 90.0}}}},
            )
            self.assertEqual(preview["details"][0]["title_english"], "Blue Box Season 2")

    def test_ambiguous_anilist_candidates_are_not_selected_automatically(self):
        candidates = [
            {"mal_id": 1, "title": "Example"},
            {"mal_id": 2, "title": "Example"},
        ]
        self.assertIsNone(select_anilist_candidate("Example", candidates))
        self.assertIsNone(select_anilist_candidate("Different title", [{"mal_id": 3, "title": "Unrelated"}]))

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
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates",
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

    def test_preview_pauses_remaining_anilist_searches_after_connection_failure(self):
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir) / "media"
            for title in ("First", "Second"):
                season_dir = root / title / "Temporada 1"
                season_dir.mkdir(parents=True)
                (season_dir / f"{title} -01.mkv").write_bytes(b"video")

            with patch(
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates",
                side_effect=ExternalApiError("Falha de conexão com API pública: Network is unreachable"),
            ) as anilist:
                preview = build_preview_for_root(
                    root,
                    cache_file=Path(tmp_dir) / "cache.json",
                    auto_guess=True,
                )

            self.assertEqual(anilist.call_count, 1)
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
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates",
                return_value=candidates,
            ) as anilist, patch(
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
            anilist.assert_called_once_with("Example")
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
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates",
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
                "plex_autoplay.infrastructure.media.directory_monitor.search_anilist_candidates"
            ) as anilist:
                preview = build_preview_for_root(root, cache_file=cache_file)

            self.assertEqual(preview["series"], {})
            self.assertIn("limite da API", preview["pending"][0]["reason"])
            anilist.assert_not_called()


class TestMediaDuration(unittest.TestCase):
    def test_missing_ffprobe_is_reported(self):
        with patch("plex_autoplay.infrastructure.media.directory_monitor.shutil.which", return_value=None):
            with self.assertRaisesRegex(RuntimeError, "ffprobe não está instalado"):
                get_media_duration_seconds("episode.mkv")


if __name__ == "__main__":
    unittest.main()
