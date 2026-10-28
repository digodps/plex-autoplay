#!/usr/bin/env python3
#
# setup-spec.py - Skill de setup isolado para mudanças OpenSpec.
#
# A partir de uma mudança OpenSpec ativa, esta skill:
#   1. Identifica a mudança ativa (via --change ou openspec list --json)
#   2. Gera um span no formato dd-mm-yyyy HH:mm:ss:mmm
#   3. Cria um branch remoto: feature/<mudanca>-<span-sanitizado>
#   4. Faz push do branch para origin
#   5. Deixa o branch local pronto para uso (apontando para o remoto)
#
# O PR e a implementacao das tasks sao feitos posteriormente pelo agente/usuario.
#
# Padroes:
#   - Branch: feature/<mudanca>-<span-sanitizado>
#   - Span (exibicao/logs): dd-mm-yyyy HH:mm:ss:mmm  (ex: 27-10-2026 14:30:00:123)
#   - Span (nomes de branch/pasta): espacos e dois-pontos substituidos por '-'
#   - PR: criado manualmente pelo usuario (este script nao automatiza)
#
# Uso:
#   python scripts/setup-spec.py [--change <nome>] [--dry-run]
#
# Exemplos:
#   python scripts/setup-spec.py
#   python scripts/setup-spec.py --change implementar-cache-generico-plugavel
#   python scripts/setup-spec.py --dry-run
#

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from datetime import datetime
from pathlib import Path
from typing import Optional

CHANGES_DIR = Path("openspec/changes")
SCRIPT_DIR = Path("scripts")


def run_cmd(cmd: list[str], check: bool = True) -> subprocess.CompletedProcess:
    """Executa um comando e retorna o resultado."""
    return subprocess.run(cmd, capture_output=True, text=True, check=check)


def run_cmd_nocheck(cmd: list[str]) -> subprocess.CompletedProcess:
    """Executa um comando sem levantar excecao em caso de falha."""
    return subprocess.run(cmd, capture_output=True, text=True)


def sanitize_span(span: str) -> str:
    """Substitui espacos e dois-pontos por '-' para uso em nomes de branch e pasta."""
    return span.replace(" ", "-").replace(":", "-")


def generate_span() -> str:
    """Gera um span no formato dd-mm-yyyy HH:mm:ss:mmm."""
    now = datetime.now()
    return now.strftime("%d-%m-%Y %H:%M:%S:%f")[:-3]


def get_in_progress_changes() -> list[dict]:
    """Lista mudancas OpenSpec com status in-progress."""
    result = run_cmd(["openspec", "list", "--json"])
    data = json.loads(result.stdout)
    changes = data.get("changes", [])
    return [c for c in changes if c.get("status") == "in-progress"]


def get_change_root(change_name: str) -> Path:
    return CHANGES_DIR / change_name


def read_artifact(change_name: str, artifact_file: str) -> Optional[str]:
    """Lê um artefato de uma mudança, se existir."""
    path = get_change_root(change_name) / artifact_file
    if path.exists():
        return path.read_text(encoding="utf-8")
    return None


def list_spec_files(change_name: str) -> list[Path]:
    """Lista arquivos de specs de uma mudança, se existirem."""
    specs_dir = get_change_root(change_name) / "specs"
    if not specs_dir.exists():
        return []
    return sorted(specs_dir.rglob("*.md"))


def fetch_origin_main() -> bool:
    """Atualiza o remote origin e fetch do main limpo."""
    result = run_cmd_nocheck(["git", "fetch", "origin"])
    if result.returncode != 0:
        print(f"❌ Falha ao fazer fetch do origin: {result.stderr}", file=sys.stderr)
        return False
    return True


def create_branch_locally(branch_name: str) -> bool:
    """Cria um branch local a partir do main remoto (remoto/origin/main)."""
    result = run_cmd_nocheck(["git", "checkout", "-b", branch_name, "origin/main"])
    if result.returncode != 0:
        print(f"❌ Falha ao criar branch '{branch_name}': {result.stderr}", file=sys.stderr)
        return False
    return True


def push_branch(branch_name: str) -> bool:
    """Faz push do branch para o remote origin."""
    result = run_cmd_nocheck(["git", "push", "origin", branch_name])
    if result.returncode != 0:
        print(f"❌ Falha ao fazer push do branch '{branch_name}': {result.stderr}", file=sys.stderr)
        return False
    return True


def main() -> int:
    parser = argparse.ArgumentParser(description="Skill de setup isolado para mudancas OpenSpec")
    parser.add_argument("--change", default=None, help="Nome da mudança OpenSpec em andamento")
    parser.add_argument("--dry-run", action="store_true", help="Apenas simula, nao executa comandos git")
    args = parser.parse_args()

    if not CHANGES_DIR.exists():
        print("❌ Diretorio openspec/changes nao encontrado. Rode a partir da raiz do repositorio.", file=sys.stderr)
        return 1

    # 1. Identifica a mudança ativa
    changes = get_in_progress_changes()
    if not changes:
        print("❌ Nenhuma mudança OpenSpec em andamento encontrada.", file=sys.stderr)
        return 1

    change_name: Optional[str] = args.change
    if change_name:
        if not any(c["name"] == change_name for c in changes):
            print(f"❌ Mudança '{change_name}' não está em andamento.", file=sys.stderr)
            print(f"   Mudanças em andamento: {', '.join(c['name'] for c in changes)}")
            return 1
    elif len(changes) == 1:
        change_name = changes[0]["name"]
        print(f"ℹ️  Usando a única mudança em andamento: {change_name}")
    else:
        print("❌ Múltiplas mudanças em andamento. Informe uma com --change:")
        for c in changes:
            print(f"   - {c['name']}")
        return 1

    assert change_name is not None

    # 2. Gera span
    span = generate_span()
    span_sanitized = sanitize_span(span)
    branch_name = f"feature/{change_name}-{span_sanitized}"

    print(f"📋 Mudança: {change_name}")
    print(f"📅 Span: {span}")
    print(f"🌿 Branch: {branch_name}")

    if args.dry_run:
        print("\n🔍 Dry-run concluído. Nenhum comando git foi executado.")
        return 0

    # 2. Cria branch local a partir do main remoto limpo
    if not fetch_origin_main():
        return 1
    if not create_branch_locally(branch_name):
        return 1
    print(f"✅ Branch local '{branch_name}' criado a partir do origin/main.")

    # 3. Push do branch para origin
    if not push_branch(branch_name):
        return 1
    print(f"✅ Branch '{branch_name}' pushado para origin.")

    print(f"\n✅ Skill setup-spec concluída com sucesso.")
    print(f"   Branch remoto: {branch_name}")
    print(f"   O PR deve ser criado manualmente no GitHub.")

    return 0


if __name__ == "__main__":
    sys.exit(main())
