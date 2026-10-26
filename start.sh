#!/usr/bin/env bash
# --------------------------------------------------------------
# start.sh  –  lint + build + up  (plex‑autoplay + embedded Redis)
# --------------------------------------------------------------
set -euo pipefail

cd "$(dirname "$0")"          # run from the repo root

# Carrega variáveis do .env para o shell atual (garante que docker compose as receba)
if [[ -f .env ]]; then
  set -a
  # shellcheck disable=SC1091
  source .env
  set +a
fi

echo "🔍  Running lint (ruff)…"
docker compose run --rm plex-autoplay ruff check .

echo "🔧  Building image (only first run or after code changes)…"
docker compose build

echo "🚀  Starting containers (detached)…"
# --env-file é redundante (compose já lê .env), mas torna explícito
docker compose --env-file .env up -d

echo ""
echo "✅  Stack is up.  You can now open the dashboard:"
echo "   • HTTP  : http://localhost:8080"
echo "   • HTTPS : https://localhost:8080   (self‑signed cert – accept the warning)"
echo ""
echo "🪵  Follow logs with:   ./logs.sh   (or  docker compose logs -f)"