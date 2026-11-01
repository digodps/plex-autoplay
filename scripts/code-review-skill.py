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


def run_dotnet_lint(files: list[dict[str, str]]) -> list[dict]:
    """Executa dotnet-format e StyleCop.Analyzers (se disponíveis)."""
    findings: list[dict] = []
    cs_files = [f["path"] for f in files]
    if not cs_files:
        return findings

    # Verifica se dotnet-format está disponível (dotnet sempre existe quando
    # o SDK é usado, mas o comando format pode falhar por outro motivo)
    if not tool_available(["dotnet", "format"]):
        findings.append({
            "stack": "dotnet",
            "tool": "lint",
            "severity": "info",
            "message": "Nenhum lint .NET configurado localmente (dotnet-format / StyleCop.Analyzers não encontrados). Instale para ativar a revisão automática.",
            "files": cs_files,
            "reference": "Design - Risco: Lint não configurado localmente",
        })
        return findings

    # dotnet-format exige um workspace (solução/projeto); arquivos .cs soltos
    # causam exceção não tratada. Localiza a solução e usa --include.
    workspace = next(
        (p for p in ("PlexAutoplay.slnx", "PlexAutoplay.sln") if Path(p).exists()),
        next((str(p) for p in Path(".").glob("*/*.csproj")), None),
    )
    if workspace is None:
        findings.append({
            "stack": "dotnet",
            "tool": "lint",
            "severity": "info",
            "message": "Nenhuma solução/projeto .NET encontrado para dotnet-format.",
            "files": cs_files,
            "reference": "Design - Risco: Lint não configurado localmente",
        })
        return findings

    result = run_cmd(
        ["dotnet", "format", workspace, "--verify-no-changes"] + sum((["--include", f] for f in cs_files), []),
        check=False,
    )
    if result.returncode != 0:
        findings.append({
            "stack": "dotnet",
            "tool": "dotnet-format",
            "severity": "warning",
            "message": "dotnet-format detectou diferenças de formatação em arquivos .cs",
            "files": cs_files,
            "reference": "Clean Code - formatação consistente",
        })

    # Verifica se StyleCop está disponível (via dotnet)
    result = run_cmd(["dotnet", "build", "-v:q"], check=False)
    if result.returncode != 0 and "StyleCop" in result.stderr:
        findings.append({
            "stack": "dotnet",
            "tool": "StyleCop.Analyzers",
            "severity": "warning",
            "message": "StyleCop.Analyzers detectou violações de estilo C#",
            "files": cs_files,
            "reference": "Clean Code - nomenclatura e documentação XML",
        })

    # Lint limpo não é achado: não emite "não configurado" quando a
    # verificação passou sem diferenças.
    return findings


def run_web_lint(files: list[dict[str, str]]) -> list[dict]:
    """Executa ESLint e Prettier (se disponíveis)."""
    findings: list[dict] = []
    web_files = [f["path"] for f in files]
    if not web_files:
        return findings

    # Verifica se ESLint está disponível (antes de interpretar exit != 0 como violação)
    if not tool_available(["npx", "eslint"]):
        findings.append({
            "stack": "web",
            "tool": "lint",
            "severity": "info",
            "message": "ESLint não configurado localmente. Instale para ativar a revisão automática.",
            "files": web_files,
            "reference": "Design - Risco: Lint não configurado localmente",
        })
    else:
        result = run_cmd(["npx", "eslint"] + web_files, check=False)
        if result.returncode != 0:
            findings.append({
                "stack": "web",
                "tool": "ESLint",
                "severity": "warning",
                "message": "ESLint detectou violações de estilo em arquivos JS/Vue",
                "files": web_files,
                "reference": "Clean Code - formatação e estilo JS/Vue",
            })

    # Verifica se Prettier está disponível
    if not tool_available(["npx", "prettier"]):
        findings.append({
            "stack": "web",
            "tool": "lint",
            "severity": "info",
            "message": "Prettier não configurado localmente. Instale para ativar a revisão automática.",
            "files": web_files,
            "reference": "Design - Risco: Lint não configurado localmente",
        })
    else:
        result = run_cmd(["npx", "prettier", "--check"] + web_files, check=False)
        if result.returncode != 0:
            findings.append({
                "stack": "web",
                "tool": "Prettier",
                "severity": "warning",
                "message": "Prettier detectou diferenças de formatação em arquivos JS/Vue",
                "files": web_files,
                "reference": "Clean Code - formatação consistente",
            })

    # Lint limpo não é achado: não emite "não configurado" quando a
    # verificação passou sem diferenças.
    return findings


