"""Plex server connection, session access, and remote player control."""

import logging
from typing import Any

import requests

try:
    from plexapi.client import PlexClient
    from plexapi.server import PlexServer
    from plexapi.video import Episode
except ImportError:
    PlexClient = object  # type: ignore
    PlexServer = object  # type: ignore
    Episode = object  # type: ignore

logger = logging.getLogger("plex-autoplay.client")


class PlexManager:
    """Implements the application gateway using PlexAPI."""

    def __init__(self, url: str, token: str):
        self.url = url.rstrip("/")
        self.token = token
        self.server: Any | None = None

    def connect(self) -> Any:
        logger.info("Conectando ao Plex Media Server em: %s", self.url)
        try:
            self.server = PlexServer(self.url, self.token)
            logger.info(
                "Conectado com sucesso ao servidor '%s' (Versão: %s, Plataforma: %s)",
                self.server.friendlyName,
                self.server.version,
                self.server.platform,
            )
            return self.server
        except Exception as error:
            logger.error("Falha ao conectar com o Plex Media Server: %s", error)
            raise

    def get_server(self) -> Any:
        if self.server is None:
            return self.connect()
        return self.server

    def get_active_sessions(self) -> list[Any]:
        try:
            return self.get_server().sessions()
        except (requests.RequestException, OSError) as error:
            logger.warning("Erro ao consultar sessões ativas: %s", error)
            return []

    def list_show_episodes(self, library_name: str, show_title: str) -> list[Any]:
        sections = [
            section
            for section in self.get_server().library.sections()
            if getattr(section, "type", None) == "show"
        ]
        preferred_sections = [
            section
            for section in sections
            if str(getattr(section, "title", "")).strip().casefold()
            == library_name.strip().casefold()
        ]
        search_sections = preferred_sections + [
            section for section in sections if section not in preferred_sections
        ]
        matching_shows: list[Any] = []
        for section in search_sections:
            shows = section.search(title=show_title, libtype="show")
            matching_shows.extend(
                show
                for show in shows
                if str(getattr(show, "title", "")).strip().casefold()
                == show_title.strip().casefold()
            )
            if matching_shows and section in preferred_sections:
                break

        if len(matching_shows) != 1:
            if len(matching_shows) > 1:
                logger.warning(
                    "Série '%s' encontrada em mais de uma seção Plex; marcadores nativos não foram associados.",
                    show_title,
                )
            return []
        return matching_shows[0].episodes()

    def find_player(self, session: Any) -> Any | None:
        for attr in ("player", "client"):
            player = getattr(session, attr, None)
            if player and isinstance(player, PlexClient):
                return player

        server = self.get_server()
        session_player = getattr(session, "player", None)
        target_name = getattr(session_player, "title", None) or getattr(
            session, "device", None
        )
        target_machine_id = getattr(session_player, "machineIdentifier", None)

        try:
            for client in server.clients():
                if target_machine_id and client.machineIdentifier == target_machine_id:
                    return client
                if target_name and client.title.lower() == str(target_name).lower():
                    return client
        except (requests.RequestException, OSError) as error:
            logger.debug("Erro ao varrer lista de clients(): %s", error)

        if session_player and hasattr(session_player, "seekTo"):
            return session_player
        return None

    def seek_player(self, player: Any, offset_ms: int) -> bool:
        player_name = getattr(player, "title", "Desconhecido")
        logger.info(
            "Executando seekTo(%d ms) no player '%s'...", offset_ms, player_name
        )
        try:
            if hasattr(player, "seekTo"):
                player.seekTo(offset=offset_ms, mtype="video")
                return True
            logger.warning("Player '%s' não possui o método seekTo.", player_name)
            return False
        except (requests.RequestException, OSError, AttributeError) as error:
            logger.error(
                "Falha ao enviar comando seekTo para '%s': %s", player_name, error
            )
            return False

    def play_media(self, player: Any, media_item: Any, offset_ms: int = 0) -> bool:
        player_name = getattr(player, "title", "Desconhecido")
        media_title = getattr(media_item, "title", "Mídia")
        logger.info(
            "Enviando playMedia('%s', offset=%d ms) para '%s'...",
            media_title,
            offset_ms,
            player_name,
        )
        try:
            if hasattr(player, "playMedia"):
                player.playMedia(media_item, offset=offset_ms)
                return True
            logger.warning("Player '%s' não possui o método playMedia.", player_name)
            return False
        except (requests.RequestException, OSError, AttributeError) as error:
            logger.error("Falha ao enviar playMedia para '%s': %s", player_name, error)
            return False

    def get_next_episode(self, current_episode: Any) -> Any | None:
        if not isinstance(current_episode, Episode):
            logger.debug(
                "Mídia atual não é um episódio de série (Tipo: %s)",
                type(current_episode),
            )
            return None

        try:
            season = current_episode.season()
            episodes = season.episodes()
            current_index = next(
                (
                    index
                    for index, episode in enumerate(episodes)
                    if episode.ratingKey == current_episode.ratingKey
                ),
                None,
            )

            if current_index is not None and current_index + 1 < len(episodes):
                next_episode = episodes[current_index + 1]
                logger.info(
                    "Próximo episódio localizado na mesma temporada: S%02dE%02d - %s",
                    next_episode.seasonNumber,
                    next_episode.episodeNumber,
                    next_episode.title,
                )
                return next_episode

            show = current_episode.show()
            seasons = [item for item in show.seasons() if item.seasonNumber > 0]
            current_season_idx = next(
                (
                    index
                    for index, item in enumerate(seasons)
                    if item.seasonNumber == season.seasonNumber
                ),
                None,
            )
            if current_season_idx is not None and current_season_idx + 1 < len(seasons):
                next_season_episodes = seasons[current_season_idx + 1].episodes()
                if next_season_episodes:
                    next_episode = next_season_episodes[0]
                    logger.info(
                        "Próximo episódio localizado na temporada seguinte: S%02dE%02d - %s",
                        next_episode.seasonNumber,
                        next_episode.episodeNumber,
                        next_episode.title,
                    )
                    return next_episode
        except (requests.RequestException, OSError, AttributeError) as error:
            logger.error("Erro ao calcular próximo episódio: %s", error)
        return None

    def list_library_sections(self) -> list[dict]:
        """Return a list of library sections with title, type, thumb_url."""
        server = self.get_server()
        result = []
        for section in server.library.sections():
            thumb = getattr(section, "thumb", None)
            thumb_url = (
                f"{self.url}{thumb}?X-Plex-Token={self.token}" if thumb else None
            )
            result.append(
                {
                    "key": section.key,
                    "title": section.title,
                    "type": section.type,
                    "thumb": thumb,
                    "thumb_url": thumb_url,
                }
            )
        return result

    def list_episodes(self, section_key: str) -> list[dict]:
        """Return simplified episode list for a library section."""
        server = self.get_server()
        section = server.library.sectionByID(int(section_key))
        eps = section.searchEpisodes()
        out = []
        for ep in eps:
            file_path = None
            if ep.media:
                part = ep.media[0].parts[0]
                file_path = part.file
            thumb = (
                f"{self.url}{ep.thumb}?X-Plex-Token={self.token}" if ep.thumb else None
            )
            out.append(
                {
                    "rating_key": ep.ratingKey,
                    "title": ep.title,
                    "season": ep.seasonNumber,
                    "episode": ep.episodeNumber,
                    "file_path": file_path,
                    "thumb_url": thumb,
                }
            )
        return out

    def list_episodes_with_markers(self, section_key: str) -> list[dict]:
        """Return episodes with marker info (intro/credits) and series title."""
        server = self.get_server()
        section = server.library.sectionByID(int(section_key))
        eps = section.searchEpisodes()
        out = []
        for ep in eps:
            # marker check
            has_intro = any(m.type == "intro" for m in ep.markers)
            has_credits = any(m.type == "credits" for m in ep.markers)
            # series title
            try:
                show = ep.show()
                series_title = show.title
            except (requests.RequestException, OSError, AttributeError):
                series_title = ""
            file_path = None
            if ep.media:
                part = ep.media[0].parts[0]
                file_path = part.file
            thumb = (
                f"{self.url}{ep.thumb}?X-Plex-Token={self.token}" if ep.thumb else None
            )
            out.append(
                {
                    "rating_key": ep.ratingKey,
                    "title": ep.title,
                    "season": ep.seasonNumber,
                    "episode": ep.episodeNumber,
                    "series_title": series_title,
                    "has_intro": has_intro,
                    "has_credits": has_credits,
                    "file_path": file_path,
                    "thumb_url": thumb,
                }
            )
        return out

    def get_episode(self, rating_key: str) -> Any:
        server = self.get_server()
        # rating_key is numeric ID; fetchItem expects a full path
        return server.fetchItem(f"/library/metadata/{rating_key}")

    def add_marker(self, episode: Any, mtype: str, start_ms: int, end_ms: int) -> None:
        """Add or update a marker on the episode via the raw Plex REST API.
        This bypasses the need for a fully-loaded Episode object."""
        rating_key = getattr(episode, "ratingKey", None) or getattr(
            episode, "rating_key", None
        )
        if not rating_key:
            raise ValueError("Episode object has no ratingKey")

        # Plex endpoint: PUT /library/metadata/<ratingKey>/markers
        # Expected form fields: type (intro|credits), start (ms), end (ms)
        url_path = f"/library/metadata/{rating_key}/markers"
        payload = {
            "type": mtype,  # "intro" or "credits"
            "start": start_ms,
            "end": end_ms,
        }

        server = self.get_server()
        full_url = server.url(url_path)
        headers = server._headers()
        # Plex uses PUT to create/update markers
        resp = server._session.put(full_url, data=payload, headers=headers)
        resp.raise_for_status()

    def remove_marker(self, episode: Any, mtype: str) -> None:
        for m in list(episode.markers):
            if m.type == mtype:
                episode.removeMarker(m)
