"""Tests for paths module."""

import os
from pathlib import Path
from unittest.mock import patch

from plex_autoplay.infrastructure.paths import (
    get_custom_config_path,
    get_custom_json_path,
    get_plexautoskip_dir,
)


class TestPaths:
    """Tests for path utility functions."""

    @patch.dict(os.environ, {"PLEX_AUTOPLAY_CONFIG_DIR": "/custom/config"})
    def test_get_plexautoskip_dir(self):
        """Test get_plexautoskip_dir returns plexautoskip dir from env."""
        result = get_plexautoskip_dir()
        assert result == Path("/custom/config")

    def test_get_plexautoskip_dir_default(self):
        """Test get_plexautoskip_dir returns default when env not set."""
        with patch.dict(os.environ, {}, clear=True):
            if "PLEX_AUTOPLAY_CONFIG_DIR" in os.environ:
                del os.environ["PLEX_AUTOPLAY_CONFIG_DIR"]
            result = get_plexautoskip_dir()
            assert result == Path("/config/plexautoskip")

    @patch.dict(os.environ, {"PLEX_AUTOPLAY_CONFIG_DIR": "/custom/config"})
    def test_get_custom_config_path(self):
        """Test get_custom_config_path returns config.ini in plexautoskip dir."""
        result = get_custom_config_path()
        assert result == Path("/custom/config/config.ini")

    def test_get_custom_config_path_default(self):
        """Test get_custom_config_path returns default."""
        with patch.dict(os.environ, {}, clear=True):
            if "PLEX_AUTOPLAY_CONFIG_DIR" in os.environ:
                del os.environ["PLEX_AUTOPLAY_CONFIG_DIR"]
            result = get_custom_config_path()
            assert result == Path("/config/plexautoskip/config.ini")

    @patch.dict(
        os.environ,
        {"PLEX_AUTOPLAY_CUSTOM_JSON_PATH": "/custom/config/plexautoskip/custom.json"},
    )
    def test_get_custom_json_path(self):
        """Test get_custom_json_path returns custom.json from explicit env."""
        result = get_custom_json_path()
        assert result == Path("/custom/config/plexautoskip/custom.json")

    def test_get_custom_json_path_default(self):
        """Test get_custom_json_path returns default."""
        with patch.dict(os.environ, {}, clear=True):
            if "PLEX_AUTOPLAY_CONFIG_DIR" in os.environ:
                del os.environ["PLEX_AUTOPLAY_CONFIG_DIR"]
            result = get_custom_json_path()
            assert result == Path("/config/plexautoskip/custom.json")

    @patch.dict(os.environ, {"PLEX_AUTOPLAY_CUSTOM_JSON_PATH": "/explicit/custom.json"})
    def test_get_custom_json_path_explicit_env(self):
        """Test get_custom_json_path reads explicit env var."""
        result = get_custom_json_path()
        assert result == Path("/explicit/custom.json")
