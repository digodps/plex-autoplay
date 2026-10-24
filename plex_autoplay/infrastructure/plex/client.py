"""Plex server connection, session access, and remote player control."""

import logging
from typing import Any, List, Optional

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
        self.server: Optional[Any] = None

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

    def get_active_sessions(self) -> List[Any]:
        try:
            return self.get_server().sessions()
        except Exception as error:
            logger.warning("Erro ao consultar sessões ativas: %s", error)
            return []

    def find_player(self, session: Any) -> Optional[Any]:
        for attr in ("player", "client"):
            player = getattr(session, attr, None)
            if player and isinstance(player, PlexClient):
                return player

        server = self.get_server()
        session_player = getattr(session, "player", None)
        target_name = getattr(session_player, "title", None) or getattr(session, "device", None)
        target_machine_id = getattr(session_player, "machineIdentifier", None)

        try:
            for client in server.clients():
                if target_machine_id and client.machineIdentifier == target_machine_id:
                    return client
                if target_name and client.title.lower() == str(target_name).lower():
                    return client
        except Exception as error:
            logger.debug("Erro ao varrer lista de clients(): %s", error)

        if session_player and hasattr(session_player, "seekTo"):
            return session_player
        return None

    def seek_player(self, player: Any, offset_ms: int) -> bool:
        player_name = getattr(player, "title", "Desconhecido")
        logger.info("Executando seekTo(%d ms) no player '%s'...", offset_ms, player_name)
        try:
            if hasattr(player, "seekTo"):
                player.seekTo(offset=offset_ms, mtype="video")
                return True
            logger.warning("Player '%s' não possui o método seekTo.", player_name)
            return False
        except Exception as error:
            logger.error("Falha ao enviar comando seekTo para '%s': %s", player_name, error)
            return False

    def play_media(self, player: Any, media_item: Any, offset_ms: int = 0) -> bool:
        player_name = getattr(player, "title", "Desconhecido")
        media_title = getattr(media_item, "title", "Mídia")
        logger.info("Enviando playMedia('%s', offset=%d ms) para '%s'...", media_title, offset_ms, player_name)
        try:
            if hasattr(player, "playMedia"):
                player.playMedia(media_item, offset=offset_ms)
                return True
            logger.warning("Player '%s' não possui o método playMedia.", player_name)
            return False
        except Exception as error:
            logger.error("Falha ao enviar playMedia para '%s': %s", player_name, error)
            return False

    def get_next_episode(self, current_episode: Any) -> Optional[Any]:
        if not isinstance(current_episode, Episode):
            logger.debug("Mídia atual não é um episódio de série (Tipo: %s)", type(current_episode))
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
                (index for index, item in enumerate(seasons) if item.seasonNumber == season.seasonNumber),
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
        except Exception as error:
            logger.error("Erro ao calcular próximo episódio: %s", error)
        return None
