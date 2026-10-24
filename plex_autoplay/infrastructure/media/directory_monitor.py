"""Discovery and marker generation for local anime media libraries."""

from __future__ import annotations

from dataclasses import dataclass
from email.utils import parsedate_to_datetime
import json
import logging
import math
import os
import re
import shutil
import subprocess
import tempfile
import time
from pathlib import Path
from typing import Any, Callable, Dict, Iterable, List, Optional
from urllib import error, parse, request

logger = logging.getLogger("plex-autoplay.media")

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
SEASON_RE = re.compile(r"^temporada\s*(\d+)$", re.IGNORECASE)
EPISODE_RE = re.compile(r"-(\d{1,3})$")
MAX_RESPONSE_BYTES = 1024 * 1024


class ExternalApiError(RuntimeError):
    """An explicit failure returned while communicating with a public API."""

    def __init__(self, message: str, status_code: Optional[int] = None, retry_after: Optional[str] = None):
        super().__init__(message)
        self.status_code = status_code
        self.retry_after = retry_after


@dataclass(frozen=True)
class MediaEpisode:
    title: str
    season: int
    episode: int
    path: str
    extension: str


def _safe_path_within_root(root: Path, candidate: Path) -> bool:
    try:
        candidate.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def _parse_season_name(value: str) -> Optional[int]:
    match = SEASON_RE.match(value.strip())
    if not match:
        return None
    season_number = int(match.group(1))
    return season_number if season_number > 0 else None


def _parse_episode_file(path: Path, root: Path) -> Optional[MediaEpisode]:
    if not _safe_path_within_root(root, path):
        logger.info("Ignorando arquivo fora da raiz de mídia: %s", path)
        return None
    if not path.is_file() or path.suffix.lower() not in VIDEO_EXTENSIONS:
        return None

    season_number = _parse_season_name(path.parent.name)
    series_dir = path.parent.parent
    if season_number is None or series_dir.parent != root:
        return None

    match = EPISODE_RE.search(path.stem)
    if not match:
        logger.info("Ignorando arquivo sem sufixo de episódio reconhecido: %s", path)
        return None

    title = series_dir.name.strip()
    episode_number = int(match.group(1))
    if not title or episode_number < 1:
        return None

    return MediaEpisode(
        title=title,
        season=season_number,
        episode=episode_number,
        path=str(path.resolve()),
        extension=path.suffix.lower(),
    )


def scan_media_root(media_root: str | Path) -> List[MediaEpisode]:
    root = Path(media_root).expanduser().resolve()
    if not root.exists() or not root.is_dir():
        raise ValueError(f"Diretório de mídia inválido: {media_root}")

    discovered: List[MediaEpisode] = []
    for show_dir in sorted(root.iterdir(), key=lambda item: item.name.lower()):
        if not _safe_path_within_root(root, show_dir) or not show_dir.is_dir():
            continue
        for season_dir in sorted(show_dir.iterdir(), key=lambda item: item.name.lower()):
            if not _safe_path_within_root(root, season_dir) or not season_dir.is_dir():
                continue
            for file_path in sorted(season_dir.iterdir(), key=lambda item: item.name.lower()):
                if file_path.is_file():
                    episode = _parse_episode_file(file_path, root)
                    if episode is not None:
                        discovered.append(episode)
    return discovered


def build_series_map(media_root: str | Path) -> Dict[str, List[MediaEpisode]]:
    items: Dict[str, List[MediaEpisode]] = {}
    for episode in scan_media_root(media_root):
        items.setdefault(episode.title, []).append(episode)
    for episodes in items.values():
        episodes.sort(key=lambda item: (item.season, item.episode, item.path))
    return items


