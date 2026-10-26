"""Unit tests for plex-autoplay components."""

import unittest
from types import SimpleNamespace
from unittest.mock import MagicMock

from plex_autoplay.config import Settings
from plex_autoplay.infrastructure.plex.client import PlexManager


class TestConfig(unittest.TestCase):
    def test_default_settings(self):
        s = Settings(plex_token="dummy_token")
        s.validate()
        self.assertTrue(s.auto_skip_intro)
        self.assertTrue(s.auto_next_episode)
        self.assertEqual(s.next_episode_threshold_seconds, 15)
        self.assertEqual(s.action_delay_seconds, 5)

    def test_validation_fails_empty_token(self):
        s = Settings(plex_token="")
        with self.assertRaises(ValueError):
            s.validate()

    def test_validation_fails_placeholder_token(self):
        s = Settings(plex_token="seu_token_aqui")
        with self.assertRaises(ValueError):
            s.validate()


class TestPlexManager(unittest.TestCase):
    def test_show_episode_lookup_prefers_matching_plex_library(self):
        episodes = [SimpleNamespace(episodeNumber=1)]
        preferred_show = MagicMock(title="Example Show")
        preferred_show.episodes.return_value = episodes
        other_show = MagicMock(title="Example Show")
        preferred_section = MagicMock(type="show", title="Series")
        preferred_section.search.return_value = [preferred_show]
        other_section = MagicMock(type="show", title="Other")
        other_section.search.return_value = [other_show]
        manager = PlexManager("http://plex", "token")
        manager.server = SimpleNamespace(
            library=SimpleNamespace(sections=lambda: [preferred_section, other_section])
        )

        result = manager.list_show_episodes("Series", "Example Show")

        self.assertIs(result, episodes)
        preferred_section.search.assert_called_once_with(title="Example Show", libtype="show")
        other_section.search.assert_not_called()

    def test_show_episode_lookup_ignores_ambiguous_titles_outside_library(self):
        section_one = MagicMock(type="show", title="One")
        section_two = MagicMock(type="show", title="Two")
        section_one.search.return_value = [MagicMock(title="Example Show")]
        section_two.search.return_value = [MagicMock(title="Example Show")]
        manager = PlexManager("http://plex", "token")
        manager.server = SimpleNamespace(
            library=SimpleNamespace(sections=lambda: [section_one, section_two])
        )

        self.assertEqual(manager.list_show_episodes("Missing", "Example Show"), [])


if __name__ == "__main__":
    unittest.main()