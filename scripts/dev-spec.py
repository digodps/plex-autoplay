#!/usr/bin/env python3
#
# dev-spec.py - Script de suporte e orquestração do workflow multi-agente /dev-spec.
#
# Valida pré-requisitos (.env, gh CLI, openspec), auxilia na execução
# dos testes, geração de commits convencionais e abertura de Pull Request.
#
# Uso:
#   python scripts/dev-spec.py [--change <nome>] [--dry-run] [--skip-tests] [--skip-pr]
#
# Exemplos:
#   python scripts/dev-spec.py
#   python scripts/dev-spec.py --change multi-agent-dev-workflow
#   python scripts/dev-spec.py --dry-run
#

from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Optional

REQUIRED_ENV_VARS = ["PLEX_URL", "PLEX_TOKEN", "JWT_SECRET", "ADMIN_PASSWORD", "MEDIA_ROOT"]
CHANGES_DIR = Path("openspec/changes")


def run_cmd(cmd: list[str], check: bool = True, capture_output: bool = True) -> subprocess.CompletedProcess:
    """Executa um comando e retorna o resultado."""
    return subprocess.run(cmd, capture_output=capture_output, text=True, check=check)


def run_cmd_nocheck(cmd: list[str], capture_output: bool = True) -> subprocess.CompletedProcess:
    """Executa um comando sem disparar exceção."""
    return subprocess.run(cmd, capture_output=capture_output, text=True)


def find_gh_cli() -> Optional[str]:
    """Localiza o binário do gh CLI no PATH ou locais conhecidos."""
    cli = shutil.which("gh")
    if cli:
        return cli
    brew_gh = Path("/home/linuxbrew/.linuxbrew/bin/gh")
    if brew_gh.exists() and os.access(brew_gh, os.X_OK):
        return str(brew_gh)
    return None


def check_prerequisites() -> tuple[bool, list[str]]:
    """Verifica todos os pré-requisitos para o fluxo de desenvolvimento."""
    errors: list[str] = []

    # 1. Verifica .env
    env_file = Path(".env")
    if not env_file.exists():
        errors.append("Arquivo '.env' não encontrado na raiz do projeto.")
    else:
        content = env_file.read_text(encoding="utf-8")
        lines = [line.strip() for line in content.splitlines() if line.strip() and not line.startswith("#")]
        env_dict = {}
        for line in lines:
            if "=" in line:
                k, v = line.split("=", 1)
                env_dict[k.strip()] = v.strip()
        missing = [v for v in REQUIRED_ENV_VARS if not env_dict.get(v)]
        if missing:
            errors.append(f"Variáveis obrigatórias ausentes ou vazias no .env: {', '.join(missing)}")

    # 2. Verifica gh CLI e autenticação
