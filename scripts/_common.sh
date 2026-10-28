#!/usr/bin/env bash
#
# _common.sh - Helpers compartilhados pelos scripts de gerenciamento.
# Fonte: source "$(dirname "${BASH_SOURCE[0]}")/_common.sh"
#
# Ambiente suportado:
#   - Dentro do distrobox/toolbox: o CLI 'docker' existe e fala com o
#     podman do host via socket compatível (/run/host/.../podman.sock).
#   - No host (ex.: Bazzite): 'docker' não existe; os scripts reexecutam
#     a si mesmos dentro do distrobox automaticamente.

# Garante que há um CLI 'docker' utilizável. Deve ser chamado ANTES de
# qualquer uso de docker. No host sem 'docker', reexecuta o próprio script
# dentro do primeiro container distrobox disponível (ou DISTROBOX_CONTAINER).
# Uso: ensure_docker "$SCRIPT_DIR/$(basename "$0")" "$@"
ensure_docker() {
  if command -v docker >/dev/null 2>&1; then
    return 0
  fi

  local self="$1"
  shift

  if command -v distrobox >/dev/null 2>&1; then
    local box
    box="${DISTROBOX_CONTAINER:-$(distrobox list --no-color 2>/dev/null | awk -F'|' 'NR>1 { gsub(/ /, "", $2); if ($2 != "") { print $2; exit } }')}"
    if [[ -n "$box" ]]; then
      echo "ℹ️  'docker' não existe no host — reexecutando dentro do distrobox '$box'..."
      exec distrobox-enter --name "$box" -- "$self" "$@"
    fi
  fi

  echo "❌ 'docker' não encontrado e nenhum container distrobox disponível." >&2
  echo "   Opções:" >&2
  echo "   - Entre no seu distrobox e rode o script de lá;" >&2
  echo "   - Ou defina DISTROBOX_CONTAINER=<nome> para escolher o container." >&2
  exit 127
}
