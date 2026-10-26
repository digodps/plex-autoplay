"""Tests for paths module."""

import os
from unittest.mock import patch

from plex_autoplay.infrastructure.paths import (
    get_config_dir,
    get_custom_json_path,
    get_plexautoskip_dir,
)


class TestPaths:
    """Tests for path utility functions."""

    @patch.dict(os.environ, {"PLEX_AUTOPLAY_CONFIG_DIR": "/custom/config"})
    def test_get_config_dir_from_env(self):
        """Test get_config_dir reads from environment."""
        assert get_config_dir() == "/custom/config"

    def test_get_config_dir_default(self):
        """Test get_config_dir returns default when env not set."""
        with patch.dict(os.environ, {}, clear=True):
            # Remove env var if present
            if "PLEX_AUTOPLAY_CONFIG_DIR" in os.environ:
                del os.environ["PLEX_AUTOPLAY_CONFIG_DIR"]
            result = get_config_dir()
            assert result == "/config"

    @patch.dict(os.environ, {"PLEX_AUTOPLAY_CONFIG_DIR": "/custom/config"})
    def test_get_plexautoskip_dir(self):
        """Test get_plexautoskip_dir returns plexautoskip subdir."""
        result = get_plexautoskip_dir()
        assert result == "/custom/config/plexautoskip"

    @patch.dict(os.environ, {"PLEX_AUTOPLAY_CONFIG_DIR": "/custom/config"})
    def test_get_custom_json_path(self):
        """Test get_custom_json_path returns custom.json in plexautoskip dir."""
        result = get_custom_json_path()
        assert result == "/custom/config/plexautoskip/custom.json"

    @patch.dict(os.environ, {"PLEX_AUTOPLAY_CUSTOM_JSON_PATH": "/explicit/custom.json"})
    def test_get_custom_json_path_explicit_env(self):
        """Test get_custom_json_path reads explicit env var."""
        result = get_custom_json_path()
        assert result == "/explicit/custom.json"