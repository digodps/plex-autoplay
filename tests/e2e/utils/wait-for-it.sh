#!/usr/bin/env bash
#
# wait-for-it.sh - Aguarda um serviço ficar disponível antes de continuar
#
# Uso: ./wait-for-it.sh host:port [-- timeout em segundos]
#
set -euo pipefail

HOST_PORT="${1:-localhost:8080}"
TIMEOUT="${2:-60}"

HOST="${HOST_PORT%:*}"
PORT="${HOST_PORT#*:}"

echo "⏳ Aguardando ${HOST}:${PORT} ficar disponível (timeout: ${TIMEOUT}s)..."

START=$(date +%s)
while true; do
  if (echo > /dev/tcp/${HOST}/${PORT}) >/dev/null 2>&1; then
    echo "✅ ${HOST}:${PORT} está disponível!"
    exit 0
  fi

  NOW=$(date +%s)
  ELAPSED=$((NOW - START))
  if [[ $ELAPSED -ge $TIMEOUT ]]; then
    echo "❌ Timeout após ${TIMEOUT}s aguardando ${HOST}:${PORT}"
    exit 1
  fi

  sleep 2
done
