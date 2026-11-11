#!/usr/bin/env bash
#
# _common.sh - Helpers compartilhados pelos scripts de gerenciamento.
<<<<<<< HEAD

=======
# Fonte: source "$(dirname "${BASH_SOURCE[0]}")/_common.sh"

# Garante que o CLI 'docker' está disponível na máquina do desenvolvedor.
# Uso: ensure_docker
>>>>>>> origin/feature/fix-plex-episodes-endpoint-31-10-2026-19-14-11
ensure_docker() {
  if ! command -v docker &> /dev/null; then
    echo "❌ 'docker' não encontrado no PATH." >&2
    exit 1
  fi
<<<<<<< HEAD
=======

  echo "❌ 'docker' não encontrado no PATH." >&2
  echo "   Instale o Docker e rode o script novamente." >&2
  exit 127
>>>>>>> origin/feature/fix-plex-episodes-endpoint-31-10-2026-19-14-11
}
