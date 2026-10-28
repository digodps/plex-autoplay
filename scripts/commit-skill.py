#!/usr/bin/env python3
#
# commit-skill.py - Skill de commit para padronizar mensagens e agrupar
# alterações em blocos lógicos (por task do OpenSpec + tipo de mudança).
#
# Padrão de commit (obrigatório em todos os commits, agente e humano):
#   tipo(🎯escopo): título
#   <linha em branco>
#   corpo descritivo livre: por que + o que foi feito
#
# Regras:
#   - tipo: feat | fix | docs | chore | style | refactor | test | perf | ci | build | revert
#   - cada tipo tem um emoji associado (ex: feat=🚀, fix=🐛, docs=📖, chore=🔧)
#   - escopo é opcional (ex: feat(cache): adiciona Redis)
#   - corpo descritivo é obrigatório (não aceita commit sem corpo)
#   - NÃO usar Co-authored-by
#   - commit direto por bloco, sem confirmação intermediária
#
# Agrupamento em blocos lógicos:
#   - Arquivos modificados são mapeados para tasks do OpenSpec (via tasks.md
#     da mudança em andamento, listada por `openspec list --json`).
#   - Agrupa por mesma task + mesmo tipo de mudança.
#   - Cada grupo gera um `git commit` separado (nunca um único commitão).
#
# Uso:
#   python scripts/commit-skill.py [--change <nome>] [--dry-run]
#
# Exemplos de commits gerados:
#   feat(🚀cache): adiciona interface abstrata de cache
#
#   Define o contrato de leitura/gravação usado pelos backends.
#
#   fix(🐛cache): invalida entrada ao salvar seção
#
#   Evita dados obsoletos quando uma seção da biblioteca é alterada.
#
#   docs(📖): registra padrão de commit no README
#
#   Explica como usar tipo(🎯escopo): título + corpo descritivo.

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path
from typing import Optional

# Mapeamento de tipo de commit -> emoji (conventional commit com emoji)
TYPE_EMOJI = {
    "feat": "🚀",
    "fix": "🐛",
    "docs": "📖",
    "chore": "🔧",
    "style": "🎨",
    "refactor": "♻️",
    "test": "✅",
    "perf": "⚡",
    "ci": "👷",
    "build": "📦",
    "revert": "⏪",
}

VALID_TYPES = set(TYPE_EMOJI.keys())

# Extensões/diretórios que sugerem tipo de mudança quando não é possível inferir
TYPE_BY_PATH = [
    (re.compile(r"(^|/)(test|tests|spec|specs)/", re.IGNORECASE), "test"),
    (re.compile(r"\.(md|rst|txt)$", re.IGNORECASE), "docs"),
    (re.compile(r"(^|/)(docs|documentation)/", re.IGNORECASE), "docs"),
    (re.compile(r"(^|/)(.github|ci|workflows?)/", re.IGNORECASE), "ci"),
    (re.compile(r"(^|/)(Dockerfile|docker-compose|\.yml|\.yaml|\.json|\.toml|\.env)$", re.IGNORECASE), "chore"),
    (re.compile(r"\.(py|cs|js|ts|jsx|tsx|go|rs|java|kt|rb|php)$", re.IGNORECASE), "feat"),
]


def run_cmd(cmd: list[str], check: bool = True) -> subprocess.CompletedProcess:
    """Executa um comando e retorna o resultado."""
    return subprocess.run(cmd, capture_output=True, text=True, check=check)


def get_git_status_porcelain() -> list[dict[str, str]]:
    """Lê `git status --porcelain` e retorna lista de arquivos alterados."""
    result = run_cmd(["git", "status", "--porcelain"])
    files: list[dict[str, str]] = []
    for line in result.stdout.splitlines():
        if not line.strip():
            continue
        # Formato: XY caminho (ou XY origem -> destino para renomeios)
        xy = line[:2].strip()
        path = line[3:].strip()
        # Ignora renomeios duplos (R100 origem -> destino)
        if " -> " in path:
            path = path.split(" -> ")[-1].strip()
        files.append({"status": xy, "path": path})
    return files


def get_in_progress_changes() -> list[dict]:
    """Lista mudanças OpenSpec com status in-progress."""
    result = run_cmd(["openspec", "list", "--json"])
    data = json.loads(result.stdout)
    changes = data.get("changes", [])
    return [c for c in changes if c.get("status") == "in-progress"]


def read_tasks_md(change_root: Path) -> Optional[str]:
    """Lê o conteúdo de tasks.md de uma mudança, se existir."""
    tasks_path = change_root / "tasks.md"
    if tasks_path.exists():
        return tasks_path.read_text(encoding="utf-8")
    return None


