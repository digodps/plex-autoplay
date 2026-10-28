#!/usr/bin/env bash
#
# run-e2e-tests.sh - Executor de testes e2e dentro do Docker
#
# Valida .env, aguarda a aplicação ficar disponível (já sobe no mesmo
# docker-compose.test.yml), roda os testes e2e (Playwright) e captura
# artefatos de falha. Não precisa de Docker-in-Docker: a aplicação é
# gerenciada pelo compose externo.
#
set -euo pipefail

# --help
if [[ "${1:-}" == "--help" || "${1:-}" == "-h" ]]; then
  echo "Uso: $0"
  echo ""
  echo "Executor de testes e2e dentro do Docker."
  echo ""
  echo "Etapas executadas:"
  echo "  1. Valida existência e preenchimento do .env"
  echo "  2. Aguarda a aplicação ficar disponível (portas 443/8081)"
  echo "  3. Roda os testes e2e (Playwright)"
  echo "  4. Em caso de falha, captura artefatos em ./test-artifacts/"
  echo ""
  echo "Variáveis obrigatórias no .env: PLEX_URL, PLEX_TOKEN, JWT_SECRET, ADMIN_PASSWORD, MEDIA_ROOT"
  exit 0
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# When running inside the executor container, the script is copied to /workspace
# and the project volume is mounted at /workspace. So PROJECT_ROOT is /workspace,
# not the parent of the script location.
if [[ "${INSIDE_EXECUTOR_CONTAINER:-}" == "true" ]]; then
  PROJECT_ROOT="/workspace"
else
  PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
fi
cd "$PROJECT_ROOT"

ARTIFACTS_DIR="./test-artifacts"

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log() { echo -e "${GREEN}[run-e2e-tests]${NC} $*"; }
warn() { echo -e "${YELLOW}[run-e2e-tests]${NC} $*"; }
error() { echo -e "${RED}[run-e2e-tests]${NC} $*" >&2; }

# Variáveis obrigatórias
REQUIRED_VARS=(PLEX_URL PLEX_TOKEN JWT_SECRET ADMIN_PASSWORD MEDIA_ROOT)

# --- 1. Validar .env ---
if [[ ! -f .env ]]; then
  error "Arquivo .env não encontrado na raiz do projeto."
  error "Copie .env.example para .env e preencha com seus valores."
  exit 1
fi

log "Arquivo .env encontrado."

# Carrega variáveis do .env (não sobrescreve variáveis já definidas no ambiente)
set -a
source .env
set +a

# Valida variáveis obrigatórias
MISSING_VARS=()
for var in "${REQUIRED_VARS[@]}"; do
  if [[ -z "${!var:-}" ]]; then
    MISSING_VARS+=("$var")
  fi
done

if [[ ${#MISSING_VARS[@]} -gt 0 ]]; then
  error "Variáveis obrigatórias não definidas no .env:"
  for var in "${MISSING_VARS[@]}"; do
    error "  - $var"
  done
  error ""
  error "Edite o arquivo .env e tente novamente."
  exit 1
fi

log "Todas as variáveis obrigatórias estão definidas."

# --- 2. Aguardar a aplicação ficar disponível ---
# A aplicação já sobe no mesmo docker-compose.test.yml; não precisa de
# Docker-in-Docker para subí-la. Usa o nome do serviço Docker em vez de
# localhost, pois estamos dentro de um container separado.
log "Aguardando a aplicação ficar disponível nas portas 443 e 8081..."
for port in 443 8081; do
  for i in $(seq 1 30); do
    if curl -sf "http://plex-autoplay:$port" >/dev/null 2>&1 || \
       curl -skf "https://plex-autoplay:$port" >/dev/null 2>&1; then
      log "Porta $port está respondendo."
      break
    fi
    if [[ "$i" -eq 30 ]]; then
      error "Timeout aguardando a porta $port ficar disponível."
      exit 1
    fi
    sleep 2
  done
done

log "Aplicação está disponível."

# --- 3. Rodar testes e2e ---
log "Rodando testes e2e (Playwright)..."
TEST_EXIT_CODE=0
cd tests/e2e
npx playwright test || TEST_EXIT_CODE=$?
cd "$PROJECT_ROOT"

if [[ "$TEST_EXIT_CODE" -ne 0 ]]; then
  error "Testes e2e falharam com código de saída $TEST_EXIT_CODE."

  # --- 4. Capturar artefatos de falha ---
  log "Capturando artefatos de falha em $ARTIFACTS_DIR/..."
  mkdir -p "$ARTIFACTS_DIR"

  # Screenshots, traces, report, test-results da suíte Playwright
  if [[ -d tests/e2e/playwright-report ]]; then
    cp -r tests/e2e/playwright-report "$ARTIFACTS_DIR/playwright-report" 2>/dev/null || true
  fi
  if [[ -d tests/e2e/test-results ]]; then
    cp -r tests/e2e/test-results "$ARTIFACTS_DIR/test-results" 2>/dev/null || true
  fi

  warn "Artefatos de falha salvos em: $ARTIFACTS_DIR/"
  warn "Revise os arquivos para analisar a causa da falha."
else
  log "Todos os testes e2e passaram."
fi

exit "$TEST_EXIT_CODE"
