"""Environment-based application configuration."""

import os
from dataclasses import dataclass, field
from typing import List

try:
    from dotenv import load_dotenv

    load_dotenv()
except ImportError:
    pass


def _str_to_bool(val: str, default: bool = False) -> bool:
    if val is None:
        return default
    return str(val).strip().lower() in ("true", "1", "yes", "on", "sim", "s")


def _str_to_list(val: str) -> List[str]:
    if not val:
        return []
    return [item.strip() for item in val.split(",") if item.strip()]


@dataclass
class Settings:
    plex_url: str = os.getenv("PLEX_URL", "http://localhost:32400").rstrip("/")
    plex_token: str = os.getenv("PLEX_TOKEN", "")
    auto_skip_intro: bool = _str_to_bool(os.getenv("AUTO_SKIP_INTRO", "true"), default=True)
    auto_next_episode: bool = _str_to_bool(os.getenv("AUTO_NEXT_EPISODE", "true"), default=True)
    next_episode_threshold_seconds: int = int(os.getenv("NEXT_EPISODE_THRESHOLD_SECONDS", "15"))
    poll_interval_seconds: int = int(os.getenv("POLL_INTERVAL_SECONDS", "2"))
    allowed_client_ids: List[str] = field(default_factory=lambda: _str_to_list(os.getenv("ALLOWED_CLIENT_IDS", "")))
    allowed_usernames: List[str] = field(default_factory=lambda: _str_to_list(os.getenv("ALLOWED_USERNAMES", "")))
    custom_markers_file: str = os.getenv("CUSTOM_MARKERS_FILE", "markers.json")
    default_intro_skip_seconds: int = int(os.getenv("DEFAULT_INTRO_SKIP_SECONDS", "0"))
    log_level: str = os.getenv("LOG_LEVEL", "INFO").upper()

    def validate(self) -> None:
        if not self.plex_url:
            raise ValueError("PLEX_URL não pode estar vazio.")
        if not self.plex_token or self.plex_token == "seu_token_aqui":
            raise ValueError(
                "PLEX_TOKEN não configurado. Defina a variável PLEX_TOKEN no seu arquivo .env com seu token do Plex."
            )
        if self.next_episode_threshold_seconds < 1:
            raise ValueError("NEXT_EPISODE_THRESHOLD_SECONDS deve ser de pelo menos 1 segundo.")
        if self.poll_interval_seconds < 1:
            raise ValueError("POLL_INTERVAL_SECONDS deve ser de pelo menos 1 segundo.")


config = Settings()
