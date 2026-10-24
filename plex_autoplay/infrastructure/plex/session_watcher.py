"""Session watcher combining WebSocket alerts and polling fallback."""

import logging
import threading
import time
from typing import Any, Dict, Optional

from plex_autoplay.application.playback_service import PlaybackService
from plex_autoplay.config import Settings
from plex_autoplay.infrastructure.plex.client import PlexManager

logger = logging.getLogger("plex-autoplay.watcher")


class SessionWatcher:
    """Monitors active Plex sessions and invokes the playback use case."""

    def __init__(self, plex_manager: PlexManager, playback_service: PlaybackService, config: Settings):
        self.plex_manager = plex_manager
        self.playback_service = playback_service
        self.config = config
        self._running = False
        self._poll_thread: Optional[threading.Thread] = None
        self._alert_listener: Optional[Any] = None

    def start(self) -> None:
        self._running = True
        logger.info("Iniciando monitoramento de sessões do Plex...")

        try:
            server = self.plex_manager.get_server()
            self._alert_listener = server.startAlertListener(
                callback=self._on_websocket_alert,
                callbackError=self._on_websocket_error,
            )
            logger.info("WebSocket AlertListener conectado com sucesso ao Plex.")
        except Exception as error:
            logger.warning(
                "Não foi possível iniciar WebSocket AlertListener (%s). O monitoramento continuará via polling.",
                error,
            )

        self._poll_thread = threading.Thread(target=self._poll_loop, daemon=True, name="PlexPollingThread")
        self._poll_thread.start()
        logger.info("Thread de polling de sessões iniciada (intervalo: %ds).", self.config.poll_interval_seconds)

    def stop(self) -> None:
        logger.info("Parando monitoramento de sessões...")
        self._running = False

        if self._alert_listener is not None:
            try:
                self._alert_listener.stop()
            except Exception as error:
                logger.debug("Erro ao parar alert listener: %s", error)

        if self._poll_thread and self._poll_thread.is_alive():
            self._poll_thread.join(timeout=3)
        logger.info("Monitoramento encerrado.")

    def _on_websocket_alert(self, data: Dict[str, Any]) -> None:
        if data.get("type") == "playing":
            self._check_sessions()

    def _on_websocket_error(self, error: Any) -> None:
        logger.debug("Alerta WebSocket: %s", error)

    def _poll_loop(self) -> None:
        while self._running:
            try:
                self._check_sessions()
            except Exception as error:
                logger.error("Erro no ciclo de verificação de sessões: %s", error)
            time.sleep(self.config.poll_interval_seconds)

    def _check_sessions(self) -> None:
        sessions = self.plex_manager.get_active_sessions()
        active_keys = set()

        for session in sessions:
            session_key = str(getattr(session, "sessionKey", None) or getattr(session, "ratingKey", ""))
            if session_key:
                active_keys.add(session_key)
            self.playback_service.process_session(session)

        self.playback_service.clean_stale_sessions(active_keys)