def map_owasp_findings(findings: list[dict]) -> list[dict]:
    """Cruza achados com OWASP Top 10 (2021) quando aplicável."""
    for finding in findings:
        message = finding.get("message", "").lower()
        if "injeção" in message or "input" in message or "validation" in message:
            finding["owasp"] = "A01:2021 - Injection"
        elif "autenticação" in message or "jwt" in message or "token" in message:
            finding["owasp"] = "A02:2021 - Broken Authentication"
        elif "dado sensível" in message or "sensitive" in message:
            finding["owasp"] = "A03:2021 - Sensitive Data Exposure"
        elif "xss" in message or "cross-site" in message:
            finding["owasp"] = "A07:2021 - Cross-Site Scripting (XSS)"
        elif "deserialização" in message or "deserialization" in message:
            finding["owasp"] = "A08:2021 - Insecure Deserialization"
        elif "componente" in message or "vulnerável" in message or "desatualizado" in message:
            finding["owasp"] = "A09:2021 - Vulnerable and Outdated Components"
        elif "logging" in message or "monitoramento" in message:
            finding["owasp"] = "A10:2021 - Insufficient Logging & Monitoring"
        elif "controle de acesso" in message or "access control" in message:
            finding["owasp"] = "A05:2021 - Broken Access Control"
        elif "configuração" in message or "misconfiguration" in message:
            finding["owasp"] = "A06:2021 - Security Misconfiguration"
        elif "xxe" in message or "external entities" in message:
            finding["owasp"] = "A04:2021 - XML External Entities (XXE)"
    return findings


def map_nist_findings(findings: list[dict]) -> list[dict]:
    """Mapeia práticas para NIST SSDF quando aplicável."""
    for finding in findings:
        message = finding.get("message", "").lower()
        if "não configurado" in message or "não encontrado" in message:
            finding["nist_ssdf"] = "PW.3 - Protect Well: identify and protect sensitive data"
        elif "vulnerabilidade" in message:
            finding["nist_ssdf"] = "RV.1 - Respond to Vulnerabilities: vulnerability disclosure"
    return findings


def evaluate_clean_code_solid(files: list[dict[str, str]]) -> list[dict]:
    """Avalia princípios de Clean Code e SOLID nos arquivos varridos."""
    findings: list[dict] = []
    for file_info in files:
        path = file_info["path"]
        try:
            content = Path(path).read_text(encoding="utf-8")
        except Exception:
            continue

        lines = content.splitlines()

        # Heurística simples: classe muito longa (> 200 linhas) -> possível SRP violado
        if len(lines) > 200:
            findings.append({
                "stack": detect_stack(path) or "unknown",
                "tool": "Clean Code / SOLID",
                "severity": "warning",
                "message": f"Classe com mais de 200 linhas ({len(lines)}) - possível violação do SRP (Responsabilidade Única)",
                "files": [path],
                "reference": "SOLID - S (Single Responsibility Principle)",
            })

        # Heurística: instanciação direta de dependências concretas -> possível DIP violado
        if "new " in content and ("Service" in content or "Repository" in content or "Context" in content):
            findings.append({
                "stack": detect_stack(path) or "unknown",
                "tool": "Clean Code / SOLID",
                "severity": "warning",
                "message": "Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)",
                "files": [path],
                "reference": "SOLID - D (Dependency Inversion Principle)",
            })

    return findings


def collect_findings() -> list[dict]:
    """Varre o working tree e coleta todos os achados por stack."""
    all_files = get_git_status_porcelain()
    grouped = filter_supported_files(all_files)

    findings: list[dict] = []

    for stack, files in grouped.items():
        if stack == "dotnet":
            findings.extend(run_dotnet_lint(files))
        elif stack == "web":
            findings.extend(run_web_lint(files))

        # Avaliação Clean Code / SOLID para todos os arquivos suportados
        findings.extend(evaluate_clean_code_solid(files))

    # Mapeamento de referências de segurança
    findings = map_owasp_findings(findings)
    findings = map_nist_findings(findings)

    return findings


def display_findings(findings: list[dict]) -> None:
    """Exibe os achados numerados para o usuário."""
    if not findings:
        print("✅ Nenhum achado encontrado nas stacks suportadas.")
        return

    print(f"\n🔍 {len(findings)} achado(s) encontrado(s):\n")
    for i, finding in enumerate(findings, start=1):
        stack = finding.get("stack", "?")
        tool = finding.get("tool", "lint")
        severity = finding.get("severity", "info")
        message = finding.get("message", "")
        files = ", ".join(finding.get("files", []))
        refs = []
        if finding.get("owasp"):
            refs.append(f"OWASP {finding['owasp']}")
        if finding.get("nist_ssdf"):
            refs.append(f"NIST SSDF {finding['nist_ssdf']}")
        if finding.get("reference"):
            refs.append(f"Ref: {finding['reference']}")
        ref_str = " | ".join(refs)
        print(f"  [{i}] [{stack}] [{severity}] {tool}")
        print(f"      {message}")
        if files:
            print(f"      Arquivos: {files}")
        if ref_str:
            print(f"      {ref_str}")
        print()


