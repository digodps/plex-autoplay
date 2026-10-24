"""Unit tests for plex-autoplay components."""

import unittest
from unittest.mock import MagicMock

from plex_autoplay.application.playback_service import PlaybackService
from plex_autoplay.config import Settings
from plex_autoplay.domain.playback_rules import should_skip_intro, should_trigger_next_episode
from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.plex.client import PlexManager


class TestConfig(unittest.TestCase):
    def test_default_settings(self):
        s = Settings(plex_token="dummy_token")
        s.validate()
        self.assertTrue(s.auto_skip_intro)
        self.assertTrue(s.auto_next_episode)
        self.assertEqual(s.next_episode_threshold_seconds, 15)

    def test_validation_fails_empty_token(self):
        s = Settings(plex_token="")
        with self.assertRaises(ValueError):
            s.validate()

    def test_validation_fails_placeholder_token(self):
        s = Settings(plex_token="seu_token_aqui")
        with self.assertRaises(ValueError):
            s.validate()


class TestJsonMarkerRepository(unittest.TestCase):
    def setUp(self):
        self.manager = JsonMarkerRepository(markers_file_path="markers.example.json")

    def test_load_example_markers(self):
        self.assertIn("Stranger Things", self.manager.custom_markers)

    def test_custom_intro_lookup(self):
        mock_ep = MagicMock()
        mock_ep.grandparentTitle = "Stranger Things"
        mock_ep.seasonNumber = 1
        mock_ep.episodeNumber = 1
        mock_ep.markers = []

        intro = self.manager.get_intro_markers(mock_ep)
        self.assertIsNotNone(intro)
        self.assertEqual(intro, (0, 65000))

    def test_custom_default_intro_lookup(self):
        mock_ep = MagicMock()
        mock_ep.grandparentTitle = "The Office"
        mock_ep.seasonNumber = 2
        mock_ep.episodeNumber = 5
        mock_ep.markers = []

        intro = self.manager.get_intro_markers(mock_ep)
        self.assertIsNotNone(intro)
        self.assertEqual(intro, (80000, 115000))


class TestController(unittest.TestCase):
    def setUp(self):
        self.config = Settings(
            plex_token="dummy",
            auto_skip_intro=True,
            auto_next_episode=True,
            next_episode_threshold_seconds=15,
        )
        self.plex_mock = MagicMock(spec=PlexManager)
        self.marker_mock = MagicMock(spec=JsonMarkerRepository)
        self.marker_mock.get_credits_marker.return_value = None
        self.marker_mock.get_intro_markers.return_value = None
        self.controller = PlaybackService(self.plex_mock, self.marker_mock, self.config)

    def test_skip_intro_triggered(self):
        session = MagicMock()
        session.sessionKey = "123"
        session.ratingKey = "456"
        session.viewOffset = 25000  # 25s
        session.duration = 1800000  # 30min
        session.player.title = "Test TV"

        # Intro de 10s a 40s (10000 a 40000 ms)
        self.marker_mock.get_intro_markers.return_value = (10000, 40000)
        player_mock = MagicMock()
        self.plex_mock.find_player.return_value = player_mock
        self.plex_mock.seek_player.return_value = True

        self.controller.process_session(session)

        # Deve ter chamado seek_player com 40000ms
        self.plex_mock.seek_player.assert_called_once_with(player_mock, 40000)
        self.assertTrue(self.controller.states["123"].intro_skipped)

    def test_auto_next_triggered(self):
        session = MagicMock()
        session.sessionKey = "123"
        session.ratingKey = "456"
        session.duration = 100000  # 100s
        session.viewOffset = 90000  # 90s (restando 10s < 15s)
        session.player.title = "Test TV"

        self.marker_mock.get_intro_markers.return_value = None
        self.marker_mock.get_credits_marker.return_value = None

        player_mock = MagicMock()
        next_ep_mock = MagicMock()
        next_ep_mock.seasonNumber = 1
        next_ep_mock.episodeNumber = 2
        next_ep_mock.title = "Ep 2"

        self.plex_mock.find_player.return_value = player_mock
        self.plex_mock.get_next_episode.return_value = next_ep_mock
        self.plex_mock.play_media.return_value = True

        self.controller.process_session(session)

        self.plex_mock.play_media.assert_called_once_with(player_mock, next_ep_mock)
        self.assertTrue(self.controller.states["123"].next_triggered)


class TestPlaybackRules(unittest.TestCase):
    def test_intro_rule_requires_position_inside_marker(self):
        self.assertTrue(should_skip_intro(25000, (10000, 40000)))
        self.assertFalse(should_skip_intro(9000, (10000, 40000)))
        self.assertFalse(should_skip_intro(39000, (10000, 40000)))
        self.assertFalse(should_skip_intro(25000, None))

    def test_next_episode_rule_uses_credits_or_threshold(self):
        self.assertTrue(should_trigger_next_episode(85000, 100000, 85000, 15))
        self.assertFalse(should_trigger_next_episode(80000, 100000, 85000, 15))
        self.assertTrue(should_trigger_next_episode(90000, 100000, None, 15))
        self.assertFalse(should_trigger_next_episode(84000, 100000, None, 15))


if __name__ == "__main__":
    unittest.main()
