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
    # Pending actions with delay
    pending_skip_intro: bool = False
    pending_skip_intro_at: float = 0.0
    pending_next_episode: bool = False
    pending_next_episode_at: float = 0.0