def _http_json(url: str, timeout: int = 10) -> Dict[str, Any]:
    headers = {
        "Accept": "application/json",
        "User-Agent": "plex-autoplay/auto-markers",
    }
    request_obj = request.Request(url, headers=headers)
    try:
        with request.urlopen(request_obj, timeout=timeout) as response:
            content_type = response.headers.get("Content-Type", "")
            if "application/json" not in content_type.lower():
                raise ExternalApiError(f"Resposta da API com Content-Type inesperado: {content_type or 'ausente'}")
            payload = response.read(MAX_RESPONSE_BYTES + 1)
    except error.HTTPError as exc:
        retry_after = exc.headers.get("Retry-After") if exc.headers else None
        exc.close()
        raise ExternalApiError(
            f"API respondeu HTTP {exc.code}" + (f"; Retry-After: {retry_after}" if retry_after else ""),
            status_code=exc.code,
            retry_after=retry_after,
        ) from exc
    except error.URLError as exc:
        raise ExternalApiError(f"Falha de conexão com API pública: {exc.reason}") from exc
    except TimeoutError as exc:
        raise ExternalApiError("Tempo limite excedido ao consultar API pública") from exc

    if len(payload) > MAX_RESPONSE_BYTES:
        raise ExternalApiError(f"Resposta da API excede o limite de {MAX_RESPONSE_BYTES} bytes")
    try:
        result = json.loads(payload.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError) as exc:
        raise ExternalApiError("API retornou JSON inválido") from exc
    if not isinstance(result, dict):
        raise ExternalApiError("API retornou um JSON com formato inesperado")
    return result


def _normalize_candidates(items: Iterable[Any]) -> List[Dict[str, Any]]:
    return [item for item in items if isinstance(item, dict)]


def _match_candidate_title(title: str, candidate: Dict[str, Any]) -> bool:
    normalized_title = title.strip().casefold()
    candidate_titles = (
        candidate.get("title"),
        candidate.get("title_english"),
        candidate.get("title_japanese"),
    )
    return any(
        isinstance(candidate_title, str) and candidate_title.strip().casefold() == normalized_title
        for candidate_title in candidate_titles
    )


def search_jikan_candidates(title: str, limit: int = 5) -> List[Dict[str, Any]]:
    if not title or not title.strip():
        return []
    query = parse.urlencode({"q": title.strip(), "limit": limit})
    payload = _http_json(f"https://api.jikan.moe/v4/anime?{query}")
    return _normalize_candidates(payload.get("data", []))


def select_jikan_candidate(title: str, candidates: Iterable[Dict[str, Any]]) -> Optional[Dict[str, Any]]:
    candidate_list = list(candidates)
    exact_matches = [candidate for candidate in candidate_list if _match_candidate_title(title, candidate)]
    return exact_matches[0] if len(exact_matches) == 1 else None


def fetch_aniskip_times(mal_id: int, episode: int, episode_length_seconds: float) -> Dict[str, Any]:
    if not math.isfinite(episode_length_seconds) or episode_length_seconds <= 0:
        raise ValueError("A duração do episódio deve ser positiva e finita.")
    query = parse.urlencode(
        [
            ("types", "op"),
            ("types", "ed"),
            ("episodeLength", str(int(episode_length_seconds))),
        ]
    )
    payload = _http_json(f"https://api.aniskip.com/v2/skip-times/{mal_id}/{episode}?{query}")
    results = payload.get("results", [])
    if not isinstance(results, list):
        raise ExternalApiError("AniSkip retornou results com formato inválido")

    normalized: Dict[str, Any] = {}
    for result in results:
        if not isinstance(result, dict):
            continue
        skip_type = result.get("skipType")
        interval = result.get("interval")
        if skip_type not in {"op", "ed"} or not isinstance(interval, dict):
            continue
        start = interval.get("startTime")
        end = interval.get("endTime")
        if (
            isinstance(start, bool)
            or isinstance(end, bool)
            or not isinstance(start, (int, float))
            or not isinstance(end, (int, float))
        ):
            continue
        start_seconds = float(start)
        end_seconds = float(end)
        if (
            not math.isfinite(start_seconds)
            or not math.isfinite(end_seconds)
            or start_seconds < 0
            or end_seconds <= start_seconds
            or end_seconds > episode_length_seconds
        ):
            logger.warning("Ignorando intervalo AniSkip inválido para %s/%s", mal_id, episode)
            continue
        normalized[skip_type] = {"start": start_seconds, "end": end_seconds}
    return normalized


