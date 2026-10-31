#!/usr/bin/env bash
#
# _common.sh - Helpers compartilhados pelos scripts de gerenciamento.
# Fonte: source "$(dirname "${BASH_SOURCE[0]}")/_common.sh"
#
# Garante que há um CLI 'docker' utilizável. Deve ser chamado ANTES de
# qualquer uso de docker. Se não encontrado, falha com mensagem clara.
# Uso: ensure_docker "$SCRIPT_DIR/$(basename "$0")" "$@"
ensure_docker() {
  if command -v docker >/dev/null 2>&1; then
    return 0
  fi

  echo "❌ 'docker' não encontrado no host e não há container disponível para execução." >&2
  echo "   Entre no seu container ou defina DISTROBOX_CONTAINER=<nome>." >&2
  exit 127
}
