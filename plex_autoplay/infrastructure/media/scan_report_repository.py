"""Atomic persistence for the latest dashboard scan report."""

from __future__ import annotations

import json
import os
from pathlib import Path
import tempfile
from typing import Any, Dict, Optional


class ScanReportRepository:
    def __init__(self, report_file: str | Path):
        self.report_file = Path(report_file).expanduser()

    def load(self) -> Optional[Dict[str, Any]]:
        if not self.report_file.exists():
            return None
        try:
            with self.report_file.open("r", encoding="utf-8") as report_file:
                report = json.load(report_file)
        except (OSError, json.JSONDecodeError) as error:
            raise ValueError(f"Não foi possível ler o relatório {self.report_file}: {error}") from error
        if not isinstance(report, dict):
            raise ValueError(f"Relatório inválido em {self.report_file}: esperado um objeto JSON.")
        return report

    def save(self, report: Dict[str, Any]) -> None:
        if not isinstance(report, dict):
            raise ValueError("O relatório deve ser um objeto JSON.")
        try:
            serialized = json.dumps(report, ensure_ascii=False, indent=2, allow_nan=False) + "\n"
        except (TypeError, ValueError) as error:
            raise ValueError(f"Relatório não serializável: {error}") from error

        self.report_file.parent.mkdir(parents=True, exist_ok=True)
        mode = self.report_file.stat().st_mode & 0o777 if self.report_file.exists() else 0o600
        temporary_path: Optional[Path] = None
        try:
            with tempfile.NamedTemporaryFile(
                mode="w",
                encoding="utf-8",
                dir=self.report_file.parent,
                prefix=f".{self.report_file.name}.",
                delete=False,
            ) as temporary_file:
                temporary_path = Path(temporary_file.name)
                temporary_file.write(serialized)
                temporary_file.flush()
                os.fsync(temporary_file.fileno())
            os.chmod(temporary_path, mode)
            os.replace(temporary_path, self.report_file)
            directory_fd = os.open(self.report_file.parent, os.O_RDONLY)
            try:
                os.fsync(directory_fd)
            finally:
                os.close(directory_fd)
        finally:
            if temporary_path is not None and temporary_path.exists():
                temporary_path.unlink()