def get_media_duration_seconds(media_path: str | Path) -> float:
    ffprobe_path = shutil.which("ffprobe")
    if ffprobe_path is None:
        raise RuntimeError("ffprobe não está instalado; não é possível determinar a duração da mídia.")
    result = subprocess.run(
        [
            ffprobe_path,
            "-v",
            "error",
            "-show_entries",
            "format=duration",
            "-of",
            "default=noprint_wrappers=1:nokey=1",
            str(media_path),
        ],
        check=True,
        capture_output=True,
        text=True,
        timeout=15,
    )
    try:
        duration = float(result.stdout.strip())
    except ValueError as exc:
        raise RuntimeError(f"ffprobe retornou duração inválida para {media_path}") from exc
    if not math.isfinite(duration) or duration <= 0:
        raise RuntimeError(f"ffprobe retornou duração inválida para {media_path}")
    return duration


def load_cache(cache_file: str | Path) -> Dict[str, Any]:
    cache_path = Path(cache_file).expanduser()
    if not cache_path.exists():
        return {}
    try:
        with cache_path.open("r", encoding="utf-8") as handle:
            payload = json.load(handle)
    except (OSError, UnicodeDecodeError, json.JSONDecodeError) as exc:
        raise RuntimeError(f"Não foi possível ler o cache {cache_path}: {exc}") from exc
    if not isinstance(payload, dict):
        raise RuntimeError(f"O cache {cache_path} precisa conter um objeto JSON.")
    for key in ("entries", "anime_mappings", "season_mappings"):
        if key in payload and not isinstance(payload[key], dict):
            raise RuntimeError(f"A propriedade {key} no cache {cache_path} precisa ser um objeto JSON.")
    return payload


def save_cache(cache_file: str | Path, payload: Dict[str, Any]) -> None:
    cache_path = Path(cache_file).expanduser()
    cache_path.parent.mkdir(parents=True, exist_ok=True)
    temp_name: Optional[str] = None
    try:
        with tempfile.NamedTemporaryFile(
            "w", encoding="utf-8", dir=str(cache_path.parent), delete=False
        ) as handle:
            temp_name = handle.name
            json.dump(payload, handle, ensure_ascii=False, indent=2)
            handle.flush()
            os.fsync(handle.fileno())
        Path(temp_name).replace(cache_path)
    finally:
        if temp_name and Path(temp_name).exists():
            Path(temp_name).unlink()


def _cache_get(cache: Dict[str, Any], key: str, now: float) -> Optional[Any]:
    entries = cache.setdefault("entries", {})
    if not isinstance(entries, dict):
        raise RuntimeError("A propriedade entries no cache precisa ser um objeto JSON.")
    entry = entries.get(key)
    if not isinstance(entry, dict):
        return None
    expires_at = entry.get("expires_at")
    if isinstance(expires_at, bool) or not isinstance(expires_at, (int, float)) or expires_at < now:
        entries.pop(key, None)
        return None
    return entry.get("value")


def _cache_set(cache: Dict[str, Any], key: str, value: Any, now: float, ttl_seconds: int) -> None:
    entries = cache.setdefault("entries", {})
    if not isinstance(entries, dict):
        raise RuntimeError("A propriedade entries no cache precisa ser um objeto JSON.")
    entries[key] = {
        "expires_at": now + ttl_seconds,
        "value": value,
    }


def _retry_deadline(retry_after: Optional[str], now: float) -> float:
    if retry_after:
        try:
            return now + max(0, int(retry_after))
        except ValueError:
            try:
                return max(now, parsedate_to_datetime(retry_after).timestamp())
            except (TypeError, ValueError, OverflowError):
                pass
    return now + 60


def _api_failure_reason(source: str, exc: ExternalApiError) -> str:
    if exc.status_code == 404:
        return f"{source} não encontrou dados para esta consulta (HTTP 404)."
    return f"Falha na consulta {source}: {exc}"


