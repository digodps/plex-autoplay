"""Build generic TV-show inventory and marker coverage."""

from __future__ import annotations

from dataclasses import dataclass, field
from pathlib import Path
from types import SimpleNamespace
from typing import Any, Callable, Dict, List, Optional

from plex_autoplay.domain.tvshow import TVEpisode, TVLibrary, TVSeason, TVSeries
from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.media.library_repository import LibraryRepository
from plex_autoplay.infrastructure.media.tvshow_scanner import scan_tv_library

ProgressReporter = Callable[[Dict[str, Any]], None]


@dataclass(frozen=True)
class EpisodeCoverage:
    library_name: str
    library_path: str
    library_kind: str
    series_title: str
    season_number: int
    episode_number: int
    path: str
    intro_start_ms: Optional[int]
    intro_end_ms: Optional[int]
    credits_ms: Optional[int]

    @property
    def has_intro(self) -> bool:
        return self.intro_start_ms is not None and self.intro_end_ms is not None

    @property
    def has_credits(self) -> bool:
        return self.credits_ms is not None


@dataclass
class LibraryCatalog:
    library: TVLibrary
    episodes: List[EpisodeCoverage] = field(default_factory=list)
    unrecognized_files: List[str] = field(default_factory=list)
    ambiguities: List[str] = field(default_factory=list)
    pending: List[dict[str, object]] = field(default_factory=list)
    assumptions: List[dict[str, object]] = field(default_factory=list)
    errors: List[str] = field(default_factory=list)


