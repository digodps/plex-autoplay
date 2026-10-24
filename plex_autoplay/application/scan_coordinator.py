"""Single-flight asynchronous coordination for library scans."""

from __future__ import annotations

import copy
from dataclasses import dataclass
from datetime import datetime, timezone
import logging
import threading
from typing import Any, Callable, Dict, Optional, Protocol

logger = logging.getLogger("plex-autoplay.scan")


@dataclass(frozen=True)
class ScanSnapshot:
    status: str
    started_at: Optional[str]
    completed_at: Optional[str]
    result: Optional[Dict[str, Any]]
    error: Optional[str]
    progress: Optional[Dict[str, Any]]

    def to_dict(self) -> Dict[str, Any]:
        return {
            "status": self.status,
            "started_at": self.started_at,
            "completed_at": self.completed_at,
            "result": self.result,
            "error": self.error,
            "progress": copy.deepcopy(self.progress),
        }


class ScanReportStore(Protocol):
    def load(self) -> Optional[Dict[str, Any]]: ...

    def save(self, report: Dict[str, Any]) -> None: ...


class ScanCoordinator:
    ACTIVE_STATUSES = {"queued", "running"}

    def __init__(
        self,
        scan_callback: Callable[[Callable[[Dict[str, Any]], None]], Dict[str, Any]],
        report_store: Optional[ScanReportStore] = None,
    ):
        self._scan_callback = scan_callback
        self._report_store = report_store
        self._lock = threading.Lock()
        self._state = ScanSnapshot(
            status="idle",
            started_at=None,
            completed_at=None,
            result=None,
            error=None,
            progress=None,
        )
        if report_store is not None:
            previous_report = report_store.load()
            if previous_report is not None:
                status = previous_report.get("status", "failed")
                if status in self.ACTIVE_STATUSES:
                    status = "failed"
                    previous_report["error"] = "Varredura interrompida pelo encerramento do processo."
                if status not in {"completed", "failed"}:
                    status = "failed"
                result = previous_report.get("result")
                self._state = ScanSnapshot(
                    status=status,
                    started_at=previous_report.get("started_at"),
                    completed_at=previous_report.get("completed_at"),
                    result=result if isinstance(result, dict) else None,
                    error=previous_report.get("error"),
                    progress=previous_report.get("progress"),
                )
        self._worker: Optional[threading.Thread] = None

    def start(self) -> bool:
        with self._lock:
            if self._state.status in self.ACTIVE_STATUSES:
                return False
            previous_state = self._state
            self._state = ScanSnapshot(
                status="queued",
                started_at=datetime.now(timezone.utc).isoformat(),
                completed_at=None,
                result=None,
                error=None,
                progress={
                    "stage": "queued",
                    "message": "Varredura aguardando execução.",
                    "library": None,
                    "directories_scanned": 0,
                    "files_scanned": 0,
                    "completed_libraries": 0,
                    "total_libraries": 0,
                    "events": [],
                },
            )
            try:
                self._persist(self._state)
            except Exception:
                self._state = previous_state
                raise
            self._worker = threading.Thread(target=self._run, name="tvshow-library-scan", daemon=True)
            try:
                self._worker.start()
            except RuntimeError as error:
                self._state = ScanSnapshot(
                    status="failed",
                    started_at=self._state.started_at,
                    completed_at=datetime.now(timezone.utc).isoformat(),
                    result=None,
                    error=f"Não foi possível iniciar worker da varredura: {error}",
                    progress={"stage": "failed", "message": "Não foi possível iniciar a varredura."},
                )
                self._persist(self._state)
                raise
            return True

    def snapshot(self) -> ScanSnapshot:
        with self._lock:
            return self._state

    def update_result(self, updater: Callable[[Dict[str, Any]], Optional[Dict[str, Any]]]) -> bool:
        with self._lock:
            current = self._state
            if current.result is None:
                return False
            result = updater(copy.deepcopy(current.result))
            if result is None:
                return False
            updated = ScanSnapshot(
                status=current.status,
                started_at=current.started_at,
                completed_at=current.completed_at,
                result=result,
                error=current.error,
                progress=copy.deepcopy(current.progress),
            )
            self._state = updated
            try:
                self._persist(updated)
            except Exception:
                self._state = current
                raise
            return True

    def wait(self, timeout: Optional[float] = None) -> ScanSnapshot:
        with self._lock:
            worker = self._worker
        if worker is not None:
            worker.join(timeout)
        return self.snapshot()

    def _run(self) -> None:
        with self._lock:
            current = self._state
            running = ScanSnapshot(
                status="running",
                started_at=current.started_at,
                completed_at=None,
                result=None,
                error=None,
                progress=copy.deepcopy(current.progress),
            )
            self._state = running
        try:
            self._persist(running)
        except Exception as error:
            self._set_failed(f"Falha ao persistir o estado da varredura: {error}")
            return
        try:
            result = self._scan_callback(self.report_progress)
        except Exception as error:
            self._set_failed(str(error))
            return

        with self._lock:
            current = self._state
            completed = ScanSnapshot(
                status="completed",
                started_at=current.started_at,
                completed_at=datetime.now(timezone.utc).isoformat(),
                result=result,
                error=None,
                progress={
                    **(current.progress or {}),
                    "stage": "completed",
                    "message": "Varredura concluída.",
                },
            )
            self._state = completed
        try:
            self._persist(completed)
        except Exception as error:
            self._set_failed(f"Falha ao persistir o relatório da varredura: {error}")

    def _set_failed(self, message: str) -> None:
        with self._lock:
            current = self._state
            failed = ScanSnapshot(
                status="failed",
                started_at=current.started_at,
                completed_at=datetime.now(timezone.utc).isoformat(),
                result=None,
                error=message,
                progress={
                    **(current.progress or {}),
                    "stage": "failed",
                    "message": message,
                },
            )
            self._state = failed
        try:
            self._persist(failed)
        except Exception:
            logger.exception("Falha ao persistir estado de erro da varredura.")

    def _persist(self, snapshot: ScanSnapshot) -> None:
        if self._report_store is not None:
            self._report_store.save(snapshot.to_dict())

    def report_progress(self, progress: Dict[str, Any]) -> None:
        with self._lock:
            current = self._state
            if current.status not in self.ACTIVE_STATUSES:
                return
            updated_progress = copy.deepcopy(progress)
            events = list((current.progress or {}).get("events", []))
            event = {
                "stage": updated_progress.get("stage", "scan"),
                "message": updated_progress.get("message", "Varredura em andamento."),
                "library": updated_progress.get("library"),
                "at": datetime.now(timezone.utc).isoformat(),
            }
            if not events or any(events[-1].get(key) != value for key, value in event.items() if key != "at"):
                events.append(event)
            updated_progress["events"] = events[-12:]
            self._state = ScanSnapshot(
                status=current.status,
                started_at=current.started_at,
                completed_at=current.completed_at,
                result=current.result,
                error=current.error,
                progress=updated_progress,
            )