def extract_file_mentions(tasks_md: str) -> dict[str, list[str]]:
    """
    Extrai menções de caminhos de arquivo das tarefas do tasks.md.
    Retorna um dicionário: tarefa_id -> [caminhos mencionados].
    """
    mentions: dict[str, list[str]] = {}
    # Padrão: "- [ ] 1.1 descrição com `caminho/arquivo.py`"
    task_pattern = re.compile(r"^\s*- \[[ xX]]\s+(\d+\.\d+)\s+(.*)$", re.MULTILINE)
    for match in task_pattern.finditer(tasks_md):
        task_id = match.group(1)
        description = match.group(2)
        # Encontra caminhos entre backticks ou que parecem caminhos de arquivo
        paths = re.findall(r"`([^`]+)`", description)
        # Também captura caminhos soltos (sem backticks) que contenham barra ou ponto
        for token in description.split():
            if ("/" in token or "\\" in token) and ("." in token):
                # Remove pontuação trailing
                clean = token.rstrip(".,;:)")
                if len(clean) > 2:
                    paths.append(clean)
        if paths:
            mentions[task_id] = paths
    return mentions


def infer_type(file_path: str) -> str:
    """Infere o tipo de commit com base no caminho do arquivo."""
    for pattern, t in TYPE_BY_PATH:
        if pattern.search(file_path):
            return t
    return "feat"


# Artefatos de build/gerados que nunca devem ser commitados (mesmo que
# rastreados ou presentes no git status).
ARTIFACT_PATTERN = re.compile(
    r"(^|/)(bin|obj)/|tests/e2e/playwright-report/|tests/e2e/test-results/|test-artifacts/"
)


def is_generated_artifact(path: str) -> bool:
    """Identifica artefatos de build/gerados que não devem virar commit."""
    return bool(ARTIFACT_PATTERN.search(path))


def validate_commit_type(commit_type: str) -> None:
    """Rejeita tipos de commit que não estão na lista de conventional commits válidos."""
    if commit_type not in VALID_TYPES:
        valid = ", ".join(sorted(VALID_TYPES))
        raise ValueError(
            f"Tipo de commit inválido: '{commit_type}'. "
            f"Tipos válidos: {valid}"
        )


def build_header(commit_type: str, scope: Optional[str], title: str) -> str:
    """Constrói o cabeçalho: tipo(🎯escopo): título ou tipo(🎯): título.

    Rejeita tipos inválidos (sem emoji correspondente).
    """
    validate_commit_type(commit_type)
    emoji = TYPE_EMOJI[commit_type]
    if scope:
        return f"{commit_type}({emoji}{scope}): {title}"
    return f"{commit_type}({emoji}): {title}"


def validate_body(body: str) -> None:
    """Rejeita corpos de commit vazios ou apenas espaços em branco."""
    if not body or not body.strip():
        raise ValueError("Corpo do commit não pode estar vazio. Descreva por que e o que foi feito.")


def build_body(commit_type: str, scope: Optional[str], files: list[str], task_desc: Optional[str] = None) -> str:
    """Constrói o corpo descritivo livre (por que + o que foi feito)."""
    emoji = TYPE_EMOJI.get(commit_type, "✨")
    scope_str = f" ({scope})" if scope else ""
    file_list = "\n".join(f"  - {f}" for f in files)
    lines = [
        f"{commit_type}{scope_str}: altera {len(files)} arquivo(s) "
        f"relacionado(s) à tarefa{' ' + task_desc if task_desc else ''}.",
        "",
        "Arquivos afetados:",
        file_list,
        "",
        "Este commit segue o padrão: tipo(🎯escopo): título + corpo descritivo livre.",
    ]
    return "\n".join(lines)


def map_files_to_tasks(
    modified_files: list[dict[str, str]],
    changes: list[dict],
    changes_dir: Path,
) -> dict[str, list[dict[str, str]]]:
    """
    Agrupa arquivos modificados por (change, task_id).
    Arquivos que não mapeiam para nenhuma task ficam no grupo "__uncategorized__".
    """
    # Para cada change em andamento, ler tasks.md e extrair menções de arquivos
    change_task_paths: dict[str, dict[str, list[str]]] = {}
    for change in changes:
        change_name = change["name"]
        change_root = changes_dir / change_name
        tasks_md = read_tasks_md(change_root)
        if tasks_md:
            mentions = extract_file_mentions(tasks_md)
            if mentions:
                change_task_paths[change_name] = mentions

    grouped: dict[str, list[dict[str, str]]] = {}
    uncategorized: list[dict[str, str]] = []

    for file_info in modified_files:
        file_path = file_info["path"]
        matched = False
        for change_name, task_mentions in change_task_paths.items():
            for task_id, paths in task_mentions.items():
                for mentioned in paths:
                    if file_path == mentioned or file_path.endswith(mentioned) or mentioned in file_path:
                        key = f"{change_name}::{task_id}"
                        grouped.setdefault(key, []).append(file_info)
                        matched = True
                        break
                if matched:
                    break
            if matched:
                break
        if not matched:
            uncategorized.append(file_info)

    if uncategorized:
        grouped["__uncategorized__"] = uncategorized

    return grouped


def get_change_root(change_name: str, changes_dir: Path) -> Path:
    return changes_dir / change_name


