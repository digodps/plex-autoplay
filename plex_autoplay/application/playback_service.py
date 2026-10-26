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
        now = time.time()
        delay = self.config.action_delay_seconds

        # Check and execute pending delayed actions
        self._process_pending_actions(session, state, player_name, now)

        # Skip intro logic
        if self.config.auto_skip_intro and not state.intro_skipped and not state.pending_skip_intro:
            intro_markers = self.marker_repository.get_intro_markers(session)
            if intro_markers is not None and should_skip_intro(view_offset, intro_markers):
                intro_start, intro_end = intro_markers
                if delay > 0:
                    state.pending_skip_intro = True
                    state.pending_skip_intro_at = now + delay
                    logger.info(
                        "⏩ [SKIP INTRO] Abertura detectada em '%s' (%d ms até %d ms). "
                        "Pulará em %ds se posição não mudar. Posição atual: %d ms.",
                        player_name,
                        intro_start,
                        intro_end,
                        delay,
                        view_offset,
                    )
                else:
                    self._execute_skip_intro(session, state, player_name, intro_end)

        # Auto next episode logic
        if self.config.auto_next_episode and not state.next_triggered and not state.pending_next_episode:
            credits_start = self.marker_repository.get_credits_marker(session)
            if should_trigger_next_episode(
                view_offset,
                duration,
                credits_start,
                self.config.next_episode_threshold_seconds,
            ):
                if delay > 0:
                    state.pending_next_episode = True
                    state.pending_next_episode_at = now + delay
                    logger.info(
                        "⏭️ [AUTO NEXT] Final do episódio detectado em '%s' (Restando: %.1fs). "
                        "Avançará em %ds se posição não mudar.",
                        player_name,
                        (duration - view_offset) / 1000.0,
                        delay,
                    )
                else:
                    self._execute_next_episode(session, state, player_name)

    def _process_pending_actions(self, session: Any, state: SessionState, player_name: str, now: float) -> None:
        """Check and execute any pending delayed actions whose time has come."""
        # Pending skip intro
        if state.pending_skip_intro and now >= state.pending_skip_intro_at:
            state.pending_skip_intro = False
            # Re-verify condition before executing
            intro_markers = self.marker_repository.get_intro_markers(session)
            view_offset = getattr(session, "viewOffset", None)
            if intro_markers is not None and view_offset is not None and should_skip_intro(view_offset, intro_markers):
                self._execute_skip_intro(session, state, player_name, intro_markers[1])
            else:
                logger.info("⏩ [SKIP INTRO] Cancelado em '%s': condição não mais válida.", player_name)

        # Pending next episode
        if state.pending_next_episode and now >= state.pending_next_episode_at:
            state.pending_next_episode = False
            # Re-verify condition before executing
            view_offset = getattr(session, "viewOffset", None)
            duration = getattr(session, "duration", None)
            credits_start = self.marker_repository.get_credits_marker(session)
            if (
                view_offset is not None
                and duration is not None
                and should_trigger_next_episode(
                    view_offset,
                    duration,
                    credits_start,
                    self.config.next_episode_threshold_seconds,
                )
            ):
                self._execute_next_episode(session, state, player_name)
            else:
                logger.info("⏭️ [AUTO NEXT] Cancelado em '%s': condição não mais válida.", player_name)

    def _execute_skip_intro(self, session: Any, state: SessionState, player_name: str, intro_end: int) -> None:
        player = self.plex_gateway.find_player(session)
        if player:
            if self.plex_gateway.seek_player(player, intro_end):
                state.intro_skipped = True
                state.last_action_timestamp = time.time()
                logger.info("⏩ [SKIP INTRO] Executado em '%s' para %d ms.", player_name, intro_end)
            else:
                logger.warning("Falha ao executar seek no player '%s'.", player_name)
        else:
            logger.warning("Não foi possível encontrar controle remoto para o player '%s'.", player_name)

    def _execute_next_episode(self, session: Any, state: SessionState, player_name: str) -> None:
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
                    logger.info("⏭️ [AUTO NEXT] Executado em '%s'.", player_name)
                else:
                    logger.warning("Falha ao iniciar próximo episódio no player '%s'.", player_name)
            else:
                logger.warning("Não foi possível controlar o player '%s' para avançar.", player_name)

    def clean_stale_sessions(self, active_session_keys: Set[str]) -> None:
        stale_keys = [key for key in self.states if key not in active_session_keys]
        for key in stale_keys:
            del self.states[key]