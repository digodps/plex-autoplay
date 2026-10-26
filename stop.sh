#!/usr/bin/env bash
# --------------------------------------------------------------
# stop.sh  –  stop the stack;  use  ./stop.sh -v  to also wipe volumes
# --------------------------------------------------------------
set -euo pipefail

cd "$(dirname "$0")"

REMOVE_VOLUMES=false
if [[ "${1:-}" == "-v" || "${1:-}" == "--volumes" ]]; then
  REMOVE_VOLUMES=true
fi

echo "🛑  Stopping containers…"
docker compose down

if $REMOVE_VOLUMES; then
  echo "🗑  Removing volumes (config, redis data, etc.)…"
  docker compose down -v
fi

echo "✅  Stack stopped."