def commit_block(
    files: list[dict[str, str]],
    commit_type: str,
    scope: Optional[str],
    body: str,
    dry_run: bool = False,
) -> bool:
    """Executa `git add` + `git commit` para um bloco de arquivos."""
    paths = [f["path"] for f in files]

    # git add
    if dry_run:
        print(f"[dry-run] git add {' '.join(paths)}")
    else:
        result = run_cmd(["git", "add"] + paths, check=False)
        if result.returncode != 0:
            print(f"❌ Falha no git add: {result.stderr}", file=sys.stderr)
            return False

    # Título com base nos arquivos
    if len(paths) == 1:
        title = f"altera {paths[0]}"
    else:
        common_dir = os.path.commonpath(paths)
        title = f"altera arquivos em {common_dir}"

    header = build_header(commit_type, scope, title)
    validate_body(body)
    message = f"{header}\n\n{body}"

    if dry_run:
        print(f"[dry-run] git commit -m {message!r}")
        return True

    # Commit com múltiplas -m para cabeçalho e corpo
    commit_cmd = ["git", "commit", "-m", header, "-m", body]
    result = run_cmd(commit_cmd, check=False)
    if result.returncode != 0:
        print(f"❌ Falha no git commit: {result.stderr}", file=sys.stderr)
        return False

    print(f"✅ Commit criado: {header}")
    print(result.stdout.strip())
    return True


def main() -> int:
    parser = argparse.ArgumentParser(description="Skill de commit por blocos lógicos")
    parser.add_argument("--change", default=None, help="Nome da mudança OpenSpec em andamento")
    parser.add_argument("--dry-run", action="store_true", help="Apenas simula, não commita")
    args = parser.parse_args()

    changes_dir = Path("openspec/changes")
    if not changes_dir.exists():
        print("❌ Diretório openspec/changes não encontrado. Rode a partir da raiz do repositório.", file=sys.stderr)
        return 1

    # 1. Lista arquivos modificados (exclui artefatos de build/gerados)
    modified_files = [f for f in get_git_status_porcelain() if not is_generated_artifact(f["path"])]
    if not modified_files:
        print("ℹ️  Nenhum arquivo modificado para commitar.")
        return 0

    print(f"📋 {len(modified_files)} arquivo(s) modificado(s) encontrado(s):")
    for f in modified_files:
        print(f"   {f['status']}  {f['path']}")

    # 2. Descobre mudanças em andamento
    if args.change:
        # --change tem prioridade: usa a mudança informada mesmo que não
        # esteja marcada como in-progress no openspec list.
        change_root = get_change_root(args.change, changes_dir)
        if not change_root.exists():
            print(f"❌ Mudança OpenSpec não encontrada: {args.change}", file=sys.stderr)
            return 1
        changes = [{"name": args.change, "status": "in-progress"}]
    else:
        changes = get_in_progress_changes()
        if not changes:
            print("⚠️  Nenhuma mudança OpenSpec em andamento encontrada.")
            print("   Arquivos serão agrupados sem referência a task.")
            changes = []

    # 3. Mapeia arquivos para tasks
    grouped = map_files_to_tasks(modified_files, changes, changes_dir)

    # 4. Para cada grupo, determina tipo e escopo, cria commit
    all_success = True
    for key, files in grouped.items():
        if key == "__uncategorized__":
            commit_type = "chore"
            scope = None
            body = build_body(commit_type, scope, [f["path"] for f in files])
        else:
            change_name, task_id = key.split("::", 1)
            # Infere tipo do primeiro arquivo (ou do mais comum)
            commit_type = infer_type(files[0]["path"])
            # Tenta extrair escopo do nome da change ou dos caminhos
            scope = None
            for f in files:
                # Tenta usar o diretório pai como escopo
                parts = Path(f["path"]).parts
                if len(parts) > 1:
                    scope = parts[0]
                    break
            # Lê a descrição da task para o corpo
            task_desc = None
            for change in changes:
                if change["name"] == change_name:
                    change_root = get_change_root(change_name, changes_dir)
                    tasks_md = read_tasks_md(change_root)
                    if tasks_md:
                        task_pattern = re.compile(
                            rf"^\s*- \[[ xX]]\s+{re.escape(task_id)}\s+(.*)$", re.MULTILINE
                        )
                        m = task_pattern.search(tasks_md)
                        if m:
                            task_desc = m.group(1).strip()
                            break
                    break
            body = build_body(commit_type, scope, [f["path"] for f in files], task_desc)

        print(f"\n🔨 Processando bloco: {key} (tipo={commit_type}, escopo={scope})")
        if not commit_block(files, commit_type, scope, body, dry_run=args.dry_run):
            all_success = False

    if args.dry_run:
        print("\n🔍 Dry-run concluído. Nenhum commit foi efetivamente criado.")
    elif all_success:
        print("\n✅ Todos os blocos foram commitados com sucesso.")
    else:
        print("\n⚠️  Alguns blocos falharam. Verifique os erros acima.", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    sys.exit(main())
