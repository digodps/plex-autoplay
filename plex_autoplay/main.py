"""Application composition root and service entry point."""

import argparse
import json
import logging
import signal
import sys
import threading
import time
from pathlib import Path
from typing import Callable, Optional

from plex_autoplay.application.playback_service import PlaybackService
from plex_autoplay.application.dashboard_scan_service import DashboardScanService
from plex_autoplay.application.manual_marker_service import ManualMarkerService
from plex_autoplay.config import config
from plex_autoplay.infrastructure.markers.json_repository import JsonMarkerRepository
from plex_autoplay.infrastructure.media.directory_monitor import (
    build_preview_for_root,
    merge_markers_file,
)
from plex_autoplay.infrastructure.media.library_repository import LibraryRepository
from plex_autoplay.infrastructure.media.scan_report_repository import ScanReportRepository
from plex_autoplay.application.scan_coordinator import ScanCoordinator
from plex_autoplay.infrastructure.plex.client import PlexManager
from plex_autoplay.infrastructure.plex.session_watcher import SessionWatcher
from plex_autoplay.infrastructure.web.app import create_app
from plex_autoplay.infrastructure.web.server import create_dashboard_server


def setup_logging(log_level: str) -> None:
    level = getattr(logging, log_level, logging.INFO)
    logging.basicConfig(
        level=level,
        format="%(asctime)s [%(levelname)s] [%(name)s] %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S",
    )


def _run_startup_anime_marker_generation(logger: logging.Logger) -> None:
    if not config.auto_generate_anime_markers:
        return

    for series_dir in config.series_media_dirs:
        series_root = Path(series_dir).expanduser()
        if not series_root.is_dir():
            logger.warning(
                "Diretório de séries comuns não montado ou inválido (%s); nenhuma consulta de timestamps será feita para ele.",
                series_root,
            )
        else:
            logger.info("Biblioteca de séries comuns montada em modo somente leitura: %s", series_root)

    marker_data: dict[str, dict[str, dict[str, dict[str, float]]]] = {}
    pending: list[dict[str, object]] = []
    assumptions: list[dict[str, object]] = []
    valid_roots = 0

    for media_dir in config.anime_media_dirs:
        anime_root = Path(media_dir).expanduser()
        if not anime_root.is_dir():
            logger.warning(
                "Geração automática ignorada: diretório de animes não montado ou inválido (%s).",
                anime_root,
            )
            continue

        valid_roots += 1
        try:
            preview = build_preview_for_root(
                anime_root,
                cache_file=config.anime_markers_cache_file,
                cache_ttl_seconds=config.anime_markers_cache_ttl_hours * 60 * 60,
                auto_guess=True,
            )
        except (OSError, RuntimeError, ValueError) as error:
            logger.error("Falha ao varrer a biblioteca de Anime %s: %s", anime_root, error)
            continue

        for title, seasons in preview.get("series", {}).items():
            target_seasons = marker_data.setdefault(title, {})
            for season, episodes in seasons.items():
                target_episodes = target_seasons.setdefault(season, {})
                for episode, markers in episodes.items():
                    if episode in target_episodes and target_episodes[episode] != markers:
                        logger.warning(
                            "Marcadores conflitantes para '%s', %s, episódio %s; mantendo o primeiro resultado.",
                            title,
                            season,
                            episode,
                        )
                        continue
                    target_episodes[episode] = markers
        pending.extend(preview.get("pending", []))
        assumptions.extend(preview.get("assumptions", []))

    if not valid_roots:
        logger.warning("Nenhum diretório de Anime configurado está montado; varredura automática não executada.")
        return

    try:
        if marker_data:
            merge_markers_file(config.custom_markers_file, marker_data)
        generated_episodes = sum(
            len(episodes)
            for seasons in marker_data.values()
            for episodes in seasons.values()
            if isinstance(episodes, dict)
        )
        logger.info(
            "Varredura automática de %d biblioteca(s) de Anime concluída: %d episódio(s) com marcadores, %d pendência(s), %d estimativa(s).",
            valid_roots,
            generated_episodes,
            len(pending),
            len(assumptions),
        )
        for assumption in assumptions:
            logger.warning(
                "Mapeamento estimado para '%s' (temporada %s): %s",
                assumption.get("title", "desconhecido"),
                assumption.get("season", "n/a"),
                assumption.get("reason", "sem detalhe"),
            )
        for item in pending:
            logger.warning(
                "Item de anime pendente '%s' (temporada %s, episódio %s): %s",
                item.get("title", "biblioteca"),
                item.get("season", "n/a"),
                item.get("episode", "n/a"),
                item.get("reason", "sem detalhe"),
            )
    except (OSError, RuntimeError, ValueError) as error:
        logger.error("Falha na geração automática de marcadores; o serviço Plex continuará: %s", error)


