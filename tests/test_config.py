"""Tests for config module."""

import os
import sys
from unittest.mock import MagicMock, patch

import pytest


def reload_config_module():
    """Reload config module to pick up new env vars."""
    # Remove cached modules
    for mod in ["plex_autoplay.config", "dotenv"]:
        if mod in sys.modules:
            del sys.modules[mod]
    # Mock dotenv
    sys.modules["dotenv"] = MagicMock()
    sys.modules["dotenv"].load_dotenv = MagicMock()
    # Re-import
    from plex_autoplay.config import Settings, config

    return Settings, config


class TestConfig:
    """Tests for Settings class and config instance."""

    def test_default_settings(self):
        """Test Settings loads with defaults."""
        with patch.dict(
            os.environ,
            {
                "PLEX_TOKEN": "",
                "PLEX_URL": "http://localhost:32400",
                "AUTO_SKIP_INTRO": "true",
                "AUTO_NEXT_EPISODE": "true",
                "NEXT_EPISODE_THRESHOLD_SECONDS": "15",
                "ACTION_DELAY_SECONDS": "5",
                "POLL_INTERVAL_SECONDS": "2",
                "ALLOWED_CLIENT_IDS": "",
                "ALLOWED_USERNAMES": "",
                "CONFIG_DIR": "/config",
                "WEB_PORT": "8080",
                "DASHBOARD_COOKIE_SECURE": "true",
                "DASHBOARD_HTTPS": "true",
                "DASHBOARD_TLS_HOSTS": "localhost,plex-autoplay,host.docker.internal",
                "LOG_LEVEL": "INFO",
                "WEBHOOK_SECRET": "",
            },
            clear=True,
        ):
            Settings, _config = reload_config_module()
            settings = Settings()
            assert settings.plex_token == ""
            assert settings.plex_url == "http://localhost:32400"
            assert settings.auto_skip_intro is True
            assert settings.auto_next_episode is True

    def test_validation_fails_empty_token(self):
        """Test validation fails when Plex token is empty."""
        with patch.dict(
            os.environ,
            {"PLEX_TOKEN": "", "PLEX_URL": "http://localhost:32400"},
            clear=True,
        ):
            Settings, _config = reload_config_module()
            settings = Settings()
            with pytest.raises(ValueError, match="PLEX_TOKEN não configurado"):
                settings.validate()

    def test_validation_fails_placeholder_token(self):
        """Test validation fails with placeholder token."""
        with patch.dict(
            os.environ,
            {"PLEX_TOKEN": "seu_token_aqui", "PLEX_URL": "http://localhost:32400"},
            clear=True,
        ):
            Settings, _config = reload_config_module()
            settings = Settings()
            with pytest.raises(ValueError, match="PLEX_TOKEN não configurado"):
                settings.validate()

    def test_validation_fails_invalid_threshold(self):
        """Test validation fails with invalid threshold."""
        with patch.dict(
            os.environ,
            {
                "PLEX_TOKEN": "valid",
                "PLEX_URL": "http://localhost:32400",
                "NEXT_EPISODE_THRESHOLD_SECONDS": "0",
            },
            clear=True,
        ):
            Settings, _config = reload_config_module()
            settings = Settings()
            with pytest.raises(
                ValueError, match="THRESHOLD_SECONDS deve ser de pelo menos 1"
            ):
                settings.validate()

    def test_validation_fails_invalid_poll_interval(self):
        """Test validation fails with invalid poll interval."""
        with patch.dict(
            os.environ,
            {
                "PLEX_TOKEN": "valid",
                "PLEX_URL": "http://localhost:32400",
                "POLL_INTERVAL_SECONDS": "0",
            },
            clear=True,
        ):
            Settings, _config = reload_config_module()
            settings = Settings()
            with pytest.raises(
                ValueError, match="POLL_INTERVAL_SECONDS deve ser de pelo menos 1"
            ):
                settings.validate()

    def test_validation_fails_invalid_port(self):
        """Test validation fails with invalid port."""
        with patch.dict(
            os.environ,
            {
                "PLEX_TOKEN": "valid",
                "PLEX_URL": "http://localhost:32400",
                "WEB_PORT": "70000",
            },
            clear=True,
        ):
            Settings, _config = reload_config_module()
            settings = Settings()
            with pytest.raises(ValueError, match="WEB_PORT deve estar entre 1 e 65535"):
                settings.validate()

    def test_settings_from_env(self):
        """Test settings load from environment variables."""
        with patch.dict(
            os.environ,
            {
                "PLEX_TOKEN": "env_token",
                "PLEX_URL": "http://env:32400",
                "AUTO_SKIP_INTRO": "false",
                "LOG_LEVEL": "DEBUG",
            },
            clear=True,
        ):
            Settings, _config = reload_config_module()
            settings = Settings()
            assert settings.plex_token == "env_token"
            assert settings.plex_url == "http://env:32400"
            assert settings.auto_skip_intro is False
            assert settings.log_level == "DEBUG"

    def test_config_instance_exists(self):
        """Test global config instance exists."""
        _Settings, config = reload_config_module()
        assert config is not None
        assert hasattr(config, "plex_token")
        assert hasattr(config, "plex_url")