def ask_what_to_fix(findings: list[dict]) -> list[int]:
    """Pergunta interativamente ao usuário quais itens deseja corrigir."""
    if not findings:
        return []

    print("Quais itens deseja corrigir? (ex: 1, 3 ou 'todos' ou 'nenhum')")
    try:
        response = input("> ").strip()
    except EOFError:
        print("ℹ️  Entrada interativa não disponível. Use --fix 1,2,3 para selecionar itens.")
        return []

    response = response.lower().strip()
    if response in ("nenhum", "n", "0", ""):
        return []
    if response in ("todos", "t", "all", "a"):
        return list(range(1, len(findings) + 1))

    selected: list[int] = []
    for token in response.replace(" ", "").split(","):
        try:
            num = int(token)
            if 1 <= num <= len(findings):
                selected.append(num)
        except ValueError:
            continue

    return sorted(set(selected))


def generate_fix_specs(findings: list[dict], selected: list[int]) -> None:
    """Gera specs de correção OpenSpec para os itens selecionados."""
    changes_dir = Path("openspec/changes")
    if not changes_dir.exists():
        print("❌ Diretório openspec/changes não encontrado.", file=sys.stderr)
        return

    for num in selected:
        finding = findings[num - 1]
        change_name = f"fix-{num}-{finding.get('tool', 'item').replace(' ', '-').lower()}"
        change_root = changes_dir / change_name
        change_root.mkdir(parents=True, exist_ok=True)

        # proposal.md
        proposal = f"""# Proposal

## Why

Correção do achado [{num}] da revisão de código: {finding.get('message', '')}

## What Changes

- Corrigir o problema identificado: {finding.get('message', '')}
- Arquivos afetados: {', '.join(finding.get('files', []))}

## Capabilities

### New Capabilities

- code-review-fix-{num}: Correção específica do achado [{num}] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: {', '.join(finding.get('files', []))}
- Referências: {finding.get('owasp', '')} {finding.get('nist_ssdf', '')} {finding.get('reference', '')}
"""
        (change_root / "proposal.md").write_text(proposal, encoding="utf-8")

        # design.md
        design = f"""# Design

## Context

Achado [{num}] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: {finding.get('message', '')}

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: {', '.join(finding.get('files', []))}

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
"""
        (change_root / "design.md").write_text(design, encoding="utf-8")

        # specs/<cap>/spec.md
        spec_dir = change_root / "specs" / f"code-review-fix-{num}"
        spec_dir.mkdir(parents=True, exist_ok=True)
        spec = f"""# Spec Delta

## Purpose

Corrigir o achado [{num}] da revisão de código: {finding.get('message', '')}

## ADDED Requirements

### Requirement: Corrigir achado [{num}]
O sistema SHALL corrigir o problema: {finding.get('message', '')}

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos {', '.join(finding.get('files', []))}
- **THEN** o achado [{num}] não é mais reportado na próxima execução de /code-review
"""
        (spec_dir / "spec.md").write_text(spec, encoding="utf-8")

        # tasks.md
        tasks = f"""# Tasks

## 1. Correção do achado [{num}]

- [ ] 1.1 Aplicar correção nos arquivos: {', '.join(finding.get('files', []))} e verificar que o achado desaparece na próxima execução de /code-review

## Workflow follow-up

- Validar com `openspec validate --change {change_name}` antes de seguir para archive/apply.
"""
        (change_root / "tasks.md").write_text(tasks, encoding="utf-8")

        print(f"✅ Change de correção criada: {change_name}/")
        print(f"   proposal.md, design.md, specs/{change_root}/spec.md, tasks.md")


def main() -> int:
    parser = argparse.ArgumentParser(description="Skill de revisão de código por stacks .NET/C# e JS/Vue")
    parser.add_argument("--fix", default=None, help="Itens para corrigir (ex: 1,3 ou 'todos')")
    parser.add_argument("--dry-run", action="store_true", help="Apenas simula, não gera specs")
    args = parser.parse_args()

    if not Path("openspec/changes").exists():
        print("❌ Diretório openspec/changes não encontrado. Rode a partir da raiz do repositório.", file=sys.stderr)
        return 1

    # 1. Varre o working tree e coleta achados
    findings = collect_findings()

    # 2. Exibe achados
    display_findings(findings)

    if not findings:
        print("ℹ️  Nada a corrigir.")
        return 0

    # 3. Determina itens para correção
    if args.fix:
        if args.fix.lower() in ("todos", "t", "all", "a"):
            selected = list(range(1, len(findings) + 1))
        else:
            selected = []
            for token in args.fix.replace(" ", "").split(","):
                try:
                    num = int(token)
                    if 1 <= num <= len(findings):
                        selected.append(num)
                except ValueError:
                    continue
            selected = sorted(set(selected))
    else:
        # Pergunta interativamente
        selected = ask_what_to_fix(findings)

    if not selected:
        print("ℹ️  Nenhum item selecionado para correção.")
        return 0

    # 4. Gera specs de correção
    if args.dry_run:
        print(f"\n[dry-run] Geraria specs de correção para itens: {selected}")
    else:
        generate_fix_specs(findings, selected)
        print(f"\n✅ Specs de correção geradas para {len(selected)} item(ns).")
        print("   Rode `openspec validate --change <nome>` em cada change gerada antes de aplicar.")

    return 0


if __name__ == "__main__":
    sys.exit(main())