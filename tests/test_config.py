"""Tests for config module."""

import os
import tempfile
from unittest.mock import patch

import pytest

from plex_autoplay.config import Config, load_config


class TestConfig:
    """Tests for Config class and load_config function."""

    def test_default_settings(self):
        """Test Config loads with defaults when file missing."""
        with tempfile.TemporaryDirectory() as tmpdir:
            config_path = os.path.join(tmpdir, "config.ini")
            config = load_config(config_path)
            assert config.plex_token == ""
            assert config.plex_url == "http://localhost:32400"
            assert config.admin_user == "admin"
            assert config.admin_password == "admin"

    def test_validation_fails_empty_token(self):
        """Test validation fails when Plex token is empty."""
        with tempfile.TemporaryDirectory() as tmpdir:
            config_path = os.path.join(tmpdir, "config.ini")
            with open(config_path, "w") as f:
                f.write("[Plex]\ntoken = \n")
            with pytest.raises(ValueError, match="Plex token"):
                load_config(config_path)

    def test_validation_fails_placeholder_token(self):
        """Test validation fails with placeholder token."""
        with tempfile.TemporaryDirectory() as tmpdir:
            config_path = os.path.join(tmpdir, "config.ini")
            with open(config_path, "w") as f:
                f.write("[Plex]\ntoken = YOUR_TOKEN_HERE\n")
            with pytest.raises(ValueError, match="Plex token"):
                load_config(config_path)

    def test_load_config_with_valid_file(self):
        """Test loading config from valid file."""
        with tempfile.TemporaryDirectory() as tmpdir:
            config_path = os.path.join(tmpdir, "config.ini")
            with open(config_path, "w") as f:
                f.write("""[Plex]
token = valid_token_123
url = http://plex:32400

[Admin]
user = myadmin
password = mypass
""")
            config = load_config(config_path)
            assert config.plex_token == "valid_token_123"
            assert config.plex_url == "http://plex:32400"
            assert config.admin_user == "myadmin"
            assert config.admin_password == "mypass"

    def test_config_dataclass_defaults(self):
        """Test Config dataclass default values."""
        config = Config()
        assert config.plex_token == ""
        assert config.plex_url == "http://localhost:32400"
        assert config.admin_user == "admin"
        assert config.admin_password == "admin"
        assert config.log_level == "INFO"
        assert config.cookie_secure is False

    @patch.dict(os.environ, {"PLEX_TOKEN": "env_token", "PLEX_URL": "http://env:32400"})
    def test_load_config_from_env(self):
        """Test config loads from environment variables."""
        with tempfile.TemporaryDirectory() as tmpdir:
            config_path = os.path.join(tmpdir, "config.ini")
            # Empty file - should use env vars
            open(config_path, "w").close()
            config = load_config(config_path)
            assert config.plex_token == "env_token"
            assert config.plex_url == "http://env:32400"