"""Current process resource metrics for the dashboard."""

from __future__ import annotations

import os
import threading
import time
from pathlib import Path
from typing import Any, Dict, Optional


class ProcessMetricsSampler:
    def __init__(self) -> None:
        self._lock = threading.Lock()
        self._last_wall_time = time.monotonic()
        self._last_cpu_time = time.process_time()
        self._last_cpu_percent: Optional[float] = None

    def sample(self) -> Dict[str, Any]:
        with self._lock:
            wall_time = time.monotonic()
            cpu_time = time.process_time()
            elapsed = wall_time - self._last_wall_time
            if elapsed >= 0.5:
                self._last_cpu_percent = round(
                    max(0.0, (cpu_time - self._last_cpu_time) / elapsed * 100),
                    1,
                )
                self._last_wall_time = wall_time
                self._last_cpu_time = cpu_time

            memory_fields = Path("/proc/self/statm").read_text(encoding="ascii").split()
            if len(memory_fields) < 2:
                raise ValueError("O sistema não retornou o uso atual de memória do processo.")
            resident_pages = int(memory_fields[1])
            page_size = os.sysconf("SC_PAGE_SIZE")

            return {
                "cpu_percent": self._last_cpu_percent,
                "memory_bytes": resident_pages * page_size,
                "sampled_at": time.time(),
            }
