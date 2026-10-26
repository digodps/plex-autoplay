"""Application entry point – starts the web dashboard only."""

import logging
import sys
import threading

from plex_autoplay.config import config
from plex_autoplay.infrastructure.web.app import create_app
from plex_autoplay.infrastructure.web.server import create_dashboard_server


def setup_logging(log_level: str) -> None:
    level = getattr(logging, log_level, logging.INFO)
    logging.basicConfig(
        level=level,
        format="%(asctime)s [%(levelname)s] [%(name)s] %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S",
    )


def main(argv: list[str] | None = None) -> int:
    # No CLI arguments needed for the new Plex‑centric workflow
    setup_logging(config.log_level)
    logger = logging.getLogger("plex-autoplay")

    logger.info("=========================================")
    logger.info("  Iniciando plex-autoplay (Plex‑centric)")
    logger.info("=========================================")

    try:
        config.validate()
    except Exception as error:
        logger.error("Configuração inválida: %s", error)
        sys.exit(1)

    logger.info("Configurações:")
    logger.info(" - Servidor Plex: %s", config.plex_url)
    logger.info(" - Porta do dashboard: %d", config.dashboard_port)
    logger.info(" - HTTPS do dashboard: %s", config.dashboard_https)

    # Create Flask app with only the config directory (no FS libraries)
    dashboard_app = create_app(
        config_dir=config.config_dir,
        cookie_secure=config.dashboard_cookie_secure,
    )

    web_server = create_dashboard_server(
        dashboard_app,
        host="0.0.0.0",
        port=config.dashboard_port,
        https_enabled=config.dashboard_https,
        config_dir=config.config_dir,
        tls_hosts=config.dashboard_tls_hosts,
    )

    threading.Thread(target=web_server.run, name="plex-autoplay-dashboard", daemon=True).start()
    protocol = "HTTPS" if config.dashboard_https else "HTTP"
    logger.info("Dashboard %s iniciado na porta %d.", protocol, config.dashboard_port)

    logger.info("Serviço ativo. Pressione Ctrl+C para encerrar.")
    # Keep process alive
    try:
        while True:
            import time
            time.sleep(1)
    except KeyboardInterrupt:
        logger.info("Encerrando...")
        web_server.close()
    return 0


if __name__ == "__main__":
    sys.exit(main())