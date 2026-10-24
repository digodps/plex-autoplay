"""Generic inventory scanning with optional Anime-only marker enrichment."""

from __future__ import annotations

import json
import logging
from pathlib import Path
from typing import Any, Dict, List, Tuple

from plex_autoplay.application.tvshow_catalog import LibraryCatalog, TVShowCatalog
from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.media.directory_monitor import build_preview_for_root, merge_markers_file
from plex_autoplay.infrastructure.media.library_repository import LibraryRepository

logger = logging.getLogger("plex-autoplay.scan")


class DashboardScanService:
    def __init__(
        self,
        libraries: LibraryRepository,
        markers: JsonMarkerRepository,
        markers_file: str | Path,
        cache_file: str | Path,
        cache_ttl_seconds: int,
        auto_generate_anime_markers: bool,
    ):
        self.libraries = libraries
        self.markers = markers
        self.markers_file = Path(markers_file).expanduser()
        self.cache_file = Path(cache_file).expanduser()
        self.cache_ttl_seconds = cache_ttl_seconds
        self.auto_generate_anime_markers = auto_generate_anime_markers
        self.catalog = TVShowCatalog(libraries, markers)

    def scan(self) -> Dict[str, Any]:
        catalogs = self.catalog.build()
        added_by_library: Dict[str, List[Dict[str, object]]] = {}
        preserved_by_library: Dict[str, List[Dict[str, object]]] = {}

        if self.auto_generate_anime_markers:
            for catalog in catalogs:
                if catalog.library.kind != "anime" or catalog.errors:
                    continue
                self._enrich_anime_library(catalog, added_by_library, preserved_by_library)

        self.catalog.refresh_markers(catalogs)
        return self.catalog.build_report(
            added_by_library=added_by_library,
            preserved_by_library=preserved_by_library,
            catalogs=catalogs,
        )

    def _enrich_anime_library(
        self,
        catalog: LibraryCatalog,
        added_by_library: Dict[str, List[Dict[str, object]]],
        preserved_by_library: Dict[str, List[Dict[str, object]]],
    ) -> None:
        root = self.libraries.media_root / catalog.library.path
        try:
            preview = build_preview_for_root(
                root,
                cache_file=self.cache_file,
                cache_ttl_seconds=self.cache_ttl_seconds,
                auto_guess=True,
            )
            catalog.pending.extend(preview.get("pending", []))
            catalog.assumptions.extend(preview.get("assumptions", []))
            generated = preview.get("series", {})
            if not isinstance(generated, dict):
                raise ValueError("Prévia de Anime retornou dados de série inválidos.")
            if not generated:
                return
            previous = self._read_markers()
            merged = merge_markers_file(self.markers_file, generated)
            added, preserved = self._classify_merge(previous, merged, generated)
            added_by_library[catalog.library.path] = added
            preserved_by_library[catalog.library.path] = preserved
            self.markers.load_custom_markers()
        except Exception as error:
            catalog.errors.append(f"Falha no enriquecimento Anime: {error}")
            logger.exception("Falha ao enriquecer a biblioteca Anime %s.", catalog.library.path)

    def _read_markers(self) -> Dict[str, Any]:
        if not self.markers_file.exists():
            return {}
        try:
            with self.markers_file.open("r", encoding="utf-8") as markers_file:
                data = json.load(markers_file)
        except (OSError, json.JSONDecodeError) as error:
            raise ValueError(f"Não foi possível ler marcadores existentes: {error}") from error
        if not isinstance(data, dict):
            raise ValueError("O arquivo de marcadores precisa conter um objeto JSON.")
        return data

    @staticmethod
    def _classify_merge(
        previous: Dict[str, Any],
        merged: Dict[str, Any],
        generated: Dict[str, Any],
    ) -> Tuple[List[Dict[str, object]], List[Dict[str, object]]]:
        added = []
        preserved = []
        for title, seasons in generated.items():
            previous_title = DashboardScanService._matching_title(previous, title)
            merged_title = DashboardScanService._matching_title(merged, title)
            previous_show = previous.get(previous_title, {}) if previous_title is not None else {}
            merged_show = merged.get(merged_title, {}) if merged_title is not None else {}
            if not isinstance(previous_show, dict) or not isinstance(merged_show, dict):
                continue
            for season, episodes in seasons.items():
                if not isinstance(episodes, dict):
                    continue
                previous_season = previous_show.get(season, {})
                merged_season = merged_show.get(season, {})
                if not isinstance(previous_season, dict) or not isinstance(merged_season, dict):
                    continue
                for episode, fields in episodes.items():
                    if not isinstance(fields, dict):
                        continue
                    episode_key = str(episode)
                    old_fields = previous_season.get(episode_key, {})
                    new_fields = merged_season.get(episode_key, {})
                    if not isinstance(old_fields, dict) or not isinstance(new_fields, dict):
                        continue
                    new_values = [key for key, value in fields.items() if key not in old_fields and new_fields.get(key) == value]
                    preserved_values = [key for key in fields if key in old_fields]
                    if new_values:
                        added.append(
                            {"series": title, "season": season, "episode": episode_key, "fields": sorted(new_values)}
                        )
                    if preserved_values:
                        preserved.append(
                            {
                                "series": title,
                                "season": season,
                                "episode": episode_key,
                                "fields": sorted(preserved_values),
                            }
                        )
        return added, preserved

    @staticmethod
    def _matching_title(data: Dict[str, Any], title: str) -> str | None:
        matches = [
            key
            for key in data
            if isinstance(key, str) and key.strip().casefold() == title.strip().casefold()
        ]
        if len(matches) > 1:
            raise ValueError(f"Mais de uma chave de série corresponde a '{title}'.")
        return matches[0] if matches else None
