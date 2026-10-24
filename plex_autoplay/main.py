"""Application composition root and service entry point."""

import logging
import signal
import sys
import time

from plex_autoplay.application.playback_service import PlaybackService
from plex_autoplay.config import config
from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.plex.client import PlexManager
from plex_autoplay.infrastructure.plex.session_watcher import SessionWatcher


def setup_logging(log_level: str) -> None:
    level = getattr(logging, log_level, logging.INFO)
    logging.basicConfig(
        level=level,
        format="%(asctime)s [%(levelname)s] [%(name)s] %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S",
    )


def main() -> None:
    setup_logging(config.log_level)
    logger = logging.getLogger("plex-autoplay")

    logger.info("=========================================")
    logger.info("  Iniciando plex-autoplay (100%% Free)")
    logger.info("=========================================")

    try:
        config.validate()
    except Exception as error:
        logger.error("Configuração inválida: %s", error)
        logger.error("Por favor, revise seu arquivo .env ou variáveis de ambiente.")
        sys.exit(1)

    logger.info("Configurações:")
    logger.info(" - Servidor Plex: %s", config.plex_url)
    logger.info(" - Auto Skip Intro: %s", config.auto_skip_intro)
    logger.info(" - Auto Next Episode: %s", config.auto_next_episode)
    logger.info(" - Threshold Próximo Episódio: %ds", config.next_episode_threshold_seconds)
    logger.info(" - Intervalo de Polling: %ds", config.poll_interval_seconds)
    if config.allowed_usernames:
        logger.info(" - Usuários Permitidos: %s", ", ".join(config.allowed_usernames))
    if config.allowed_client_ids:
        logger.info(" - Dispositivos Permitidos: %s", ", ".join(config.allowed_client_ids))

    plex_manager = PlexManager(url=config.plex_url, token=config.plex_token)
    marker_repository = JsonMarkerRepository(
        markers_file_path=config.custom_markers_file,
        default_intro_skip_seconds=config.default_intro_skip_seconds,
    )
    playback_service = PlaybackService(plex_manager, marker_repository, config)
    watcher = SessionWatcher(plex_manager, playback_service, config)

    def shutdown_handler(signum: int, _frame: object) -> None:
        logger.info("Sinal recebido (%s). Finalizando serviço graciosamente...", signum)
        watcher.stop()
        sys.exit(0)

    signal.signal(signal.SIGINT, shutdown_handler)
    signal.signal(signal.SIGTERM, shutdown_handler)

    try:
        plex_manager.connect()
    except Exception as error:
        logger.error("Não foi possível conectar ao servidor Plex: %s", error)
        logger.error("Verifique se o PLEX_URL e o PLEX_TOKEN estão corretos.")
        sys.exit(1)

    watcher.start()
    logger.info("Serviço ativo e monitorando reproduções. Pressione Ctrl+C para encerrar.")

    while True:
        time.sleep(1)
