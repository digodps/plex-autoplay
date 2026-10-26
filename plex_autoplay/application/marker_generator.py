"""Marker generation utilities – detect intro/credits timestamps from media file
and (optionally) emit a Plex‑compatible side‑car XML (Local Media Assets)."""

from __future__ import annotations

import json
import logging
import re
import subprocess
import xml.etree.ElementTree as ET
from dataclasses import dataclass

logger = logging.getLogger("plex-autoplay.marker_generator")


@dataclass
class MarkerResult:
    intro_start: int | None = None  # ms
    intro_end: int | None = None
    credits_start: int | None = None
    credits_end: int | None = None


def _run_ffprobe(file_path: str) -> dict:
    """Run ffprobe to get format and stream info as JSON."""
    cmd = [
        "ffprobe",
        "-v",
        "error",
        "-print_format",
        "json",
        "-show_format",
        "-show_streams",
        file_path,
    ]
    try:
        result = subprocess.run(
            cmd, capture_output=True, text=True, check=True, timeout=30
        )
        return json.loads(result.stdout)
    except Exception as exc:
        logger.exception("ffprobe failed for %s", file_path)
        raise RuntimeError(f"ffprobe error: {exc}") from exc


def _get_duration(file_path: str) -> float:
    """Return total duration of media in seconds."""
    info = _run_ffprobe(file_path)
    fmt = info.get("format", {})
    dur = fmt.get("duration")
    if dur:
        return float(dur)
    for s in info.get("streams", []):
        if s.get("codec_type") == "video" and s.get("duration"):
            return float(s["duration"])
    return 0.0


def _detect_silence_intervals(
    file_path: str, start_sec: float, duration_sec: float
) -> list[tuple[float, float]]:
    """
    Run ffmpeg silencedetect on a slice of the file.
    Returns list of (silence_start, silence_end) in seconds (relative to file start).
    """
    cmd = [
        "ffmpeg",
        "-hide_banner",
        "-vn",
        "-ss",
        str(start_sec),
        "-t",
        str(duration_sec),
        "-i",
        file_path,
        "-af",
        "silencedetect=n=-50dB:d=1.0",
        "-f",
        "null",
        "-",
    ]
    try:
        proc = subprocess.run(
            cmd, check=False, capture_output=True, text=True, timeout=120
        )
    except Exception:
        logger.exception("silence detection failed for %s", file_path)
        return []

    starts = []
    ends = []
    for line in proc.stderr.splitlines():
        if "silence_start" in line:
            m = re.search(r"silence_start:\s*([0-9.]+)", line)
            if m:
                starts.append(float(m.group(1)) + start_sec)
        if "silence_end" in line:
            m = re.search(r"silence_end:\s*([0-9.]+)", line)
            if m:
                ends.append(float(m.group(1)) + start_sec)

    intervals = []
    for s, e in zip(starts, ends):
        if e > s:
            intervals.append((s, e))
    return intervals


def generate_markers(file_path: str) -> MarkerResult:
    """
    Main entry point. Attempts to detect intro and credits markers using silence detection.
    Heuristics:
      - Intro: first silence >= 2s occurring within first 5 minutes.
      - Credits: last silence >= 2s occurring within last 10% of duration.
    Returns MarkerResult with times in milliseconds (or None if not found).
    """
    if not file_path:
        return MarkerResult()

    duration = _get_duration(file_path)
    if duration <= 0:
        logger.warning("Could not determine duration for %s", file_path)
        return MarkerResult()

    # ---- Intro detection (first 5 minutes) ----
    intro_window = min(300.0, duration * 0.2)
    intro_intervals = _detect_silence_intervals(file_path, 0.0, intro_window)
    intro_start = intro_end = None
    for s, e in intro_intervals:
        if (e - s) >= 2.0:
            intro_start = s
            intro_end = e
            break

    # ---- Credits detection (last 10% of video) ----
    tail_start = max(0.0, duration * 0.9)
    tail_dur = duration - tail_start
    credits_intervals = _detect_silence_intervals(file_path, tail_start, tail_dur)
    credits_start = credits_end = None
    for s, e in reversed(credits_intervals):
        if (e - s) >= 2.0:
            credits_start = s
            credits_end = e
            break

    def ms(v: float | None) -> int | None:
        return int(v * 1000) if v is not None else None

    return MarkerResult(
        intro_start=ms(intro_start),
        intro_end=ms(intro_end),
        credits_start=ms(credits_start),
        credits_end=ms(credits_end),
    )


def _build_plex_xml(res: MarkerResult, media_filename: str) -> str:
    """
    Build a Plex‑compatible side‑car XML (Local Media Assets) for the given media file.
    The XML follows the schema described in Plex docs:
    https://support.plex.tv/articles/201273953-local-media-assets/
    """
    media_el = ET.Element("Media")
    part_el = ET.SubElement(media_el, "Part", file=media_filename)

    if res.intro_start is not None and res.intro_end is not None:
        ET.SubElement(
            part_el,
            "Marker",
            type="intro",
            start=str(res.intro_start),
            end=str(res.intro_end),
        )
    if res.credits_start is not None and res.credits_end is not None:
        ET.SubElement(
            part_el,
            "Marker",
            type="credits",
            start=str(res.credits_start),
            end=str(res.credits_end),
        )

    # Pretty‑print
    ET.indent(media_el, space="  ")
    return ET.tostring(media_el, encoding="unicode", xml_declaration=True)


def generate(file_path: str) -> dict[str, dict[str, int]]:
    """Backwards‑compatible helper – still returns the plain dict used by older callers."""
    res = generate_markers(file_path)
    out = {}
    if res.intro_start is not None and res.intro_end is not None:
        out["intro"] = {"start": res.intro_start, "end": res.intro_end}
    if res.credits_start is not None and res.credits_end is not None:
        out["credits"] = {"start": res.credits_start, "end": res.credits_end}
    return out


def generate_with_xml(file_path: str) -> tuple[dict[str, dict[str, int]], str]:
    """
    New public API used by the scanner.
    Returns a tuple (marker_dict, xml_string) where xml_string is ready to be written as
    `<video-file>.xml` next to the media file.
    """
    res = generate_markers(file_path)
    marker_dict = {}
    if res.intro_start is not None and res.intro_end is not None:
        marker_dict["intro"] = {"start": res.intro_start, "end": res.intro_end}
    if res.credits_start is not None and res.credits_end is not None:
        marker_dict["credits"] = {"start": res.credits_start, "end": res.credits_end}

    media_filename = file_path.split("/")[-1]  # only the filename, not the full path
    xml_str = _build_plex_xml(res, media_filename)
    return marker_dict, xml_str
