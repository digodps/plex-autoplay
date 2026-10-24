"""Persistent configuration for media libraries within a read-only root."""

from __future__ import annotations

import json
import os
from pathlib import Path
import tempfile
from typing import Dict, List

from plex_autoplay.domain.tvshow import TVLibrary


class LibraryRepository:
    """Stores relative media paths and their inventory/enrichment kind."""

    VALID_KINDS = {"anime", "series"}

    def __init__(self, media_root: str | Path, config_file: str | Path):
        self.media_root = Path(media_root).expanduser().resolve()
        self.config_file = Path(config_file).expanduser()
        if not self.media_root.is_dir():
            raise ValueError(f"Raiz de mídia inválida: {media_root}")

    def list_libraries(self) -> List[TVLibrary]:
        if not self.config_file.exists():
            return []
        with self.config_file.open("r", encoding="utf-8") as config_file:
            payload = json.load(config_file)
        if not isinstance(payload, list):
            raise ValueError(f"Configuração de bibliotecas inválida: {self.config_file}")
        libraries = []
        for item in payload:
            if (
                not isinstance(item, dict)
                or not isinstance(item.get("name"), str)
                or not isinstance(item.get("path"), str)
                or item.get("kind") not in self.VALID_KINDS
            ):
                raise ValueError(f"Entrada de biblioteca inválida: {self.config_file}")
            libraries.append(TVLibrary(name=item["name"], path=item["path"], kind=item["kind"]))
        return libraries

    def list_directories(self, relative_path: str = "") -> List[str]:
        parent = self.media_root
        if relative_path:
            parent, _ = self._resolve_relative(relative_path)
        if not parent.exists() or not parent.is_dir():
            raise ValueError(f"Diretório inexistente sob a raiz de mídia: {relative_path}")
        directories = []
        for child in sorted(parent.iterdir(), key=lambda item: item.name.casefold()):
            if not child.is_dir():
                continue
            try:
                resolved = child.resolve()
                relative = resolved.relative_to(self.media_root).as_posix()
            except (OSError, ValueError):
                continue
            directories.append(relative)
        return directories

    def add_library(self, relative_path: str, kind: str) -> TVLibrary:
        if kind not in self.VALID_KINDS:
            raise ValueError(f"Tipo de biblioteca inválido: {kind}")
        resolved, relative = self._resolve_selection(relative_path)
        if any(self._stored_path(library.path) == resolved for library in self.list_libraries()):
            raise ValueError(f"Biblioteca já cadastrada: {relative}")
        library = TVLibrary(name=resolved.name, path=relative, kind=kind)
        libraries = self.list_libraries()
        libraries.append(library)
        self._write(libraries)
        return library

    def remove_library(self, relative_path: str) -> bool:
        target, relative = self._resolve_relative(relative_path)
        libraries = self.list_libraries()
        remaining = [library for library in libraries if self._stored_path(library.path) != target]
        if len(remaining) == len(libraries):
            return False
        self._write(remaining)
        return True

    def _resolve_selection(self, relative_path: str) -> tuple[Path, str]:
        resolved, relative = self._resolve_relative(relative_path)
        if resolved == self.media_root:
            raise ValueError("A raiz inteira não pode ser cadastrada como biblioteca.")
        if not resolved.exists() or not resolved.is_dir():
            raise ValueError(f"Diretório de biblioteca inexistente: {relative_path}")
        return resolved, relative

    def _resolve_relative(self, relative_path: str) -> tuple[Path, str]:
        candidate_path = Path(relative_path)
        if candidate_path.is_absolute() or not relative_path.strip():
            raise ValueError("O caminho da biblioteca deve ser relativo à raiz de mídia.")
        if ".." in candidate_path.parts:
            raise ValueError("O caminho da biblioteca não pode conter traversal.")
        resolved = (self.media_root / candidate_path).resolve()
        try:
            relative = resolved.relative_to(self.media_root).as_posix()
        except ValueError as error:
            raise ValueError("O caminho da biblioteca escapa da raiz de mídia.") from error
        return resolved, relative

    def _stored_path(self, relative_path: str) -> Path:
        return (self.media_root / relative_path).resolve()

    def _write(self, libraries: List[TVLibrary]) -> None:
        self.config_file.parent.mkdir(parents=True, exist_ok=True)
        payload: List[Dict[str, str]] = [
            {"name": library.name, "path": library.path, "kind": library.kind}
            for library in sorted(libraries, key=lambda item: item.path.casefold())
        ]
        temporary_path = None
        try:
            with tempfile.NamedTemporaryFile(
                mode="w",
                encoding="utf-8",
                dir=self.config_file.parent,
                prefix=f".{self.config_file.name}.",
                delete=False,
            ) as temporary_file:
                temporary_path = Path(temporary_file.name)
                json.dump(payload, temporary_file, ensure_ascii=False, indent=2)
                temporary_file.write("\n")
                temporary_file.flush()
                os.fsync(temporary_file.fileno())
            os.chmod(temporary_path, 0o600)
            os.replace(temporary_path, self.config_file)
        finally:
            if temporary_path is not None and temporary_path.exists():
                temporary_path.unlink()
