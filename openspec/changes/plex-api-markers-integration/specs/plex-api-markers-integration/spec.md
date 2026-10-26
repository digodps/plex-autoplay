# Spec: Plex API Markers Integration

## 1. Overview
This spec defines the contract for integrating **Plex native markers** (intro / credits) as the single source of truth, replacing the local filesystem watcher + `markers.json` approach.

## 2. Data Model

### 2.1 Library Section (returned by `GET /api/plex/libraries`)
```json
{
  "key": "1",
  "title": "TV Shows",
  "type": "show",
  "thumb": "/library/metadata/1/thumb/12345",
  "thumb_url": "http://plex:32400/library/metadata/1/thumb/12345?X-Plex-Token=xxxx"
}
```

### 2.2 Episode Summary (returned by `PlexManager.list_episodes`)
```json
{
  "rating_key": "12345",
  "title": "Pilot",
  "season": 1,
  "episode": 1,
  "file_path": "/data/tv/Show/Season 01/Show - S01E01.mkv",
  "thumb_url": "http://plex:32400/library/metadata/12345/thumb/67890?X-Plex-Token=xxxx"
}
```

### 2.3 Marker Payload (used by `add_marker`)
| Field | Type | Description |
|-------|------|-------------|
| `type` | `"intro"` \| `"credits"` | Marker kind |
| `start` | integer (ms) | Inclusive start offset |
| `end` | integer (ms) | Exclusive end offset |

## 3. API Contracts

### 3.1 `POST /api/plex/scan`
**Request**
```json
{ "section_key": "1" }
```
**Response 202**
```json
{ "job_id": "scan-abc123", "status": "queued" }
```

### 3.2 `GET /api/plex/scan/<job_id>`
**Response 200**
```json
{
  "job_id": "scan-abc123",
  "status": "completed",   // queued | running | completed | failed
  "processed": 142,
  "skipped": 10,
  "errors": [
    { "rating_key": "99999", "error": "ffmpeg timeout" }
  ]
}
```

### 3.3 `POST /plex/webhook`
**Headers**
* `X-Plex-Signature: sha256=<hmac>` – verified with `WEBHOOK_SECRET`.
**Body** (Plex webhook JSON). Handler extracts `event` and `Metadata.ratingKey` for `library.new` / `library.updated`.

**Response** `200 OK` (empty) – must return quickly.

## 4. Component Interfaces (Python)

```python
# plex_autoplay.infrastructure.plex.client.PlexManager
class PlexManager:
    def list_library_sections(self) -> List[dict]: ...
    def list_episodes(self, section_key: str) -> List[dict]: ...
    def get_episode(self, rating_key: str) -> Any: ...          # plexapi Episode
    def add_marker(self, episode: Any, mtype: str, start_ms: int, end_ms: int) -> None: ...
    def remove_marker(self, episode: Any, mtype: str) -> None: ...

# plex_autoplay.application.marker_generator.MarkerGenerator
class MarkerGenerator:
    def generate(self, file_path: str) -> dict:
        """
        Returns {"intro": {"start": int, "end": int}, "credits": {"start": int}}
        Missing keys => marker not generated.
        """
        ...

# plex_autoplay.application.plex_library_scanner.PlexLibraryScanner
class PlexLibraryScanner:
    def __init__(self, plex: PlexManager, generator: MarkerGenerator): ...
    def scan_section(self, section_key: str) -> ScanResult: ...
    def process_episode(self, rating_key: str) -> EpisodeResult: ...

@dataclass
class ScanResult:
    processed: int
    skipped: int
    errors: List[Dict[str, str]]

@dataclass
class EpisodeResult:
    rating_key: str
    status: Literal["created", "updated", "skipped", "error"]
    error: Optional[str] = None
```

## 5. Configuration Schema (env / `config/app-config.json`)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `plex_url` | string | `"http://localhost:32400"` | Base URL of PMS |
| `plex_token` | string | *required* | Admin token |
| `scan_interval_minutes` | int | `5` | Polling fallback interval |
| `webhook_secret` | string | `""` | HMAC secret for `X-Plex-Signature` |
| `worker_backend` | `"sync" \| "rq"` | `"sync"` | Background job executor |
| `redis_url` | string | `""` | Required if `worker_backend == "rq"` |
| `enable_fs_watcher` | bool | `false` | Legacy filesystem monitor |

## 6. Error Codes
| HTTP | Code | Meaning |
|------|------|---------|
| 400 | `INVALID_PAYLOAD` | Missing/invalid JSON fields |
| 401 | `UNAUTHENTICATED` | No valid admin session |
| 403 | `WEBHOOK_SIGNATURE_INVALID` | HMAC verification failed |
| 500 | `PLEX_CONNECTION_FAILED` | Cannot reach PMS |
| 500 | `MARKER_GENERATION_FAILED` | ffmpeg / detection error |
| 503 | `SCANNER_UNAVAILABLE` | Scanner service not initialized |

## 7. Security Considerations
* `plex_token` must have **Owner/Admin** role – validated at startup by attempting a harmless `add_marker`/`remove_marker` on a test episode.
* Webhook endpoint validates HMAC (`X-Plex-Signature`) using `webhook_secret`; rejects mismatched signatures with 403.
* All mutating endpoints (`/api/plex/scan`, `/plex/webhook`) require authenticated admin session (CSRF protected).

## 8. Observability
* Structured logs: `plex_autoplay.scanner`, `plex_autoplay.webhook`, `plex_autoplay.marker_generator`.
* Metrics (Prometheus exposition if enabled):
  * `plex_autoplay_scan_duration_seconds_bucket`
  * `plex_autoplay_markers_created_total{type="intro|credits"}`
  * `plex_autoplay_webhook_events_total{event="library.new|library.updated"}`
  * `plex_autoplay_errors_total{component="scanner|webhook|generator"}`

## 9. Migration Notes
1. Deploy new version with `ENABLE_FS_WATCHER=false`.
2. Run `POST /api/plex/scan` for each library section.
3. Verify markers in Plex Web UI → “Skip Intro” appears.
4. Remove old env vars (`*_MEDIA_DIR*`, `MEDIA_ROOT`, `CUSTOM_MARKERS_FILE`).
5. Delete `markers.json` and `metadata-cache.json` from config volume.

## 10. Versioning
* This spec corresponds to **v0.4.0** (minor bump) of `plex-autoplay`.
* Future breaking changes (e.g., marker schema changes) will bump major version.

---  
*End of spec.*