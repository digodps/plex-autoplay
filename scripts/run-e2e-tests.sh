#!/usr/bin/env bash
#
# run-e2e-tests.sh - Executor de testes e2e dentro do Docker
#
# Valida .env, aguarda a aplicação ficar disponível (já sobe no mesmo
# docker-compose.test.yml), roda os testes e2e (Playwright) com relatório
# HTML e captura artefatos de falha. Não precisa de Docker-in-Docker: a
# aplicação é gerenciada pelo compose externo.
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
  echo "  3. Roda os testes e2e (Playwright) com relatório HTML"
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

# --- 2. Aguardar a aplicacao ficar disponível ---
# Em modo rootless/Podman o DNS do compose resolve apenas
# <servico>.dns.podman. Usa esse nome para acessar a aplicacao
# a partir do container executor (rede Docker interna).
# A aplicacao ja sobe no mesmo docker-compose.test.yml; nao precisa de
# Docker-in-Docker para subí-la.
# O host pode ser sobrescrito via ambiente (ex.: no GitHub Actions o
# Docker resolve o container_name diretamente: APP_HOST=plex-autoplay-test).
APP_HOST="${APP_HOST:-plex-autoplay.dns.podman}"
log "Aguardando a aplicacao ficar disponível em ${APP_HOST}:80..."
for i in $(seq 1 30); do
  APP_PORT=443
if curl -sk -o /dev/null "https://${APP_HOST}:${APP_PORT}" >/dev/null 2>&1; then
    log "Aplicacao esta respondendo."
    break
  fi
  if [[ "$i" -eq 30 ]]; then
    error "Timeout aguardando a aplicacao ficar disponível."
    exit 1
  fi
  sleep 2
done


log "Aplicação está disponível."

# --- 3. Preparar diretórios de resultados (evitar erro EACCES) ---
# Remove diretórios criados por root em execuções anteriores e recria com
# permissão correta para o usuário atual do container.
log "Preparando diretórios de resultados do Playwright..."
rm -rf tests/e2e/test-results tests/e2e/playwright-report 2>/dev/null || true
mkdir -p tests/e2e/test-results tests/e2e/playwright-report
chmod -R u+rwX tests/e2e/test-results tests/e2e/playwright-report

# --- 4. Rodar testes e2e ---
log "Rodando testes e2e (Playwright) com relatório HTML..."
TEST_EXIT_CODE=0

cd tests/e2e

# Garante que as dependências do Playwright estão instaladas no container
if [[ ! -d node_modules ]]; then
  log "Instalando dependências do Playwright (npm install)..."
  npm install
fi

npx playwright test --reporter=html || TEST_EXIT_CODE=$?
cd "$PROJECT_ROOT"

# --- 5. Capturar artefatos (sempre, para diagnóstico) ---
log "Capturando artefatos de execução em $ARTIFACTS_DIR/..."
mkdir -p "$ARTIFACTS_DIR"
# Remove artifacts de execuções anteriores para evitar arquivos de falha obsoletos
rm -rf "$ARTIFACTS_DIR/test-results" "$ARTIFACTS_DIR/playwright-report" 2>/dev/null || true

# Relatório HTML do Playwright (sempre gerado com --reporter=html)
if [[ -d tests/e2e/playwright-report ]]; then
  cp -r tests/e2e/playwright-report "$ARTIFACTS_DIR/playwright-report" 2>/dev/null || true
  log "Relatório HTML: $ARTIFACTS_DIR/playwright-report/index.html"
fi

# Resultados detalhados (test-results)
if [[ -d tests/e2e/test-results ]]; then
  cp -r tests/e2e/test-results "$ARTIFACTS_DIR/test-results" 2>/dev/null || true
  log "Resultados: $ARTIFACTS_DIR/test-results/"
fi

# Logs de console e traces
if [[ -d tests/e2e/test-results ]]; then
  find tests/e2e/test-results -name "*.log" -o -name "*.txt" -o -name "trace.zip" 2>/dev/null | while read -r f; do
    cp "$f" "$ARTIFACTS_DIR/" 2>/dev/null || true
  done
fi

# Capturar stdout/stderr do container se disponível
if command -v docker >/dev/null 2>&1 && docker compose ps --services --filter status=running 2>/dev/null | grep -q plex-autoplay; then
  docker compose logs --tail=100 plex-autoplay > "$ARTIFACTS_DIR/app-logs.txt" 2>/dev/null || true
  log "Logs da aplicação: $ARTIFACTS_DIR/app-logs.txt"
fi

if [[ "$TEST_EXIT_CODE" -ne 0 ]]; then
  error "Testes e2e falharam com código de saída $TEST_EXIT_CODE."
  warn "Artefatos de falha salvos em: $ARTIFACTS_DIR/"
  warn "Revise o relatório HTML: $ARTIFACTS_DIR/playwright-report/index.html"
else
  log "Todos os testes e2e passaram."
  log "Relatório HTML: $ARTIFACTS_DIR/playwright-report/index.html"
fi

exit "$TEST_EXIT_CODE"