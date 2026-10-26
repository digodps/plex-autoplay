"""Current process resource metrics for the dashboard."""

from __future__ import annotations

import os
import threading
import time
from pathlib import Path
from typing import Any


class ProcessMetricsSampler:
    def __init__(self) -> None:
        self._lock = threading.Lock()
        self._last_wall_time = time.monotonic()
        self._last_cpu_time = time.process_time()
        self._last_cpu_percent: float | None = None
        self._last_network_sample: tuple[float, int, int] | None = None
        self._download_bytes_per_second: float | None = None
        self._upload_bytes_per_second: float | None = None

    def sample(self) -> dict[str, Any]:
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
            received_bytes, sent_bytes, interface_count = self._read_network_counters()
            if self._last_network_sample is not None:
                previous_time, previous_received, previous_sent = self._last_network_sample
                network_elapsed = wall_time - previous_time
                if network_elapsed >= 0.5:
                    if received_bytes >= previous_received:
                        self._download_bytes_per_second = round(
                            (received_bytes - previous_received) / network_elapsed,
                            1,
                        )
                    else:
                        self._download_bytes_per_second = None
                    if sent_bytes >= previous_sent:
                        self._upload_bytes_per_second = round(
                            (sent_bytes - previous_sent) / network_elapsed,
                            1,
                        )
                    else:
                        self._upload_bytes_per_second = None
                    self._last_network_sample = (wall_time, received_bytes, sent_bytes)
            else:
                self._last_network_sample = (wall_time, received_bytes, sent_bytes)

            return {
                "cpu_percent": self._last_cpu_percent,
                "memory_bytes": resident_pages * page_size,
                "network_download_bytes_per_second": self._download_bytes_per_second,
                "network_upload_bytes_per_second": self._upload_bytes_per_second,
                "network_received_bytes_total": received_bytes,
                "network_sent_bytes_total": sent_bytes,
                "network_interface_count": interface_count,
                "sampled_at": time.time(),
            }

    @staticmethod
    def _read_network_counters() -> tuple[int, int, int]:
        received_total = 0
        sent_total = 0
        interface_count = 0
        network_stats = Path("/proc/net/dev").read_text(encoding="ascii")
        for line in network_stats.splitlines()[2:]:
            interface, separator, counters = line.partition(":")
            if not separator:
                continue
            interface_name = interface.strip()
            if interface_name == "lo":
                continue
            values = counters.split()
            if len(values) < 9:
                raise ValueError(f"Contadores de rede inválidos para a interface {interface_name}.")
            received_total += int(values[0])
            sent_total += int(values[8])
            interface_count += 1
        return received_total, sent_total, interface_count
