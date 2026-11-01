#!/usr/bin/env bash
#
# _common.sh - Helpers compartilhados pelos scripts de gerenciamento.
# Fonte: source "$(dirname "${BASH_SOURCE[0]}")/_common.sh"

# Garante que o CLI 'docker' está disponível na máquina do desenvolvedor.
# Uso: ensure_docker
ensure_docker() {
  if command -v docker >/dev/null 2>&1; then
    return 0
  fi

  echo "❌ 'docker' não encontrado no PATH." >&2
  echo "   Instale o Docker e rode o script novamente." >&2
  exit 127
}
