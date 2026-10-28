#!/usr/bin/env bash
#
# stop.sh - Para e remove o container Plex Autoplay
#
# Uso: ./stop.sh [--remove-volumes]
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

source "$SCRIPT_DIR/_common.sh"
ensure_docker "$SCRIPT_DIR/$(basename "$0")" "$@"

REMOVE_VOLUMES=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --remove-volumes|-v) REMOVE_VOLUMES=true; shift ;;
    --help|-h)
      echo "Uso: $0 [--remove-volumes] [--help]"
      echo ""
      echo "Opções:"
      echo "  --remove-volumes  Remove também o volume persistente (dados são perdidos)"
      echo "  --help            Exibe esta ajuda"
      exit 0
      ;;
    *)
      echo "Opção desconhecida: $1" >&2
      exit 1
      ;;
  esac
done

echo "🛑 Parando o Plex Autoplay..."

if docker compose ps --services 2>/dev/null | grep -q plex-autoplay; then
  docker compose stop
  echo "   Container parado."
else
  echo "   Nenhum container em execução encontrado."
fi

docker compose down

if [[ "$REMOVE_VOLUMES" == true ]]; then
  echo "🗑  Removendo volume persistente (plex-data)..."
  docker compose down -v
  echo "   Volume removido. Todos os dados locais (SQLite, custom.json, cache) foram apagados."
fi

echo ""
echo "✅ Plex Autoplay parado com sucesso."
