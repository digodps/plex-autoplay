#!/usr/bin/env bash
#
# logs.sh - Monitora os logs do container Plex Autoplay
#
# Uso: ./logs.sh [opções]
#   --follow|-f   Acompanha os logs em tempo real (padrão)
#   --tail N      Mostra as últimas N linhas (padrão: 100)
#   --since       Mostra logs desde um tempo relativo (ex: 1h, 30m)
#   --help        Exibe esta ajuda
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

source "$SCRIPT_DIR/_common.sh"
ensure_docker "$SCRIPT_DIR/$(basename "$0")" "$@"

FOLLOW=true
TAIL=100
SINCE=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --follow|-f) FOLLOW=true; shift ;;
    --no-follow) FOLLOW=false; shift ;;
    --tail) TAIL="${2:-100}"; shift 2 ;;
    --since) SINCE="--since=$2"; shift 2 ;;
    --help|-h)
      echo "Uso: $0 [--follow|--no-follow] [--tail N] [--since TEMPO] [--help]"
      echo ""
      echo "Opções:"
      echo "  --follow     Acompanha logs em tempo real (padrão)"
      echo "  --no-follow  Mostra apenas as últimas linhas e sai"
      echo "  --tail N     Últimas N linhas (padrão: 100)"
      echo "  --since      Logs desde um tempo (ex: 1h, 30m, 2026-01-01T00:00:00)"
      echo "  --help       Exibe esta ajuda"
      exit 0
      ;;
    *)
      echo "Opção desconhecida: $1" >&2
      exit 1
      ;;
  esac
done

echo "📋 Logs do Plex Autoplay"
echo "=============================================="

ARGS=()
if [[ "$FOLLOW" == true ]]; then
  ARGS+=("--follow")
fi
ARGS+=("--tail=$TAIL")
if [[ -n "$SINCE" ]]; then
  ARGS+=("$SINCE")
fi

docker compose logs "${ARGS[@]}"