def _select_candidate_interactively(
    title: str,
    candidates: List[Dict[str, Any]],
    chooser: Callable[[str], str],
) -> Optional[Dict[str, Any]]:
    selected = select_jikan_candidate(title, candidates)
    if selected is not None or not candidates:
        return selected

    print(f"Candidatos encontrados para {title}:")
    for index, candidate in enumerate(candidates, start=1):
        titles = " / ".join(
            str(candidate.get(key))
            for key in ("title", "title_english", "title_japanese")
            if candidate.get(key)
        )
        print(f"  {index}. {titles} (MAL ID: {candidate.get('mal_id', 'inválido')})")
    answer = chooser("Escolha o número do candidato (ou Enter para deixar pendente): ").strip()
    if not answer:
        return None
    try:
        index = int(answer) - 1
    except ValueError:
        return None
    return candidates[index] if 0 <= index < len(candidates) else None


def build_preview_for_root(
    media_root: str | Path,
    cache_file: str | Path = ".anime-markers-cache.json",
    cache_ttl_seconds: int = 7 * 24 * 60 * 60,
    chooser: Optional[Callable[[str], str]] = None,
    duration_reader: Callable[[str | Path], float] = get_media_duration_seconds,
    auto_guess: bool = False,
) -> Dict[str, Any]:
    if cache_ttl_seconds < 0:
        raise ValueError("O TTL do cache não pode ser negativo.")
    cache = load_cache(cache_file)
    series_map = build_series_map(media_root)
    preview: Dict[str, Any] = {"series": {}, "pending": [], "details": [], "assumptions": []}
    now = time.time()
    retry_until = cache.get("api_retry_until", 0)
    if (
        not isinstance(retry_until, bool)
        and isinstance(retry_until, (int, float))
        and retry_until > now
    ):
        preview["pending"].append(
            {
                "reason": (
                    "Consultas pausadas devido ao limite da API até "
                    f"{time.strftime('%Y-%m-%d %H:%M:%S UTC', time.gmtime(retry_until))}."
                )
            }
        )
        save_cache(cache_file, cache)
        return preview
    rate_limited = False

    for show_title, episodes in sorted(series_map.items()):
        if rate_limited:
            break
        mapping_key = show_title.casefold()
        anime_map = cache.setdefault("anime_mappings", {})
        if not isinstance(anime_map, dict):
            raise RuntimeError("A propriedade anime_mappings no cache precisa ser um objeto JSON.")
        mal_id = anime_map.get(mapping_key)
        if isinstance(mal_id, bool) or (mal_id is not None and (not isinstance(mal_id, int) or mal_id <= 0)):
            preview["pending"].append({"title": show_title, "reason": "Mapeamento MAL inválido no cache local."})
            continue
        search_key = f"jikan:{mapping_key}"
        candidates = _cache_get(cache, search_key, now)
        if mal_id is None:
            if candidates is None:
                try:
                    candidates = search_jikan_candidates(show_title)
                    _cache_set(cache, search_key, candidates, now, cache_ttl_seconds)
                except ExternalApiError as exc:
                    preview["pending"].append(
                        {"title": show_title, "reason": _api_failure_reason("Jikan", exc)}
                    )
                    if exc.status_code == 429:
                        cache["api_retry_until"] = _retry_deadline(exc.retry_after, now)
                        rate_limited = True
                    continue
            selected = select_jikan_candidate(show_title, candidates)
            if selected is None and candidates and auto_guess:
                selected = candidates[0]
                preview["assumptions"].append(
                    {
                        "title": show_title,
                        "mal_id": selected.get("mal_id"),
                        "reason": "Usado o primeiro candidato Jikan por modo de estimativa automática.",
                    }
                )
            if selected is None and chooser is not None and candidates:
                selected = _select_candidate_interactively(show_title, candidates, chooser)
            if selected is None:
                reason = (
                    "Identificação ambígua; escolha um candidato na próxima execução."
                    if candidates
                    else "Nenhum candidato encontrado na API pública."
                )
                preview["pending"].append({"title": show_title, "reason": reason})
                continue
            mal_id = selected.get("mal_id")
            if isinstance(mal_id, bool) or not isinstance(mal_id, int) or mal_id <= 0:
                preview["pending"].append({"title": show_title, "reason": "Candidato sem MAL ID válido."})
                continue
            anime_map[mapping_key] = mal_id

        season_mappings = cache.setdefault("season_mappings", {})
        if not isinstance(season_mappings, dict):
            raise RuntimeError("A propriedade season_mappings no cache precisa ser um objeto JSON.")
        for season_number in sorted({episode.season for episode in episodes}):
            season_key = f"{mal_id}:{season_number}"
            episode_offset = season_mappings.get(season_key)
            if isinstance(episode_offset, bool) or not isinstance(episode_offset, int):
                if auto_guess:
                    episode_offset = 0
                    season_mappings[season_key] = episode_offset
                    preview["assumptions"].append(
                        {
                            "title": show_title,
                            "season": season_number,
                            "reason": "Assumido offset de episódio 0 para esta temporada.",
                        }
                    )
                elif chooser is None:
                    preview["pending"].append(
                        {
                            "title": show_title,
                            "season": season_number,
                            "reason": "Mapeamento da temporada local para a numeração AniSkip requer confirmação.",
                        }
                    )
                    continue
            if not isinstance(episode_offset, int):
                answer = chooser(
                    f"Offset de episódio da temporada local {season_number} para AniSkip "
                    "(ex.: 0 para manter a numeração; Enter para pendente): "
                ).strip()
                if not answer:
                    preview["pending"].append(
                        {
                            "title": show_title,
                            "season": season_number,
                            "reason": "Mapeamento da temporada não confirmado.",
                        }
                    )
                    continue
                try:
                    episode_offset = int(answer)
                except ValueError:
                    preview["pending"].append(
                        {
                            "title": show_title,
                            "season": season_number,
                            "reason": "Offset de episódio inválido.",
                        }
                    )
                    continue
                season_mappings[season_key] = episode_offset

            for media_episode in (item for item in episodes if item.season == season_number):
                episode_length: float
                try:
                    episode_length = duration_reader(media_episode.path)
                    if not math.isfinite(episode_length) or episode_length <= 0:
                        raise ValueError("duração inválida")
                except (OSError, RuntimeError, subprocess.SubprocessError, ValueError) as exc:
                    preview["pending"].append(
                        {
                            "title": show_title,
                            "season": season_number,
                            "episode": media_episode.episode,
                            "reason": f"Não foi possível obter a duração real da mídia: {exc}",
                        }
                    )
                    continue

                mapped_episode = media_episode.episode + episode_offset
                if mapped_episode <= 0:
                    preview["pending"].append(
                        {
                            "title": show_title,
                            "season": season_number,
                            "episode": media_episode.episode,
                            "reason": "O mapeamento resultou em número de episódio inválido.",
                        }
                    )
                    continue

                response_key = f"aniskip:{mal_id}:{mapped_episode}:{int(episode_length)}"
                skip_times = _cache_get(cache, response_key, now)
                if skip_times is None:
                    try:
                        skip_times = fetch_aniskip_times(mal_id, mapped_episode, episode_length)
                        _cache_set(cache, response_key, skip_times, now, cache_ttl_seconds)
                    except ExternalApiError as exc:
                        preview["pending"].append(
                            {
                                "title": show_title,
                                "season": season_number,
                                "episode": media_episode.episode,
                                "reason": _api_failure_reason("AniSkip", exc),
                                "retry_after": exc.retry_after,
                            }
                        )
                        if exc.status_code == 429:
                            cache["api_retry_until"] = _retry_deadline(exc.retry_after, now)
                            rate_limited = True
                            break
                        continue

                entry: Dict[str, float] = {}
                op_segment = skip_times.get("op", {}) if isinstance(skip_times, dict) else {}
                ed_segment = skip_times.get("ed", {}) if isinstance(skip_times, dict) else {}
                if isinstance(op_segment, dict):
                    if "start" in op_segment:
                        entry["start"] = float(op_segment["start"])
                    if "end" in op_segment:
                        entry["end"] = float(op_segment["end"])
                if isinstance(ed_segment, dict) and "start" in ed_segment:
                    entry["credits"] = float(ed_segment["start"])
                if not entry:
                    preview["pending"].append(
                        {
                            "title": show_title,
                            "season": season_number,
                            "episode": media_episode.episode,
                            "reason": "AniSkip não possui marcadores para este episódio.",
                        }
                    )
                    continue

                season_markers = preview["series"].setdefault(show_title, {}).setdefault(
                    f"season_{season_number}", {}
                )
                season_markers[str(media_episode.episode)] = entry
                preview["details"].append(
                    {
                        "title": show_title,
                        "mal_id": mal_id,
                        "season": season_number,
                        "episode": media_episode.episode,
                        "aniskip_episode": mapped_episode,
                        "duration_seconds": episode_length,
                        "markers": entry,
                    }
                )
            if rate_limited:
                break

    if rate_limited:
        retry_until = cache.get("api_retry_until")
        preview["pending"].append(
            {
                "reason": (
                    "As demais consultas desta varredura foram adiadas por limite da API até "
                    f"{time.strftime('%Y-%m-%d %H:%M:%S UTC', time.gmtime(retry_until))}."
                )
            }
        )

    if cache_file:
        save_cache(cache_file, cache)
    return preview


