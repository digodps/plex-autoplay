#!/usr/bin/env python3
"""Create skill from OpenSpec proposal, then sync all clients."""
from pathlib import Path
import argparse
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / ".agent-source"
TEMPLATE = SOURCE / "skills" / "create-skill" / "template.md"
SYNC = ROOT / "scripts" / "sync-agent-configs.py"


def kebab(value: str) -> str:
    result = re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")
    if not result:
        raise SystemExit("Skill name must contain letters or numbers")
    return result


def ensure_change(name: str, description: str) -> Path:
    change = ROOT / "openspec" / "changes" / name
    if not change.is_dir():
        print(f"Creating OpenSpec change '{name}'...")
        subprocess.run(["openspec", "new", "change", name], cwd=ROOT, check=True)
    
    # Check if artifacts exist, if not create dummy ones or run propose
    proposal = change / "proposal.md"
    if not proposal.exists():
        print(f"Generating default OpenSpec artifacts for '{name}'...")
        proposal.write_text(f"# Proposal: {name}\n\n{description}\n")
        (change / "design.md").write_text(f"# Design: {name}\n\nArchitecture & design details.\n")
        (change / "tasks.md").write_text(f"# Tasks: {name}\n\n- [ ] Implement skill {name}\n")
    return change


def read_material(change: Path) -> str:
    files = [change / "proposal.md", change / "design.md", change / "tasks.md"]
    parts = []
    for file in files:
        if file.exists():
            parts.append(f"## {file.name}\n{file.read_text()}")
    return "\n\n".join(parts)


def create(args: argparse.Namespace) -> None:
    name = kebab(args.name)
    description = args.description or f"Skill generated from OpenSpec change {name}."
    destination = SOURCE / "skills" / name / "SKILL.md"

    if destination.exists() and not args.force:
        raise SystemExit(f"Skill already exists: {destination}. Use --force to update.")

    change = ensure_change(name, description)
    material = read_material(change)
    text = TEMPLATE.read_text()
    text = text.replace("{{NAME}}", name).replace("{{DESCRIPTION}}", description)
    text += f"\n\n## OpenSpec Material\n\n{material}\n"

    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(text)
    subprocess.run([sys.executable, str(SYNC)], cwd=ROOT, check=True)
    print(f"Skill created/updated: {destination}")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("name")
    parser.add_argument("--desc", "--description", dest="description")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()
    create(args)


if __name__ == "__main__":
    main()
