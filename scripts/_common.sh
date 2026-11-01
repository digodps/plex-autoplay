#!/usr/bin/env bash
#
# _common.sh - Helpers compartilhados pelos scripts de gerenciamento.
# Fonte: source "$(dirname "${BASH_SOURCE[0]}")/_common.sh"
<<<<<<< HEAD
#
# Garante que há um CLI 'docker' utilizável. Deve ser chamado ANTES de
# qualquer uso de docker. Se não encontrado, falha com mensagem clara.
# Uso: ensure_docker "$SCRIPT_DIR/$(basename "$0")" "$@"
=======

# Garante que o CLI 'docker' está disponível na máquina do desenvolvedor.
# Uso: ensure_docker
>>>>>>> origin/main
ensure_docker() {
  if command -v docker >/dev/null 2>&1; then
    return 0
  fi

<<<<<<< HEAD
  echo "❌ 'docker' não encontrado no host e não há container disponível para execução." >&2
  echo "   Entre no seu container ou defina DISTROBOX_CONTAINER=<nome>." >&2
=======
  echo "❌ 'docker' não encontrado no PATH." >&2
  echo "   Instale o Docker e rode o script novamente." >&2
>>>>>>> origin/main
  exit 127
}