def _run_markers_command(args: argparse.Namespace) -> int:
    media_root = Path(args.media_root).expanduser().resolve()
    if not media_root.exists() or not media_root.is_dir():
        raise ValueError(f"Diretório de mídia inválido: {args.media_root}")

    output_path = (
        Path(args.output).expanduser().resolve()
        if args.output
        else Path(config.custom_markers_file).expanduser().resolve()
    )
    cache_file = (
        Path(args.cache_file).expanduser().resolve()
        if args.cache_file
        else Path(".anime-markers-cache.json").resolve()
    )
    chooser: Optional[Callable[[str], str]] = input if sys.stdin.isatty() else None
    if args.dry_run:
        preview = build_preview_for_root(
            media_root,
            cache_file=cache_file,
            cache_ttl_seconds=args.cache_ttl_hours * 60 * 60,
            chooser=chooser,
            auto_guess=args.auto_guess,
        )
        print(json.dumps(preview, ensure_ascii=False, indent=2))
        return 0

    preview = build_preview_for_root(
        media_root,
        cache_file=cache_file,
        cache_ttl_seconds=args.cache_ttl_hours * 60 * 60,
        chooser=chooser,
        auto_guess=args.auto_guess,
    )
    print(json.dumps(preview, ensure_ascii=False, indent=2))
    marker_data = preview.get("series", {})
    if not marker_data:
        print("Nenhum marcador válido para gravar; arquivo de saída preservado.", file=sys.stderr)
        return 1

    if args.yes:
        confirmed = True
    elif not sys.stdin.isatty():
        raise RuntimeError("Confirmação necessária em execução não interativa; use --yes para confirmar.")
    else:
        action = "substituir marcadores existentes e mesclar os novos" if args.force else "mesclar sem substituir marcadores existentes"
        confirmed = input(f"Confirmar gravação em {output_path} para {action}? [s/N] ").strip().casefold() in {
            "s",
            "sim",
            "y",
            "yes",
        }
    if not confirmed:
        print("Gravação cancelada; arquivo de saída preservado.")
        return 0

    merge_markers_file(output_path, marker_data, force=args.force)

    if not args.quiet:
        print(f"Marcadores gerados para {media_root} -> {output_path}")
    return 0


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="plex-autoplay")
    parser.add_argument("--media-root", help="Diretório raiz contendo series/Temporada n/arquivos")
    parser.add_argument("--output", help="Arquivo JSON de saída para marcadores automáticos")
    parser.add_argument("--cache-file", help="Arquivo JSON para cache de busca e metadados")
    parser.add_argument(
        "--cache-ttl-hours",
        type=int,
        default=168,
        help="Validade do cache de respostas de API em horas (padrão: 168)",
    )
    parser.add_argument("--dry-run", action="store_true", help="Mostra a prévia sem gravar o arquivo final")
    parser.add_argument(
        "--force",
        action="store_true",
        help="Substitui campos de marcadores existentes (ainda exige confirmação)",
    )
    parser.add_argument(
        "--auto-guess",
        action="store_true",
        help="Escolhe candidatos e offsets por estimativa quando a identificação for ambígua",
    )
    parser.add_argument(
        "--yes",
        action="store_true",
        help="Confirma a gravação explicitamente, útil em execuções automatizadas",
    )
    parser.add_argument("--quiet", action="store_true", help="Reduz a saída para uso não interativo")
    args = parser.parse_args(argv)

    if args.cache_ttl_hours < 0:
        parser.error("--cache-ttl-hours não pode ser negativo")
    if args.media_root:
        try:
            return _run_markers_command(args)
        except (EOFError, OSError, RuntimeError, ValueError) as error:
            print(f"Erro ao gerar marcadores: {error}", file=sys.stderr)
            return 1
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
    logger.info(" - Geração de marcadores de anime no start: %s", config.auto_generate_anime_markers)
    if config.allowed_usernames:
        logger.info(" - Usuários Permitidos: %s", ", ".join(config.allowed_usernames))
    if config.allowed_client_ids:
        logger.info(" - Dispositivos Permitidos: %s", ", ".join(config.allowed_client_ids))

    plex_manager = PlexManager(url=config.plex_url, token=config.plex_token)
    marker_repository = JsonMarkerRepository(
        markers_file_path=config.custom_markers_file,
        default_intro_skip_seconds=config.default_intro_skip_seconds,
    )
    web_server = None
    scan_coordinator = None
    media_root = Path(config.media_root).expanduser()
    if media_root.is_dir():
        try:
            library_repository = LibraryRepository(
                media_root,
                Path(config.config_dir) / "libraries.json",
            )
            report_repository = ScanReportRepository(Path(config.config_dir) / "scan-report.json")
            scan_service = DashboardScanService(
                libraries=library_repository,
                markers=marker_repository,
                markers_file=config.custom_markers_file,
                cache_file=config.anime_markers_cache_file,
                cache_ttl_seconds=config.anime_markers_cache_ttl_hours * 60 * 60,
                auto_generate_anime_markers=config.auto_generate_anime_markers,
                plex_episode_lookup=plex_manager.list_show_episodes,
            )
            scan_coordinator = ScanCoordinator(
                lambda progress_callback: scan_service.scan(progress_callback),
                report_repository,
            )
            dashboard_app = create_app(
                library_repository,
                config.config_dir,
                cookie_secure=config.dashboard_cookie_secure,
                scan_coordinator=scan_coordinator,
                manual_marker_service=ManualMarkerService(config.custom_markers_file),
                marker_reload=marker_repository.load_custom_markers,
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
        except (OSError, RuntimeError, ValueError) as error:
            logger.exception("Dashboard não pôde ser iniciado; o watcher Plex continuará: %s", error)
    else:
        logger.error(
            "Dashboard não iniciado: raiz de mídia %s ausente ou inválida. Monte MEDIA_ROOT e configure as bibliotecas.",
            media_root,
        )
        if config.auto_generate_anime_markers:
            threading.Thread(
                target=_run_startup_anime_marker_generation,
                args=(logger,),
                name="plex-autoplay-legacy-anime-scan",
                daemon=True,
            ).start()

    playback_service = PlaybackService(plex_manager, marker_repository, config)
    watcher = SessionWatcher(plex_manager, playback_service, config)

    def shutdown_handler(signum: int, _frame: object) -> None:
        logger.info("Sinal recebido (%s). Finalizando serviço graciosamente...", signum)
        watcher.stop()
        if web_server is not None:
            web_server.close()
        sys.exit(0)

    signal.signal(signal.SIGINT, shutdown_handler)
    signal.signal(signal.SIGTERM, shutdown_handler)

    try:
        plex_manager.connect()
    except Exception as error:
        logger.error("Não foi possível conectar ao servidor Plex: %s", error)
        logger.error("Verifique se o PLEX_URL e o PLEX_TOKEN estão corretos.")
        sys.exit(1)

    if scan_coordinator is not None and not scan_coordinator.start():
        logger.warning("A varredura anterior já está ativa; não foi criada outra execução de startup.")

    watcher.start()
    logger.info("Serviço ativo e monitorando reproduções. Pressione Ctrl+C para encerrar.")

    while True:
        time.sleep(1)
