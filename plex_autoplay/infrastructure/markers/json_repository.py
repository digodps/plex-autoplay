"""JSON/YAML-backed repository for custom episode markers."""

import json
import logging
import math
from pathlib import Path
from typing import Any, Dict, Optional, Tuple

try:
    import yaml
except ImportError:
    yaml = None

logger = logging.getLogger("plex-autoplay.markers")


class JsonMarkerRepository:
    """Loads native Plex markers and optional custom markers from disk."""

    def __init__(self, markers_file_path: str = "markers.json", default_intro_skip_seconds: int = 0):
        self.markers_file_path = markers_file_path
        self.default_intro_skip_seconds = default_intro_skip_seconds
        self.custom_markers: Dict[str, Any] = {}
        self.load_custom_markers()

    def load_custom_markers(self) -> None:
        path = Path(self.markers_file_path)
        if not path.is_file():
            logger.debug("Arquivo de marcadores customizados não encontrado: %s", self.markers_file_path)
            self.custom_markers = {}
            return

        try:
            with open(path, "r", encoding="utf-8") as marker_file:
                if path.suffix.lower() in (".yaml", ".yml") and yaml is not None:
                    self.custom_markers = yaml.safe_load(marker_file) or {}
                else:
                    self.custom_markers = json.load(marker_file) or {}
            logger.info("Marcadores customizados carregados com sucesso de %s", path)
        except Exception as error:
            logger.warning("Erro ao ler arquivo de marcadores %s: %s", path, error)
            self.custom_markers = {}

    def get_intro_markers(self, episode: Any) -> Optional[Tuple[int, int]]:
        native_marker = self._get_native_intro(episode)
        if native_marker:
            return native_marker

        custom_marker = self._get_custom_intro(episode)
        if custom_marker:
            return custom_marker

        if self.default_intro_skip_seconds > 0:
            return (0, self.default_intro_skip_seconds * 1000)

        return None

    def get_credits_marker(self, episode: Any) -> Optional[int]:
        if hasattr(episode, "markers"):
            try:
                for marker in episode.markers:
                    marker_type = getattr(marker, "type", "").lower()
                    if marker_type in ("credits", "outro"):
                        return int(marker.start)
            except Exception:
                pass

        show_title = getattr(episode, "grandparentTitle", "") or getattr(episode, "showTitle", "")
        season_num = getattr(episode, "seasonNumber", None) or getattr(episode, "parentIndex", None)
        ep_num = getattr(episode, "episodeNumber", None) or getattr(episode, "index", None)

        show_data = self._find_show_data(show_title)
        if show_data:
            return self._find_episode_credits(show_data, season_num, ep_num)
        return None

    def get_custom_marker_values(self, episode: Any) -> Dict[str, float]:
        """Return explicit custom marker fields in seconds for an episode."""
        show_title = getattr(episode, "grandparentTitle", "") or getattr(episode, "showTitle", "")
        season_num = getattr(episode, "seasonNumber", None) or getattr(episode, "parentIndex", None)
        ep_num = getattr(episode, "episodeNumber", None) or getattr(episode, "index", None)
        show_data = self._find_show_data(show_title)
        if not show_data:
            return {}

        values: Dict[str, float] = {}
        default_entry = show_data.get("default")
        if isinstance(default_entry, dict):
            values.update(self._validated_marker_values(default_entry))
        if season_num is not None and ep_num is not None:
            season_key = f"season_{season_num}"
            season_data = show_data.get(season_key)
            if isinstance(season_data, dict):
                episode_data = season_data.get(str(ep_num))
                if isinstance(episode_data, dict):
                    values.update(self._validated_marker_values(episode_data))
        return values

    @staticmethod
    def _validated_marker_values(data: Dict[str, Any]) -> Dict[str, float]:
        values: Dict[str, float] = {}
        for key, value in data.items():
            if key not in {"start", "end", "credits"}:
                continue
            if isinstance(value, bool) or not isinstance(value, (int, float)):
                raise ValueError(f"Marcador customizado '{key}' deve ser numérico.")
            numeric_value = float(value)
            if not math.isfinite(numeric_value) or numeric_value < 0:
                raise ValueError(f"Marcador customizado '{key}' deve ser finito e não negativo.")
            values[key] = numeric_value
        return values

    def _get_native_intro(self, episode: Any) -> Optional[Tuple[int, int]]:
        if not hasattr(episode, "markers"):
            return None

        try:
            for marker in episode.markers:
                marker_type = getattr(marker, "type", "").lower()
                if marker_type in ("intro", "intro_marker"):
                    return (int(marker.start), int(marker.end))
        except Exception as error:
            logger.debug("Falha ao inspecionar marcadores nativos: %s", error)

        return None

    def _find_show_data(self, show_title: str) -> Optional[Dict[str, Any]]:
        if not show_title or not self.custom_markers:
            return None

        clean_title = show_title.strip().lower()
        for key, value in self.custom_markers.items():
            if key.strip().lower() == clean_title and isinstance(value, dict):
                return value
        return None

    def _find_episode_credits(
        self,
        show_data: Dict[str, Any],
        season_num: Any,
        ep_num: Any,
    ) -> Optional[int]:
        if season_num is not None and ep_num is not None:
            season_key = f"season_{season_num}"
            if season_key in show_data and isinstance(show_data[season_key], dict):
                episode_data = show_data[season_key].get(str(ep_num))
                if isinstance(episode_data, dict) and "credits" in episode_data:
                    return int(episode_data["credits"] * 1000)

        default_entry = show_data.get("default")
        if isinstance(default_entry, dict) and "credits" in default_entry:
            return int(default_entry["credits"] * 1000)
        return None

    def _get_custom_intro(self, episode: Any) -> Optional[Tuple[int, int]]:
        show_title = getattr(episode, "grandparentTitle", "") or getattr(episode, "showTitle", "")
        season_num = getattr(episode, "seasonNumber", None) or getattr(episode, "parentIndex", None)
        ep_num = getattr(episode, "episodeNumber", None) or getattr(episode, "index", None)

        show_data = self._find_show_data(show_title)
        if not show_data:
            return None

        if season_num is not None and ep_num is not None:
            season_key = f"season_{season_num}"
            if season_key in show_data and isinstance(show_data[season_key], dict):
                episode_data = show_data[season_key].get(str(ep_num))
                if isinstance(episode_data, dict) and "start" in episode_data and "end" in episode_data:
                    return (int(episode_data["start"] * 1000), int(episode_data["end"] * 1000))

        default_entry = show_data.get("default")
        if isinstance(default_entry, dict) and "start" in default_entry and "end" in default_entry:
            return (int(default_entry["start"] * 1000), int(default_entry["end"] * 1000))
        return None
