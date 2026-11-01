#!/usr/bin/env python3
#
# code-review-skill.py - Skill de revisão de código para padronizar a
# avaliação de alterações antes do commit.
#
# Executa lint por stack (.NET/C# com dotnet-format + StyleCop.Analyzers;
# JS/Vue com ESLint + Prettier), cruza achados com OWASP Top 10 (2021) e
# NIST SSDF, avalia Clean Code / SOLID, pergunta interativamente ao usuário
# quais itens deseja corrigir e gera specs de correção via OpenSpec.
#
# Uso:
#   python scripts/code-review-skill.py [--fix 1,2,3] [--dry-run]
#
# Exemplos:
#   python scripts/code-review-skill.py          # varre e lista achados
#   python scripts/code-review-skill.py --fix 1,3  # gera specs para itens 1 e 3

from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
from pathlib import Path
from typing import Optional

# Extensões suportadas por stack
SUPPORTED_EXTENSIONS = {
    "dotnet": {".cs"},
    "web": {".js", ".vue"},
}

# Referências de segurança embutidas (OWASP Top 10 2021 + NIST SSDF)
OWASP_TOP10_2021 = {
    "A01:2021": "Injection",
    "A02:2021": "Broken Authentication",
    "A03:2021": "Sensitive Data Exposure",
    "A04:2021": "XML External Entities (XXE)",
    "A05:2021": "Broken Access Control",
    "A06:2021": "Security Misconfiguration",
    "A07:2021": "Cross-Site Scripting (XSS)",
    "A08:2021": "Insecure Deserialization",
    "A09:2021": "Vulnerable and Outdated Components",
    "A10:2021": "Insufficient Logging & Monitoring",
}

NIST_SSDF_PRACTICES = {
    "PW.3": "Protect Well: identify and protect sensitive data",
    "PS.3": "Protect Software: respond to vulnerability reports",
    "PE.1": "Prepare the Organization: security requirements",
    "RV.1": "Respond to Vulnerabilities: vulnerability disclosure",
}


def run_cmd(cmd: list[str], check: bool = False) -> subprocess.CompletedProcess:
    """Executa um comando e retorna o resultado."""
    try:
        return subprocess.run(cmd, capture_output=True, text=True, check=check)
    except FileNotFoundError:
        return subprocess.CompletedProcess(cmd, 127, "", f"Command not found: {cmd[0]}")


def get_git_status_porcelain() -> list[dict[str, str]]:
    """Lê `git status --porcelain` e retorna lista de arquivos alterados."""
    result = run_cmd(["git", "status", "--porcelain"])
    files: list[dict[str, str]] = []
    for line in result.stdout.splitlines():
        if not line.strip():
            continue
        xy = line[:2].strip()
        path = line[3:].strip()
        if " -> " in path:
            path = path.split(" -> ")[-1].strip()
        # Arquivos deletados não existem mais no disco: não podem ser
        # passados a linters (derrubariam a análise inteira).
        if "D" in xy:
            continue
        files.append({"status": xy, "path": path})
    return files


def detect_stack(file_path: str) -> Optional[str]:
    """Detecta a stack com base na extensão do arquivo."""
    ext = Path(file_path).suffix.lower()
    for stack, extensions in SUPPORTED_EXTENSIONS.items():
        if ext in extensions:
            return stack
    return None


# Artefatos gerados/minificados não são código-fonte: nunca devem ser
# lintados ou avaliados por heurísticas de Clean Code / SOLID.
EXCLUDED_PATH_PREFIXES = (
    "src/PlexAutoplay.Web/wwwroot/",
    "vue/plex-frontend/dist/",
    "node_modules/",
    "test-results/",
    "playwright-report/",
    "test-artifacts/",
    "coverage/",
)


def is_excluded(file_path: str) -> bool:
    """Retorna True para artefatos de build que devem ficar fora da revisão."""
    normalized = file_path.replace("\\", "/")
    return any(normalized.startswith(prefix) or f"/{prefix}" in normalized for prefix in EXCLUDED_PATH_PREFIXES)


def filter_supported_files(files: list[dict[str, str]]) -> dict[str, list[dict[str, str]]]:
    """Agrupa arquivos alterados por stack suportada."""
    grouped: dict[str, list[dict[str, str]]] = {}
    for file_info in files:
        if is_excluded(file_info["path"]):
            continue
        stack = detect_stack(file_info["path"])
        if stack:
            grouped.setdefault(stack, []).append(file_info)
    return grouped


def tool_available(argv: list[str]) -> bool:
    """Verifica se uma ferramenta está instalada (sem executar a análise)."""
    probe = argv[:2] + ["--version"]
    result = run_cmd(probe, check=False)
    return result.returncode == 0
    sys.exit(main())