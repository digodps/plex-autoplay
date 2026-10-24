"""Generic TV-show inventory models and filename episode parsing."""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
import re
from typing import Optional, Tuple


@dataclass(frozen=True)
class TVLibrary:
    name: str
    path: str
    kind: str


@dataclass(frozen=True)
class TVSeries:
    library_name: str
    title: str


@dataclass(frozen=True)
class TVSeason:
    series: TVSeries
    number: int


@dataclass(frozen=True)
class EpisodeNumber:
    season: int
    episode: int


@dataclass(frozen=True)
class EpisodeParseResult:
    episodes: Tuple[EpisodeNumber, ...]
    ambiguity: Optional[str] = None


@dataclass(frozen=True)
class TVEpisode:
    season: TVSeason
    number: int
    path: str
    extension: str


_STANDARD_EPISODE_RE = re.compile(
    r"(?<![A-Za-z0-9])S(?P<season>\d{1,3})E(?P<start>\d{1,3})"
    r"(?:\s*-\s*E?(?P<end>\d{1,3}))?(?!\d)",
    re.IGNORECASE,
)
_X_EPISODE_RE = re.compile(
    r"(?<![A-Za-z0-9])(?P<season>\d{1,3})x(?P<episode>\d{1,3})(?!\d)",
    re.IGNORECASE,
)
_CONTEXTUAL_EPISODE_RE = re.compile(r"(?<![A-Za-z0-9])E(?P<episode>\d{1,3})(?!\d)", re.IGNORECASE)
_LEGACY_EPISODE_RE = re.compile(r"-(?P<episode>\d{1,3})$")
_TITLE_SEASON_EPISODE_RE = re.compile(
    r"^(?P<title>.+?)\s+(?P<season>\d{1,3})\s*-\s*(?P<episode>\d{1,3})$"
)


def parse_episode_numbers(filename: str, season_context: Optional[int] = None) -> Optional[EpisodeParseResult]:
    """Parse a filename episode identifier, optionally using its season folder."""
    stem = Path(filename).stem
    match = _TITLE_SEASON_EPISODE_RE.fullmatch(stem)
    if match:
        season = int(match.group("season"))
        episode = int(match.group("episode"))
        if season < 1 or episode < 1:
            return None
        return _episode_result(season, episode, episode, season_context)

    match = _STANDARD_EPISODE_RE.search(stem)
    if match:
        season = int(match.group("season"))
        first_episode = int(match.group("start"))
        last_episode = int(match.group("end") or first_episode)
        if season < 1 or first_episode < 1 or last_episode < first_episode:
            return None
        return _episode_result(season, first_episode, last_episode, season_context)

    match = _X_EPISODE_RE.search(stem)
    if match:
        season = int(match.group("season"))
        episode = int(match.group("episode"))
        if season < 1 or episode < 1:
            return None
        return _episode_result(season, episode, episode, season_context)

    if season_context is None or season_context < 1:
        return None

    match = _CONTEXTUAL_EPISODE_RE.search(stem) or _LEGACY_EPISODE_RE.search(stem)
    if not match:
        return None
    episode = int(match.group("episode"))
    if episode < 1:
        return None
    return EpisodeParseResult((EpisodeNumber(season_context, episode),))


def _episode_result(
    season: int,
    first_episode: int,
    last_episode: int,
    season_context: Optional[int],
) -> EpisodeParseResult:
    ambiguity = None
    if season_context is not None and season_context > 0 and season != season_context:
        ambiguity = f"temporada do arquivo ({season}) diverge da pasta ({season_context})"
    episodes = tuple(EpisodeNumber(season, number) for number in range(first_episode, last_episode + 1))
    return EpisodeParseResult(episodes=episodes, ambiguity=ambiguity)
