#!/usr/bin/env python3
#
# setup-spec.py - Skill de setup isolado para mudanças OpenSpec.
#
# A partir de uma mudança OpenSpec ativa, esta skill:
#   1. Identifica a mudança ativa (via --change ou openspec list --json)
#   2. Gera um span no formato dd-mm-yyyy HH:mm:ss:mmm
#   3. Cria um branch remoto: feature/<mudanca>-<span-sanitizado>
#   4. Faz push do branch para origin
#   5. Cria uma pasta irmã: plex-setup-<mudanca>-<span-sanitizado>/
#   6. Executa git clone -b <branch> <origin> <pasta-irma>
#   7. Copia os artefatos da mudança (proposal, design, tasks, specs) para o clone
#   8. Commita e pusha os artefatos no branch remoto, deixando-o pronto para trabalho
#   9. Remove os artefatos da change do repositório original (já estão no outro branch)
#
# O PR e a implementacao das tasks sao feitos posteriormente pelo agente/usuario.
#
# Padroes:
#   - Branch: feature/<mudanca>-<span-sanitizado>
#   - Span (exibicao/logs): dd-mm-yyyy HH:mm:ss:mmm  (ex: 27-10-2026 14:30:00:123)
#   - Span (nomes de branch/pasta): espacos e dois-pontos substituidos por '-'
#   - Pasta: plex-setup-<mudanca>-<span-sanitizado>/
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
import shutil
import subprocess
import sys
from datetime import datetime
from pathlib import Path
from typing import Optional

CHANGES_DIR = Path("openspec/changes")
SCRIPT_DIR = Path("scripts")
PARENT_DIR = Path(".").resolve().parent


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


def clone_branch(branch_name: str, origin_url: str, target_dir: Path) -> bool:
    """Clona o branch em uma pasta alvo."""
    result = run_cmd_nocheck(["git", "clone", "-b", branch_name, origin_url, str(target_dir)])
    if result.returncode != 0:
        print(f"❌ Falha ao clonar branch '{branch_name}' para '{target_dir}': {result.stderr}", file=sys.stderr)
        return False
    return True


def get_origin_url() -> Optional[str]:
    """Retorna a URL do remote origin."""
    result = run_cmd_nocheck(["git", "remote", "get-url", "origin"])
    if result.returncode != 0:
        return None
    return result.stdout.strip()


def copy_artifacts_to_clone(change_name: str, clone_dir: Path) -> list[str]:
    """Copia os artefatos da mudança para o clone. Retorna lista de arquivos copiados."""
    copied: list[str] = []
    change_root = get_change_root(change_name)

    # Copia proposal.md, design.md, tasks.md
    for artifact in ["proposal.md", "design.md", "tasks.md"]:
        src = change_root / artifact
        if src.exists():
            dst = clone_dir / artifact
            shutil.copy2(src, dst)
            copied.append(artifact)

    # Copia specs/ (se existir)
    specs_src = change_root / "specs"
    if specs_src.exists():
        specs_dst = clone_dir / "specs"
        if specs_dst.exists():
            shutil.rmtree(specs_dst)
        shutil.copytree(specs_src, specs_dst)
        copied.append("specs/")

    return copied


def remove_artifacts_from_original(change_name: str) -> list[str]:
    """Remove os artefatos da mudança do repositório original (working tree).

    Os artefatos ja foram copiados para o branch isolado, entao nao
    precisam mais permanecer no repositorio original.
    """
    removed: list[str] = []
    change_root = get_change_root(change_name)

    for artifact in ["proposal.md", "design.md", "tasks.md", ".openspec.yaml"]:
        path = change_root / artifact
        if path.exists():
            if path.is_dir():
                shutil.rmtree(path)
            else:
                path.unlink()
            removed.append(str(path))

    specs_src = change_root / "specs"
    if specs_src.exists():
        shutil.rmtree(specs_src)
        removed.append(str(specs_src))

    return removed


