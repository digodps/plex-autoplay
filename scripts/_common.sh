#!/usr/bin/env bash
#
# _common.sh - Helpers compartilhados pelos scripts de gerenciamento.

ensure_docker() {
  if ! command -v docker &> /dev/null; then
    echo "❌ 'docker' não encontrado no PATH." >&2
    exit 1
  fi
}
