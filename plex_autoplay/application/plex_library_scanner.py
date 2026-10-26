"""Plex library scanner – writes intro/credits markers to a JSON file for PlexAutoSkip."""

from __future__ import annotations

import json
import logging
import os
import pathlib
import threading
from dataclasses import dataclass, field
from typing import List, Optional

from plex_autoplay.infrastructure.plex.client import PlexManager
from plex_autoplay.application.marker_generator import generate_with_xml
from plex_autoplay.infrastructure.paths import get_custom_json_path

logger = logging.getLogger("plex-autoplay.scanner")

# Path to the JSON file consumed by PlexAutoSkip (shared via /config volume)
CUSTOM_JSON_PATH = get_custom_json_path()


@dataclass
class EpisodeResult:
    rating_key: str
    status: str  # created, updated, skipped, error
    error: Optional[str] = None


@dataclass
class ScanResult:
    processed: int = 0
    skipped: int = 0
    errors: List[dict] = field(default_factory=list)


class PlexLibraryScanner:
    def __init__(self, plex: PlexManager, cancel_event: threading.Event | None = None):
        self.plex = plex
        self.cancel_event = cancel_event or threading.Event()

    def scan_section(self, section_key: str) -> ScanResult:
        result = ScanResult()
        episodes = self.plex.list_episodes(section_key)
        for ep in episodes:
            if self.cancel_event.is_set():
                break
            ep_res = self.process_episode(ep["rating_key"])
            if ep_res.status == "error":
                result.errors.append({"rating_key": ep_res.rating_key, "error": ep_res.error})
            elif ep_res.status == "skipped":
                result.skipped += 1
            else:
                result.processed += 1
        return result

    def process_episode(self, rating_key: str) -> EpisodeResult:
        try:
            server = self.plex.get_server()
            episode = server.fetchItem(f"/library/metadata/{rating_key}")

            # Extract friendly identifiers
            series_name = getattr(episode, "grandparentTitle", None) or getattr(episode, "showTitle", None) or "Unknown Series"
            season_num = getattr(episode, "parentIndex", None) or getattr(episode, "seasonNumber", None) or "?"
            episode_num = getattr(episode, "index", None) or getattr(episode, "episodeNumber", None) or "?"
            episode_title = getattr(episode, "title", None) or "Unknown Episode"

            # If both markers already exist in Plex, skip (optional)
            has_intro = any(m.type == "intro" for m in episode.markers)
            has_credits = any(m.type == "credits" for m in episode.markers)
            if has_intro and has_credits:
                logger.info("[%s] S%02sE%02s %s – already has intro & credits, skipping", series_name, season_num, episode_num, episode_title)
                return EpisodeResult(rating_key, "skipped")

            # Locate media file
            file_path = None
            if episode.media:
                file_path = episode.media[0].parts[0].file
            if not file_path:
                logger.warning("[%s] S%02sE%02s %s – no file path available", series_name, season_num, episode_num, episode_title)
                return EpisodeResult(rating_key, "error", "No file path available")

            logger.info("[%s] S%02sE%02s %s – scanning %s", series_name, season_num, episode_num, episode_title, file_path)

            # Run detection (returns dict with ms timestamps)
            markers_dict, _ = generate_with_xml(file_path)
            if not markers_dict:
                logger.info("[%s] S%02sE%02s %s – no markers detected", series_name, season_num, episode_num, episode_title)
                return EpisodeResult(rating_key, "skipped")

            # Load existing custom.json
            data = {}
            # Ensure directory exists
            CUSTOM_JSON_PATH.parent.mkdir(parents=True, exist_ok=True)
            if CUSTOM_JSON_PATH.is_file():
                try:
                    with CUSTOM_JSON_PATH.open("r", encoding="utf-8") as f:
                        data = json.load(f)
                except json.JSONDecodeError:
                    data = {}

            # Update entry for this ratingKey
            data[rating_key] = markers_dict

            # Atomic write
            tmp_path = CUSTOM_JSON_PATH.with_suffix(".tmp")
            with tmp_path.open("w", encoding="utf-8") as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            tmp_path.replace(CUSTOM_JSON_PATH)

            logger.info("[%s] S%02sE%02s %s – markers written (intro=%s, credits=%s)",
                        series_name, season_num, episode_num, episode_title,
                        "intro" in markers_dict, "credits" in markers_dict)
            return EpisodeResult(rating_key, "created")
        except Exception as exc:
            logger.exception("[%s] Failed processing episode %s", series_name, rating_key)
            return EpisodeResult(rating_key, "error", str(exc))