class TVShowCatalog:
    def __init__(self, libraries: LibraryRepository, markers: JsonMarkerRepository):
        self.libraries = libraries
        self.markers = markers

    def build(self, progress_callback: Optional[ProgressReporter] = None) -> List[LibraryCatalog]:
        catalogs = []
        libraries = self.libraries.list_libraries()
        directories_scanned = 0
        files_scanned = 0
        for index, library in enumerate(libraries):
            completed_libraries = index
            if progress_callback is not None:
                progress_callback(
                    {
                        "stage": "inventory",
                        "message": f"Preparando a varredura da biblioteca {library.name}.",
                        "library": library.name,
                        "current_directory": None,
                        "directories_scanned": directories_scanned,
                        "files_scanned": files_scanned,
                        "completed_libraries": completed_libraries,
                        "total_libraries": len(libraries),
                    }
                )

            def report_directory(relative_directory: str, library_files_scanned: int) -> None:
                nonlocal directories_scanned, files_scanned, files_scanned_in_library
                directories_scanned += 1
                files_scanned += max(0, library_files_scanned - files_scanned_in_library)
                files_scanned_in_library = library_files_scanned
                if progress_callback is not None:
                    progress_callback(
                        {
                            "stage": "inventory",
                            "message": f"Varrendo arquivos em {library.name}/{relative_directory}.",
                            "library": library.name,
                            "current_directory": relative_directory,
                            "directories_scanned": directories_scanned,
                            "files_scanned": files_scanned,
                            "completed_libraries": completed_libraries,
                            "total_libraries": len(libraries),
                        }
                    )

            files_scanned_in_library = 0
            catalogs.append(self._build_library(library, report_directory))
            if progress_callback is not None:
                progress_callback(
                    {
                        "stage": "inventory",
                        "message": f"Inventário da biblioteca {library.name} concluído.",
                        "library": library.name,
                        "current_directory": None,
                        "directories_scanned": directories_scanned,
                        "files_scanned": files_scanned,
                        "completed_libraries": index + 1,
                        "total_libraries": len(libraries),
                    }
                )
        return catalogs

    def build_report(
        self,
        added_by_library: Optional[dict[str, list[dict[str, object]]]] = None,
        preserved_by_library: Optional[dict[str, list[dict[str, object]]]] = None,
        catalogs: Optional[List[LibraryCatalog]] = None,
    ) -> dict[str, object]:
        added_by_library = added_by_library or {}
        preserved_by_library = preserved_by_library or {}
        library_reports = []
        catalogs = catalogs if catalogs is not None else self.build()
        for catalog in catalogs:
            missing = []
            for episode in catalog.episodes:
                missing_fields = []
                if episode.intro_start_ms is None:
                    missing_fields.append("start")
                if episode.intro_end_ms is None:
                    missing_fields.append("end")
                if episode.credits_ms is None:
                    missing_fields.append("credits")
                if missing_fields:
                    missing.append(
                        {
                            "series": episode.series_title,
                            "season": episode.season_number,
                            "episode": episode.episode_number,
                            "path": episode.path,
                            "fields": missing_fields,
                        }
                    )
            library_reports.append(
                {
                    "name": catalog.library.name,
                    "path": catalog.library.path,
                    "kind": catalog.library.kind,
                    "episode_count": len(catalog.episodes),
                    "with_intro": sum(episode.has_intro for episode in catalog.episodes),
                    "with_credits": sum(episode.has_credits for episode in catalog.episodes),
                    "episodes": [
                        {
                            "series": episode.series_title,
                            "season": episode.season_number,
                            "episode": episode.episode_number,
                            "path": episode.path,
                            "intro": {
                                "start_ms": episode.intro_start_ms,
                                "end_ms": episode.intro_end_ms,
                            },
                            "credits_ms": episode.credits_ms,
                        }
                        for episode in catalog.episodes
                    ],
                    "added": added_by_library.get(catalog.library.path, []),
                    "preserved": preserved_by_library.get(catalog.library.path, []),
                    "missing": missing,
                    "unrecognized": catalog.unrecognized_files,
                    "ambiguities": catalog.ambiguities,
                    "pending": catalog.pending,
                    "assumptions": catalog.assumptions,
                    "errors": catalog.errors,
                }
            )
        return {"libraries": library_reports}

    def refresh_markers(self, catalogs: List[LibraryCatalog]) -> None:
        for catalog in catalogs:
            catalog.episodes = [
                self._coverage(
                    catalog.library,
                    TVEpisode(
                        season=TVSeason(
                            series=TVSeries(
                                library_name=catalog.library.name,
                                title=episode.series_title,
                            ),
                            number=episode.season_number,
                        ),
                        number=episode.episode_number,
                        path=episode.path,
                        extension=Path(episode.path).suffix.lower(),
                    ),
                )
                for episode in catalog.episodes
            ]

    def _build_library(
        self,
        library: TVLibrary,
        progress_callback: Optional[Callable[[str, int], None]] = None,
    ) -> LibraryCatalog:
        catalog = LibraryCatalog(library=library)
        scan_library = TVLibrary(
            name=library.name,
            path=str(self.libraries.media_root / library.path),
            kind=library.kind,
        )
        try:
            scan = scan_tv_library(scan_library, progress_callback=progress_callback)
        except (OSError, RuntimeError, ValueError) as error:
            catalog.errors.append(str(error))
            return catalog
        catalog.unrecognized_files = scan.unrecognized_files
        catalog.ambiguities = scan.ambiguities
        catalog.errors = scan.errors
        for episode in scan.episodes:
            try:
                catalog.episodes.append(self._coverage(library, episode))
            except ValueError as error:
                catalog.errors.append(f"{episode.path}: {error}")
        return catalog

    def _coverage(self, library: TVLibrary, episode: TVEpisode) -> EpisodeCoverage:
        series_title = episode.season.series.title
        plex_episode = SimpleNamespace(
            grandparentTitle=series_title,
            showTitle=series_title,
            seasonNumber=episode.season.number,
            parentIndex=episode.season.number,
            episodeNumber=episode.number,
            index=episode.number,
            markers=[],
        )
        values = self.markers.get_custom_marker_values(plex_episode)
        return EpisodeCoverage(
            library_name=library.name,
            library_path=library.path,
            library_kind=library.kind,
            series_title=series_title,
            season_number=episode.season.number,
            episode_number=episode.number,
            path=episode.path,
            intro_start_ms=int(values["start"] * 1000) if "start" in values else None,
            intro_end_ms=int(values["end"] * 1000) if "end" in values else None,
            credits_ms=int(values["credits"] * 1000) if "credits" in values else None,
        )
