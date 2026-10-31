#!/usr/bin/env bash
#
# start.sh - Inicia o container Plex Autoplay (.NET 10)
#
# Sobe o container, aguarda a aplicação responder, exibe o link do
# login e sai. NÃO exibe os logs - para isso use ./scripts/logs.sh.
#
# Uso: ./start.sh [opções]
#   --build    Força rebuild da imagem antes de subir
#   --attach   Modo antigo: acompanha os logs do container (Ctrl+C para)
#   --help     Exibe esta ajuda
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$PROJECT_ROOT"

source "$SCRIPT_DIR/_common.sh"
ensure_docker

BUILD=false
ATTACH=false
LOGIN_URL="https://localhost:8443/login"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --build) BUILD=true; shift ;;
    --attach) ATTACH=true; shift ;;
    --detach|-d) shift ;;  # aceito por compatibilidade; padrão já é detached
    --help|-h)
      echo "Uso: $0 [--build] [--attach] [--help]"
      echo ""
      echo "Opções:"
      echo "  --build    Força rebuild da imagem Docker antes de iniciar"
      echo "  --attach   Acompanha os logs do container (não sai; use Ctrl+C para parar)"
      echo "  --help     Exibe esta ajuda"
      echo ""
      echo "Logs em separado: ./scripts/logs.sh"
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

if [[ "$ATTACH" == true ]]; then
  echo "🚀 Subindo container (anexado - pressione Ctrl+C para parar)..."
  docker compose up
  exit 0
fi

# --- Modo padrão: sobe, espera responder, mostra o link e sai ---
echo "🚀 Subindo container em segundo plano..."
docker compose up -d
echo ""

echo -n "⏳ Aguardando a aplicação responder"
for _ in $(seq 1 60); do
  if curl -sk -o /dev/null "$LOGIN_URL" 2>/dev/null; then
    echo ""
    echo ""
    echo "────────────────────────────────────────────────"
    echo "✅ Aplicação no ar!"
    echo ""
    echo "   👉 Login:  $LOGIN_URL"
    echo ""
    echo "   (aceite o certificado auto-assinado no navegador)"
    echo "────────────────────────────────────────────────"
    echo ""
    echo "   Para ver os logs:   ./scripts/logs.sh"
    echo "   Para parar:         ./scripts/stop.sh"
    exit 0
  fi
  echo -n "."
  sleep 2
done

echo ""
echo ""
echo "❌ Timeout: a aplicação não respondeu em 120s."
echo "   Verifique os logs:   ./scripts/logs.sh --no-follow --tail 50"
exit 1
