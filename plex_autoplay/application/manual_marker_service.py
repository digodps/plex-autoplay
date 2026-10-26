"""Validated, atomic operations for manually maintained episode markers."""

from __future__ import annotations

import json
import math
import os
import tempfile
import threading
from collections.abc import Iterable
from pathlib import Path
from typing import ClassVar


class MarkerConflictError(ValueError):
    """Raised when an update would replace an existing marker without approval."""


class ManualMarkerService:
    VALID_FIELDS: ClassVar[set[str]] = {"start", "end", "credits"}
    _write_lock: ClassVar[threading.RLock] = threading.RLock()

    def __init__(self, markers_file: str | Path):
        self.markers_file = Path(markers_file).expanduser()

    def get_values(
        self, series_title: str, season: int, episode: int
    ) -> dict[str, float]:
        self._validate_location(series_title, season, episode)
        with self._write_lock:
            data = self._read()
            series_key = self._find_series_key(data, series_title)
            if series_key is None:
                return {}
            show_data = data[series_key]
            values: dict[str, float] = {}
            default_entry = show_data.get("default")
            if isinstance(default_entry, dict):
                defaults = {
                    key: value
                    for key, value in default_entry.items()
                    if key in self.VALID_FIELDS
                }
                values.update(self._validate_values(defaults))
            season_data = show_data.get(f"season_{season}")
            if isinstance(season_data, dict):
                episode_data = season_data.get(str(episode))
                if isinstance(episode_data, dict):
                    custom = {
                        key: value
                        for key, value in episode_data.items()
                        if key in self.VALID_FIELDS
                    }
                    values.update(self._validate_values(custom))
            self._validate_values(values)
            return values

    def update(
        self,
        series_title: str,
        season: int,
        episode: int,
        values: dict[str, object],
        *,
        confirm_replace: bool = False,
    ) -> dict[str, float]:
        normalized = self._validate_values(values)
        if not normalized:
            raise ValueError("Informe pelo menos um marcador para salvar.")
        self._validate_location(series_title, season, episode)

        with self._write_lock:
            data = self._read()
            series_key = self._find_series_key(data, series_title)
            if series_key is None:
                series_key = series_title.strip()
                data[series_key] = {}
            season_data = data[series_key].setdefault(f"season_{season}", {})
            if not isinstance(season_data, dict):
                raise TypeError(
                    f"Estrutura de marcadores inválida para {series_key}, temporada {season}."
                )
            episode_key = str(episode)
            episode_data = season_data.setdefault(episode_key, {})
            if not isinstance(episode_data, dict):
                raise TypeError(
                    f"Estrutura de marcadores inválida para {series_key}, episódio {episode}."
                )
            current_values = {
                key: value
                for key, value in episode_data.items()
                if key in self.VALID_FIELDS
            }
            self._validate_values(current_values)

            replacements = {
                key: value
                for key, value in normalized.items()
                if key in episode_data and episode_data[key] != value
            }
            if replacements and not confirm_replace:
                raise MarkerConflictError(
                    f"Marcadores existentes seriam substituídos: {', '.join(sorted(replacements))}."
                )
            merged_values = dict(current_values)
            merged_values.update(normalized)
            self._validate_values(merged_values)
            episode_data.update(normalized)
            self._write(data)
            return {
                key: float(value)
                for key, value in episode_data.items()
                if key in self.VALID_FIELDS
            }

    def remove_fields(
        self,
        series_title: str,
        season: int,
        episode: int,
        fields: Iterable[str],
    ) -> dict[str, float]:
        selected = set(fields)
        if not selected or not selected.issubset(self.VALID_FIELDS):
            raise ValueError(
                "Selecione um ou mais campos válidos: start, end ou credits."
            )
        self._validate_location(series_title, season, episode)
        with self._write_lock:
            data = self._read()
            series_key = self._find_series_key(data, series_title)
            if series_key is None:
                raise ValueError(f"Série não encontrada: {series_title}")
            season_data = data.get(series_key, {}).get(f"season_{season}")
            if not isinstance(season_data, dict):
                raise TypeError("Temporada não encontrada.")
            episode_key = str(episode)
            episode_data = season_data.get(episode_key)
            if not isinstance(episode_data, dict):
                raise TypeError("Episódio não encontrado.")
            for field in selected:
                episode_data.pop(field, None)
            if not episode_data:
                del season_data[episode_key]
            self._write(data)
            return {
                key: float(value)
                for key, value in episode_data.items()
                if key in self.VALID_FIELDS
            }

    def _read(self) -> dict[str, object]:
        if not self.markers_file.exists():
            return {}
        try:
            with self.markers_file.open("r", encoding="utf-8") as markers_file:
                data = json.load(markers_file)
        except (OSError, json.JSONDecodeError) as error:
            raise ValueError(
                f"Não foi possível ler {self.markers_file}: {error}"
            ) from error
        if not isinstance(data, dict):
            raise TypeError(
                f"Formato inválido em {self.markers_file}: esperado um objeto JSON."
            )
        return data

    @staticmethod
    def _find_series_key(data: dict[str, object], title: str) -> str | None:
        matches = [
            key
            for key, value in data.items()
            if isinstance(key, str)
            and key.strip().casefold() == title.strip().casefold()
            and isinstance(value, dict)
        ]
        if len(matches) > 1:
            raise ValueError(
                f"Mais de uma série corresponde ao título '{title}'; escolha uma chave explícita."
            )
        return matches[0] if matches else None

    @staticmethod
    def _validate_location(series_title: str, season: int, episode: int) -> None:
        if (
            not isinstance(series_title, str)
            or not series_title.strip()
            or isinstance(season, bool)
            or not isinstance(season, int)
            or season < 0
            or isinstance(episode, bool)
            or not isinstance(episode, int)
            or episode < 1
        ):
            raise ValueError("Série, temporada e episódio inválidos.")

    @classmethod
    def _validate_values(cls, values: dict[str, object]) -> dict[str, float]:
        if not isinstance(values, dict) or not set(values).issubset(cls.VALID_FIELDS):
            raise ValueError("Campos de marcador inválidos.")
        normalized = {}
        for key, value in values.items():
            if isinstance(value, bool) or not isinstance(value, (int, float)):
                raise TypeError(f"O marcador '{key}' deve ser numérico.")
            numeric_value = float(value)
            if not math.isfinite(numeric_value) or numeric_value < 0:
                raise ValueError(f"O marcador '{key}' deve ser finito e não negativo.")
            normalized[key] = numeric_value
        start = normalized.get("start")
        end = normalized.get("end")
        if start is not None and end is not None and start > end:
            raise ValueError("O início da intro não pode ser posterior ao fim.")
        return normalized

    def _write(self, data: dict[str, object]) -> None:
        self.markers_file.parent.mkdir(parents=True, exist_ok=True)
        existing_mode = (
            self.markers_file.stat().st_mode & 0o777
            if self.markers_file.exists()
            else 0o600
        )
        temporary_path: Path | None = None
        try:
            with tempfile.NamedTemporaryFile(
                mode="w",
                encoding="utf-8",
                dir=self.markers_file.parent,
                prefix=f".{self.markers_file.name}.",
                delete=False,
            ) as temporary_file:
                temporary_path = Path(temporary_file.name)
                json.dump(data, temporary_file, ensure_ascii=False, indent=2)
                temporary_file.write("\n")
                temporary_file.flush()
                os.fsync(temporary_file.fileno())
            os.chmod(temporary_path, existing_mode)
            os.replace(temporary_path, self.markers_file)
            directory_fd = os.open(self.markers_file.parent, os.O_RDONLY)
            try:
                os.fsync(directory_fd)
            finally:
                os.close(directory_fd)
        finally:
            if temporary_path is not None and temporary_path.exists():
                temporary_path.unlink()
