#!/usr/bin/env python3
#
# dev-spec.py - Script de suporte e orquestração do workflow multi-agente /dev-spec.
#
# Valida pré-requisitos (.env, Distrobox, gh CLI, openspec), auxilia na execução
# dos testes em container, geração de commits convencionais e abertura de Pull Request.
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

    # 2. Verifica distrobox
    if not shutil.which("distrobox"):
        errors.append("CLI 'distrobox' não encontrado no sistema.")

    # 3. Verifica gh CLI e autenticação
    gh_bin = find_gh_cli()
    if not gh_bin:
        errors.append("CLI 'gh' (GitHub CLI) não encontrado no PATH.")
    else:
        res = run_cmd_nocheck([gh_bin, "auth", "status"])
        if res.returncode != 0:
            errors.append(f"GitHub CLI ('gh') não está autenticado: {res.stderr or res.stdout}")

    # 4. Verifica openspec CLI
    if not shutil.which("openspec"):
        errors.append("CLI 'openspec' não encontrado no PATH.")

    return len(errors) == 0, errors


def get_active_change(target_name: Optional[str]) -> Optional[str]:
    """Identifica a mudança OpenSpec ativa."""
    res = run_cmd_nocheck(["openspec", "list", "--json"])
    if res.returncode != 0:
        return None
    try:
        data = json.loads(res.stdout)
        changes = data.get("changes", [])
        in_progress = [c for c in changes if c.get("status") == "in-progress"]
        if target_name:
            match = next((c["name"] for c in changes if c["name"] == target_name), None)
            return match
        if len(in_progress) == 1:
            return in_progress[0]["name"]
        return None
    except Exception:
        return None


def run_unit_tests(dry_run: bool = False) -> bool:
    """Executa testes unitários .NET dentro do Distrobox."""
    cmd = ["distrobox", "enter", "fedora", "--", "dotnet", "test", "tests/unit/PlexAutoplay.UnitTests.csproj"]
    print(f"🧪 Executando testes unitários: {' '.join(cmd)}")
    if dry_run:
        print("   [DRY-RUN] Testes unitários simulados com sucesso.")
        return True
    res = run_cmd_nocheck(cmd, capture_output=False)
    return res.returncode == 0


def run_e2e_tests(dry_run: bool = False) -> bool:
    """Executa testes E2E Playwright via Docker Compose no Distrobox."""
    up_cmd = [
        "distrobox", "enter", "fedora", "--",
        "docker", "compose", "-f", "docker-compose.test.yml", "up",
        "--build", "--abort-on-container-exit", "--exit-code-from", "executor"
    ]
    down_cmd = [
        "distrobox", "enter", "fedora", "--",
        "docker", "compose", "-f", "docker-compose.test.yml", "down", "-v"
    ]
    print(f"🐳 Executando stack E2E: {' '.join(up_cmd)}")
    if dry_run:
        print("   [DRY-RUN] Stack E2E simulada com sucesso.")
        return True

    success = False
    try:
        res = run_cmd_nocheck(up_cmd, capture_output=False)
        success = (res.returncode == 0)
    finally:
        print(f"🧹 Derrubando stack de testes: {' '.join(down_cmd)}")
        run_cmd_nocheck(down_cmd, capture_output=True)

    return success


def run_commit_skill(change_name: str, dry_run: bool = False) -> bool:
    """Executa scripts/commit-skill.py para agrupar alterações em commits semânticos."""
    cmd = ["python3", "scripts/commit-skill.py", "--change", change_name]
    if dry_run:
        cmd.append("--dry-run")
    print(f"📝 Executando commit-skill: {' '.join(cmd)}")
    res = run_cmd_nocheck(cmd, capture_output=False)
    return res.returncode == 0


def create_pull_request(change_name: str, branch_name: str, dry_run: bool = False) -> bool:
    """Abre o Pull Request no GitHub usando o gh CLI."""
    gh_bin = find_gh_cli() or "gh"
    proposal_file = CHANGES_DIR / change_name / "proposal.md"
    title = f"feat({change_name}): implementa mudanca OpenSpec"
    body = f"Implementação completa da mudança OpenSpec `{change_name}`.\n\n"

    if proposal_file.exists():
        body += f"Ref: `openspec/changes/{change_name}/`\n"

    cmd = [
        gh_bin, "pr", "create",
        "--title", title,
        "--body", body,
        "--base", "main",
        "--head", branch_name
    ]
    print(f"🚀 Criando Pull Request: {' '.join(cmd)}")
    if dry_run:
        print(f"   [DRY-RUN] Pull Request simulado para {branch_name} -> main.")
        return True

    res = run_cmd_nocheck(cmd, capture_output=False)
    return res.returncode == 0


def main() -> int:
    parser = argparse.ArgumentParser(description="Orquestrador e suporte do workflow /dev-spec")
    parser.add_argument("--change", default=None, help="Nome da mudança OpenSpec")
    parser.add_argument("--dry-run", action="store_true", help="Simula os passos sem modificar código ou git")
    parser.add_argument("--skip-tests", action="store_true", help="Pula execução da suíte de testes")
    parser.add_argument("--skip-pr", action="store_true", help="Pula abertura automática do Pull Request")
    parser.add_argument("--check-only", action="store_true", help="Apenas valida os pré-requisitos do ambiente")
    args = parser.parse_args()

    print("🔍 [1/5] Verificando pré-requisitos do ambiente...")
    ok, errors = check_prerequisites()
    if not ok:
        print("❌ Pré-requisitos não atendidos:")
        for err in errors:
            print(f"   - {err}")
        return 1
    print("✅ Pré-requisitos atendidos (.env, Distrobox, gh CLI e openspec).")

    if args.check_only:
        return 0

    change_name = get_active_change(args.change)
    if not change_name:
        print("❌ Nenhuma mudança OpenSpec válida identificada.", file=sys.stderr)
        return 1
    print(f"📋 Mudança selecionada: {change_name}")

    # Identifica branch atual
    res = run_cmd_nocheck(["git", "branch", "--show-current"])
    branch_name = res.stdout.strip() if res.returncode == 0 else ""

    if not args.skip_tests:
        print("\n🧪 [2/5] Executando testes unitários (.NET)...")
        if not run_unit_tests(dry_run=args.dry_run):
            print("❌ Testes unitários falharam.")
            return 1
        print("✅ Testes unitários aprovados com 100% de sucesso.")

        print("\n🐳 [3/5] Executando testes E2E (Container no Distrobox)...")
        if not run_e2e_tests(dry_run=args.dry_run):
            print("❌ Testes E2E falharam.")
            return 1
        print("✅ Testes E2E aprovados com 100% de sucesso.")
    else:
        print("\n⏩ [2/5 e 3/5] Testes ignorados (--skip-tests).")

    print("\n📝 [4/5] Agrupando commits via commit-skill...")
    if not run_commit_skill(change_name, dry_run=args.dry_run):
        print("⚠️ Falha ou nenhuma alteração pendente para commit.")

    if not args.skip_pr:
        print("\n🚀 [5/5] Abrindo Pull Request no GitHub...")
        if not create_pull_request(change_name, branch_name or "HEAD", dry_run=args.dry_run):
            print("❌ Falha ao abrir Pull Request.")
            return 1
        print("✅ Pull Request aberto com sucesso!")
    else:
        print("\n⏩ [5/5] Criação de PR ignorada (--skip-pr).")

    print("\n✨ Workflow /dev-spec concluído com sucesso!")
    return 0


if __name__ == "__main__":
    sys.exit(main())