def commit_and_push_artifacts(clone_dir: Path, branch_name: str) -> bool:
    """Comita e push os artefatos copiados no clone."""
    # git add
    result = run_cmd_nocheck(["git", "add", "."], cwd=str(clone_dir))
    if result.returncode != 0:
        print(f"❌ Falha no git add: {result.stderr}", file=sys.stderr)
        return False

    # Verifica se há algo para commitar
    status = run_cmd_nocheck(["git", "status", "--porcelain"], cwd=str(clone_dir))
    if not status.stdout.strip():
        print("ℹ️  Nenhuma alteracao para commitar no clone.")
        return True

    # git commit
    header = f"chore(🔧): adiciona artefatos da mudança {change_name_from_clone(clone_dir)}"
    body = (
        f"Copia os artefatos da mudança OpenSpec para o branch {branch_name}.\n\n"
        "Arquivos:\n"
        "  - proposal.md\n"
        "  - design.md\n"
        "  - tasks.md\n"
        "  - specs/\n\n"
        "Branch pronto para implementacao das tasks. "
        "O PR sera criado manualmente pelo usuario."
    )
    result = run_cmd_nocheck(["git", "commit", "-m", header, "-m", body], cwd=str(clone_dir))
    if result.returncode != 0:
        print(f"❌ Falha no git commit: {result.stderr}", file=sys.stderr)
        return False

    # git push
    result = run_cmd_nocheck(["git", "push", "origin", branch_name], cwd=str(clone_dir))
    if result.returncode != 0:
        print(f"❌ Falha ao fazer push dos artefatos: {result.stderr}", file=sys.stderr)
        return False

    print(f"✅ Artefatos commitados e pushados no branch '{branch_name}'.")
    return True


def change_name_from_clone(clone_dir: Path) -> str:
    """Tenta extrair o nome da mudança a partir do caminho do clone (best effort)."""
    parts = clone_dir.parts
    for part in reversed(parts):
        if part.startswith("plex-setup-"):
            return part.replace("plex-setup-", "")
    return "desconhecida"


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
    clone_dir_name = f"plex-setup-{change_name}-{span_sanitized}"
    clone_dir = PARENT_DIR / clone_dir_name

    print(f"📋 Mudança: {change_name}")
    print(f"📅 Span: {span}")
    print(f"🌿 Branch: {branch_name}")
    print(f"📁 Pasta irmã: {clone_dir}")

    if args.dry_run:
        print("\n🔍 Dry-run concluído. Nenhum comando git foi executado.")
        return 0

    # 3. Cria branch local a partir do main remoto limpo
    if not fetch_origin_main():
        return 1
    if not create_branch_locally(branch_name):
        return 1
    print(f"✅ Branch local '{branch_name}' criado a partir do origin/main.")

    # 4. Push do branch para origin
    if not push_branch(branch_name):
        return 1
    print(f"✅ Branch '{branch_name}' pushado para origin.")

    # 5. Obtém URL do origin
    origin_url = get_origin_url()
    if not origin_url:
        print("❌ Não foi possível obter a URL do remote origin.", file=sys.stderr)
        return 1

    # 6. Cria pasta irmã e clona o branch
    if clone_dir.exists():
        print(f"❌ Pasta irmã já existe: {clone_dir}", file=sys.stderr)
        return 1
    if not clone_branch(branch_name, origin_url, clone_dir):
        return 1
    print(f"✅ Clone criado em: {clone_dir}")

    # 7. Copia artefatos da mudança para o clone
    copied = copy_artifacts_to_clone(change_name, clone_dir)
    if copied:
        print(f"✅ Artefatos copiados: {', '.join(copied)}")
    else:
        print("⚠️  Nenhum artefato foi copiado.")

    # 8. Commit e push dos artefatos no branch remoto
    if not commit_and_push_artifacts(clone_dir, branch_name):
        return 1

    # 9. Remove artefatos da change do repositório original (já estão no outro branch)
    removed = remove_artifacts_from_original(change_name)
    if removed:
        print(f"🗑️  Artefatos removidos do repositório original: {', '.join(removed)}")
    else:
        print("ℹ️  Nenhum artefato para remover do repositório original.")

    print(f"\n✅ Skill setup-spec concluída com sucesso.")
    print(f"   Branch remoto: {branch_name}")
    print(f"   Pasta irmã: {clone_dir}")
    print(f"   O PR deve ser criado manualmente no GitHub.")

    return 0


if __name__ == "__main__":
    sys.exit(main())
