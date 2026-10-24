"""Pure playback rules independent of Plex and infrastructure."""

from typing import Optional, Tuple


def should_skip_intro(view_offset: int, intro_markers: Optional[Tuple[int, int]]) -> bool:
    if intro_markers is None:
        return False
    intro_start, intro_end = intro_markers
    return intro_start <= view_offset < (intro_end - 1000)


def should_trigger_next_episode(
    view_offset: int,
    duration: int,
    credits_start: Optional[int],
    threshold_seconds: int,
) -> bool:
    trigger_offset = (
        credits_start
        if isinstance(credits_start, (int, float))
        else duration - (threshold_seconds * 1000)
    )
    return view_offset >= trigger_offset and (duration - view_offset) <= threshold_seconds * 2000
