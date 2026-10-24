"""Domain models used to track playback."""

from dataclasses import dataclass


@dataclass
class SessionState:
    """Tracks actions already taken during a playback session."""

    session_key: str
    rating_key: str
    intro_skipped: bool = False
    next_triggered: bool = False
    last_action_timestamp: float = 0.0
