#!/usr/bin/env bash
#
# _common.sh - Helpers compartilhados pelos scripts de gerenciamento.
# Fonte: source "$(dirname "${BASH_SOURCE[0]}")/_common.sh"
  echo "❌ 'docker' não encontrado no PATH." >&2
  echo "   Instale o Docker e rode o script novamente." >&2