def merge_markers_file(
    path: str | Path,
    new_series: Dict[str, Any],
    force: bool = False,
) -> Dict[str, Any]:
    markers_path = Path(path).expanduser()
    markers_path.parent.mkdir(parents=True, exist_ok=True)
    if markers_path.exists():
        try:
            with markers_path.open("r", encoding="utf-8") as handle:
                existing = json.load(handle)
        except (OSError, UnicodeDecodeError, json.JSONDecodeError) as exc:
            raise RuntimeError(f"Não foi possível ler os marcadores atuais {markers_path}: {exc}") from exc
        if not isinstance(existing, dict):
            raise RuntimeError(f"O arquivo de marcadores {markers_path} precisa conter um objeto JSON.")
    else:
        existing = {}

    for show_title, show_data in new_series.items():
        if not isinstance(show_data, dict):
            continue
        matching_titles = [
            title
            for title, value in existing.items()
            if isinstance(title, str)
            and title.strip().casefold() == show_title.strip().casefold()
            and isinstance(value, dict)
        ]
        if len(matching_titles) > 1:
            raise RuntimeError(f"Mais de uma chave de série corresponde a '{show_title}'.")
        show_key = matching_titles[0] if matching_titles else show_title
        existing_show = existing.setdefault(show_key, {})
        for season_key, episodes in show_data.items():
            if not isinstance(episodes, dict):
                continue
            season_data = existing_show.setdefault(season_key, {})
            for episode_key, entry in episodes.items():
                if not isinstance(entry, dict):
                    continue
                existing_entry = season_data.setdefault(str(episode_key), {})
                if not isinstance(existing_entry, dict):
                    if not force:
                        continue
                    existing_entry = {}
                    season_data[str(episode_key)] = existing_entry
                for field, value in entry.items():
                    if field not in existing_entry or force:
                        existing_entry[field] = value

    temp_name: Optional[str] = None
    try:
        with tempfile.NamedTemporaryFile(
            "w", encoding="utf-8", dir=str(markers_path.parent), delete=False
        ) as handle:
            temp_name = handle.name
            json.dump(existing, handle, ensure_ascii=False, indent=2)
            handle.flush()
            os.fsync(handle.fileno())
        Path(temp_name).replace(markers_path)
    finally:
        if temp_name and Path(temp_name).exists():
            Path(temp_name).unlink()
    return existing
