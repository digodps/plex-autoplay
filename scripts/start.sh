#!/usr/bin/env bash
#
# start.sh - Inicia o container Plex Autoplay (.NET 10)
#
# Uso: ./start.sh [opções]
#   --build    Força rebuild da imagem antes de subir
#   --detach   Roda em segundo plano (padrão: anexado)
#   --help     Exibe esta ajuda
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$PROJECT_ROOT"

BUILD=false
DETACH=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --build) BUILD=true; shift ;;
    --detach|-d) DETACH=true; shift ;;
    --help|-h)
      echo "Uso: $0 [--build] [--detach] [--help]"
      echo ""
      echo "Opções:"
      echo "  --build    Força rebuild da imagem Docker antes de iniciar"
      echo "  --detach   Roda o container em segundo plano"
      echo "  --help     Exibe esta ajuda"
      exit 0
      ;;
    *)
      echo "Opção desconhecida: $1" >&2
      exit 1
      ;;
  esac
done

# Verifica se o .env existe
if [[ ! -f .env ]]; then
  echo "⚠️  Arquivo .env não encontrado."
  echo "   Copie .env.example para .env e preencha com seus valores."
  echo "   Exemplo: cp .env.example .env"
  exit 1
fi

# Carrega variáveis do .env
set -a
source .env
set +a

# Valida variáveis obrigatórias
MISSING_VARS=()
for var in PLEX_URL PLEX_TOKEN MEDIA_ROOT JWT_SECRET ADMIN_PASSWORD; do
  if [[ -z "${!var:-}" ]]; then
    MISSING_VARS+=("$var")
  fi
done

if [[ ${#MISSING_VARS[@]} -gt 0 ]]; then
  echo "❌ Variáveis obrigatórias não definidas no .env:"
  for var in "${MISSING_VARS[@]}"; do
    echo "   - $var"
  done
  echo ""
  echo "   Edite o arquivo .env e tente novamente."
  exit 1
fi

echo "=============================================="
echo "  Plex Autoplay (.NET 10) - Iniciando"
echo "=============================================="
echo "PLEX_URL:     $PLEX_URL"
echo "MEDIA_ROOT:   $MEDIA_ROOT"
echo "WEB_PORT:     ${WEB_PORT:-8080}"
echo "WS_PORT:      ${WS_PORT:-8081}"
echo ""

if [[ "$BUILD" == true ]]; then
  echo "🔨 Fazendo build da imagem Docker..."
  docker compose build
fi

if [[ "$DETACH" == true ]]; then
  echo "🚀 Subindo container em segundo plano..."
  docker compose up -d
  echo ""
  echo "✅ Container iniciado!"
  echo "   Dashboard:  https://localhost:8443"
  echo "   WebSocket:  wss://localhost:8081"
  echo ""
  echo "   Para ver os logs:   ./scripts/logs.sh"
  echo "   Para parar:         ./scripts/stop.sh"
else
  echo "🚀 Subindo container (modo anexado - pressione Ctrl+C para parar)..."
  docker compose up
fi
