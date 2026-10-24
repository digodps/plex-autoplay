"""Playback use case coordinating domain rules and external gateways."""

import logging
import time
from typing import Any, Dict, Set

from plex_autoplay.application.ports import MarkerRepository, PlexGateway
from plex_autoplay.config import Settings
from plex_autoplay.domain.models import SessionState
from plex_autoplay.domain.playback_rules import should_skip_intro, should_trigger_next_episode

logger = logging.getLogger("plex-autoplay.controller")


class PlaybackService:
    """Evaluates active sessions and issues the required playback commands."""

    def __init__(
        self,
        plex_gateway: PlexGateway,
        marker_repository: MarkerRepository,
        config: Settings,
    ):
        self.plex_gateway = plex_gateway
        self.marker_repository = marker_repository
        self.config = config
        self.states: Dict[str, SessionState] = {}

    def is_session_allowed(self, session: Any) -> bool:
        if self.config.allowed_usernames:
            user_title = ""
            for attr in ("usernames", "user"):
                val = getattr(session, attr, None)
                if val:
                    if isinstance(val, list) and len(val) > 0:
                        user_title = str(val[0])
                    elif hasattr(val, "title"):
                        user_title = str(val.title)
                    else:
                        user_title = str(val)
                    break

            if user_title.lower() not in [user.lower() for user in self.config.allowed_usernames]:
                logger.debug("Sessão ignorada: Usuário '%s' não está na lista permitida.", user_title)
                return False

        if self.config.allowed_client_ids:
            player = getattr(session, "player", None)
            machine_id = getattr(player, "machineIdentifier", "") or getattr(session, "machineIdentifier", "")
            player_title = getattr(player, "title", "") or getattr(session, "device", "")

            allowed = any(
                allowed_id.lower() in (str(machine_id).lower(), str(player_title).lower())
                for allowed_id in self.config.allowed_client_ids
            )
            if not allowed:
                logger.debug("Sessão ignorada: Dispositivo '%s' (%s) não permitido.", player_title, machine_id)
                return False

        return True

    def process_session(self, session: Any) -> None:
        if not self.is_session_allowed(session):
            return

        session_key = str(getattr(session, "sessionKey", None) or getattr(session, "ratingKey", ""))
        rating_key = str(getattr(session, "ratingKey", ""))
        view_offset = getattr(session, "viewOffset", None)
        duration = getattr(session, "duration", None)

        if not session_key or view_offset is None or duration is None:
            return

        state = self.states.get(session_key)
        if state is None or state.rating_key != rating_key:
            state = SessionState(session_key=session_key, rating_key=rating_key)
            self.states[session_key] = state

        player_name = getattr(getattr(session, "player", None), "title", "Player")

        if self.config.auto_skip_intro and not state.intro_skipped:
            intro_markers = self.marker_repository.get_intro_markers(session)
            if intro_markers is not None and should_skip_intro(view_offset, intro_markers):
                intro_start, intro_end = intro_markers
                logger.info(
                    "⏩ [SKIP INTRO] Abertura detectada em '%s' (%d ms até %d ms). Posição atual: %d ms.",
                    player_name,
                    intro_start,
                    intro_end,
                    view_offset,
                )
                player = self.plex_gateway.find_player(session)
                if player:
                    if self.plex_gateway.seek_player(player, intro_end):
                        state.intro_skipped = True
                        state.last_action_timestamp = time.time()
                else:
                    logger.warning("Não foi possível encontrar controle remoto para o player '%s'.", player_name)

        if self.config.auto_next_episode and not state.next_triggered:
            credits_start = self.marker_repository.get_credits_marker(session)
            if should_trigger_next_episode(
                view_offset,
                duration,
                credits_start,
                self.config.next_episode_threshold_seconds,
            ):
                logger.info(
                    "⏭️ [AUTO NEXT] Final do episódio detectado em '%s' (Restando: %.1fs).",
                    player_name,
                    (duration - view_offset) / 1000.0,
                )
                next_episode = self.plex_gateway.get_next_episode(session)
                if next_episode:
                    player = self.plex_gateway.find_player(session)
                    if player:
                        logger.info(
                            "Iniciando próximo episódio automaticamente no '%s': S%02dE%02d - %s",
                            player_name,
                            next_episode.seasonNumber,
                            next_episode.episodeNumber,
                            next_episode.title,
                        )
                        if self.plex_gateway.play_media(player, next_episode):
                            state.next_triggered = True
                            state.last_action_timestamp = time.time()
                    else:
                        logger.warning("Não foi possível controlar o player '%s' para avançar.", player_name)

    def clean_stale_sessions(self, active_session_keys: Set[str]) -> None:
        stale_keys = [key for key in self.states if key not in active_session_keys]
        for key in stale_keys:
            del self.states[key]
