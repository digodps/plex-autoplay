"""Path utilities for PlexAutoSkip configuration."""

import os
import pathlib


def get_custom_json_path() -> "pathlib.Path":
    """Get the path to the PlexAutoSkip custom.json file.
    Can be overridden via PLEX_AUTOPLAY_CUSTOM_JSON_PATH environment variable.
    """
    custom_path = os.getenv("PLEX_AUTOPLAY_CUSTOM_JSON_PATH")
    if custom_path:
        return pathlib.Path(custom_path)
    return pathlib.Path("/config/plexautoskip/custom.json")


def get_custom_config_path() -> "pathlib.Path":
    """Get the path to the PlexAutoSkip config.ini file."""
    custom_path = os.getenv("PLEX_AUTOPLAY_CONFIG_DIR")
    if custom_path:
        return pathlib.Path(custom_path) / "config.ini"
    return pathlib.Path("/config/plexautoskip/config.ini")


def get_plexautoskip_dir() -> "pathlib.Path":
    """Get the PlexAutoSkip directory path."""
    custom_path = os.getenv("PLEX_AUTOPLAY_CONFIG_DIR")
    if custom_path:
        return pathlib.Path(custom_path)
    return pathlib.Path("/config/plexautoskip")


def get_custom_json_path() -> "pathlib.Path":
    """Get the path to the PlexAutoSkip custom.json file.
    Can be overridden via PLEX_AUTOPLAY_CUSTOM_JSON_PATH environment variable.
    """
    custom_path = os.getenv("PLEX_AUTOPLAY_CUSTOM_JSON_PATH")
    if custom_path:
        return pathlib.Path(custom_path)
    return pathlib.Path("/config/plexautoskip/custom.json")


def get_custom_config_path() -> "pathlib.Path":
    """Get the path to the PlexAutoSkip config.ini file."""
    custom_path = os.getenv("PLEX_AUTOPLAY_CONFIG_DIR")
    if custom_path:
        return pathlib.Path(custom_path) / "config.ini"
    return pathlib.Path("/config/plexautoskip/config.ini")


def get_plexautoskip_dir() -> "pathlib.Path":
    """Get the PlexAutoSkip directory path."""
    custom_path = os.getenv("PLEX_AUTOPLAY_CONFIG_DIR")
    if custom_path:
        return pathlib.Path(custom_path)
    return pathlib.Path("/config/plexautoskip")