"""Filesystem discovery for generic TV-show libraries."""

from __future__ import annotations

from dataclasses import dataclass, field
import os
from pathlib import Path
import re
from typing import Callable, List, Optional

from plex_autoplay.domain.tvshow import TVEpisode, TVLibrary, TVSeason, TVSeries, parse_episode_numbers


VIDEO_EXTENSIONS = {
    ".avi",
    ".m2ts",
    ".m4v",
    ".mkv",
    ".mov",
    ".mp4",
    ".mpeg",
    ".mpg",
    ".mts",
    ".ts",
    ".wmv",
}
_SEASON_DIRECTORY_RE = re.compile(r"^(?:season|temporada)\s*(\d+)$", re.IGNORECASE)


@dataclass
class TVLibraryScan:
    episodes: List[TVEpisode] = field(default_factory=list)
    unrecognized_files: List[str] = field(default_factory=list)
    ambiguities: List[str] = field(default_factory=list)
    errors: List[str] = field(default_factory=list)


def _season_number(directory_name: str) -> Optional[int]:
    match = _SEASON_DIRECTORY_RE.fullmatch(directory_name.strip())
    if not match:
        return None
    number = int(match.group(1))
    return number if number > 0 else None


def _is_within(root: Path, path: Path) -> bool:
    try:
        path.resolve().relative_to(root)
        return True
    except (OSError, ValueError):
        return False


def _series_title(root: Path, relative_parts: tuple[str, ...], season_index: Optional[int]) -> str:
    if season_index is not None:
        return relative_parts[season_index - 1] if season_index > 0 else root.name
    if len(relative_parts) > 1:
        return relative_parts[0]
    return root.name


def scan_tv_library(
    library: TVLibrary,
    progress_callback: Optional[Callable[[str, int], None]] = None,
) -> TVLibraryScan:
    root = Path(library.path).expanduser().resolve()
    if not root.exists() or not root.is_dir():
        raise ValueError(f"Diretório de biblioteca inválido: {library.path}")

    result = TVLibraryScan()
    scanned_video_files = 0

    def record_walk_error(error: OSError) -> None:
        result.errors.append(f"{getattr(error, 'filename', root)}: {error}")

    for current, directory_names, file_names in os.walk(
        root,
        topdown=True,
        onerror=record_walk_error,
        followlinks=False,
    ):
        current_path = Path(current)
        directory_names[:] = [
            name
            for name in directory_names
            if _is_within(root, current_path / name) and not (current_path / name).is_symlink()
        ]
        for filename in sorted(file_names, key=str.casefold):
            file_path = current_path / filename
            if file_path.suffix.lower() not in VIDEO_EXTENSIONS:
                continue
            try:
                if not file_path.is_file() or not _is_within(root, file_path):
                    continue
                scanned_video_files += 1
                relative_parts = file_path.relative_to(root).parts
                season_directories = [
                    (index, _season_number(part))
                    for index, part in enumerate(relative_parts[:-1])
                    if _season_number(part) is not None
                ]
                season_index, season_context = season_directories[-1] if season_directories else (None, None)
                parsed = parse_episode_numbers(filename, season_context=season_context)
                if parsed is None:
                    result.unrecognized_files.append(str(file_path))
                    continue

                title = _series_title(root, relative_parts, season_index)
                if parsed.ambiguity:
                    result.ambiguities.append(f"{file_path}: {parsed.ambiguity}")
                for episode_number in parsed.episodes:
                    series = TVSeries(library_name=library.name, title=title)
                    season = TVSeason(series=series, number=episode_number.season)
                    result.episodes.append(
                        TVEpisode(
                            season=season,
                            number=episode_number.episode,
                            path=str(file_path.resolve()),
                            extension=file_path.suffix.lower(),
                        )
                    )
            except OSError as error:
                result.errors.append(f"{file_path}: {error}")
        if progress_callback is not None:
            relative_directory = current_path.relative_to(root).as_posix() or "."
            progress_callback(relative_directory, scanned_video_files)

    result.episodes.sort(
        key=lambda episode: (
            episode.season.series.title.casefold(),
            episode.season.number,
            episode.number,
            episode.path,
        )
    )
    result.unrecognized_files.sort(key=str.casefold)
    result.ambiguities.sort(key=str.casefold)
    result.errors.sort(key=str.casefold)
    return result
