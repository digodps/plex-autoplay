#!/usr/bin/env python3
"""Copy canonical agent config from .agent-source to every supported client."""
from pathlib import Path
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / ".agent-source"
TARGETS = [".agents", ".claude", ".continue", ".opencode", ".github", ".dsh"]


def copy_tree(src: Path, dst: Path) -> None:
    if not src.exists():
        return
    dst.mkdir(parents=True, exist_ok=True)
    for item in src.iterdir():
        target = dst / item.name
        if item.is_dir():
            if target.exists():
                shutil.rmtree(target)
            shutil.copytree(item, target)
        else:
            shutil.copy2(item, target)


def sync() -> None:
    if not SOURCE.is_dir():
        raise SystemExit(f"Missing source directory: {SOURCE}")

    for client in TARGETS:
        target = ROOT / client
        if not target.is_dir():
            continue
        copy_tree(SOURCE / "skills", target / "skills")
        copy_tree(SOURCE / "commands", target / "commands")
        copy_tree(SOURCE / "workflows", target / "workflows")
        copy_tree(SOURCE / "prompts", target / "prompts")
    print(f"Synced {SOURCE} to {', '.join(TARGETS)}")


if __name__ == "__main__":
    sync()
