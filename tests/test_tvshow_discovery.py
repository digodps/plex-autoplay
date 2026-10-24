import unittest

from plex_autoplay.domain.tvshow import EpisodeNumber, EpisodeParseResult, parse_episode_numbers


class TestEpisodeFilenameParsing(unittest.TestCase):
    def test_parses_standard_plex_episode_name(self):
        self.assertEqual(
            parse_episode_numbers("Example.Show.S01E02.1080p.mkv"),
            EpisodeParseResult((EpisodeNumber(season=1, episode=2),)),
        )

    def test_parses_multi_episode_range_once_per_episode(self):
        self.assertEqual(
            parse_episode_numbers("Example.Show.S01E02-E03.mkv"),
            EpisodeParseResult(
                (
                    EpisodeNumber(season=1, episode=2),
                    EpisodeNumber(season=1, episode=3),
                )
            ),
        )

    def test_parses_compact_season_episode_name(self):
        self.assertEqual(
            parse_episode_numbers("Example.Show.2x07.mkv"),
            EpisodeParseResult((EpisodeNumber(season=2, episode=7),)),
        )

    def test_ignores_unrecognized_or_invalid_episode_names(self):
        for filename in ("special-feature.mkv", "Show.S00E02.mkv", "Show.S01E00.mkv", "Show.0x01.mkv"):
            with self.subTest(filename=filename):
                self.assertIsNone(parse_episode_numbers(filename))

    def test_uses_season_folder_for_contextual_and_legacy_names(self):
        expected = EpisodeParseResult((EpisodeNumber(season=1, episode=2),))
        self.assertEqual(parse_episode_numbers("Example.Show.E02.mkv", season_context=1), expected)
        self.assertEqual(parse_episode_numbers("Example.Show -02.mkv", season_context=1), expected)

    def test_reports_file_and_folder_season_conflict(self):
        result = parse_episode_numbers("Example.Show.S02E03.mkv", season_context=1)
        self.assertEqual(result.episodes, (EpisodeNumber(season=2, episode=3),))
        self.assertIsNotNone(result.ambiguity)


if __name__ == "__main__":
    unittest.main()
