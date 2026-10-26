# Design: Plex API Markers Integration

## High‑Level Architecture

```
+-------------------+          +---------------------------+
|  Plex Media Server|  <--->   |  plex‑autoplay container  |
|  (library, markers)|  API    |  - Flask (dashboard)      |
+-------------------+          |  - PlexManager            |
                               |  - PlexLibraryScanner     |
                               |  - MarkerGenerator        |
                               |  - SessionWatcher         |
                               +---------------------------+
```

### Components

| Component | Responsibility | Key Interfaces |
|-----------|----------------|----------------|
| **PlexManager** (infrastructure/plex/client.py) | Thin wrapper around `plexapi`. Provides `list_library_sections()`, `list_episodes(section_key)`, `get_episode(rating_key)`, `add_marker(episode, type, start, end)`, `remove_marker(episode, type)`. | Used by scanner, webhook handler, session watcher. |
| **PlexLibraryScanner** (application/plex_library_scanner.py) | Orchestrates a *scan job*: for each episode in a section, ensure intro/credits markers exist. Can be invoked via API (`POST /api/plex/scan`) or webhook (`POST /plex/webhook`). | Calls `PlexManager`, `MarkerGenerator`. |
| **MarkerGenerator** (application/marker_generator.py) | Given a media file path, returns `{'intro': {'start': ms, 'end': ms}, 'credits': {'start': ms}}`. Reuses existing detection logic (silence, logo, AniDB). | Pure function – no side effects. |
| **SessionWatcher** (infrastructure/plex/session_watcher.py) | Listens to Plex playback sessions. When a session starts, fetches the episode’s markers from Plex (or local cache) and issues `seekTo` when playback reaches marker boundaries. | Reads markers via `PlexManager.get_episode(...).markers`. |
| **Webhook Handler** (infrastructure/web/app.py – new route) | Verifies `X-Plex-Signature`, parses JSON payload, extracts `ratingKey` of added/updated episodes, enqueues a *single‑episode* scan job. | Returns 200 quickly; heavy work delegated to background worker. |
| **Background Worker** (optional, e.g., `rq` worker) | Executes `MarkerGenerator` + `PlexManager.add_marker` out of the request path. | Consumes jobs from a Redis queue (or in‑process `queue.Queue`). |

## Data Flow

### Full Section Scan (manual or scheduled)
1. Client calls `POST /api/plex/scan {section_key}`.
2. `PlexLibraryScanner.scan_section(section_key)`:
   * `episodes = PlexManager.list_episodes(section_key)`
   * For each episode:
     * If markers already present → continue.
     * `file_path = episode.file_path` (from `media[0].parts[0].file`).
     * `markers = MarkerGenerator.generate(file_path)`.
     * `PlexManager.add_marker(episode, 'intro', markers['intro']['start'], markers['intro']['end'])`.
     * `PlexManager.add_marker(episode, 'credits', markers['credits']['start'], markers['credits']['end'])`.
3. Respond with summary (processed, skipped, errors).

### Webhook‑Driven Incremental Scan
1. Plex sends `POST /plex/webhook` with `payload` containing `event = "library.new"` and `Metadata.ratingKey`.
2. Handler validates signature, extracts `ratingKey`.
3. Enqueue job `process_episode(ratingKey)`.
4. Worker loads episode via `PlexManager.get_episode(ratingKey)` and runs the same marker‑ensure logic as above.

### Playback Skip
1. `SessionWatcher` receives `Playing` event → gets `ratingKey`.
2. `episode = PlexManager.get_episode(ratingKey)`.
3. Parse `episode.markers` for `type == "intro"` and `"credits"`.
4. When playback position enters `[intro.start, intro.end)` → `player.seekTo(intro.end)`.
5. Same for credits.

## Configuration Changes

| Removed env vars | Reason |
|------------------|--------|
| `ANIME_MEDIA_DIR`, `ANIME_MEDIA_DIRS`, `SERIES_MEDIA_DIR`, `SERIES_MEDIA_DIRS`, `MEDIA_ROOT` | No longer needed – media paths come from Plex. |
| `CUSTOM_MARKERS_FILE` | Markers stored in Plex. |
| `ANIME_MARKERS_CACHE_FILE`, `ANIME_MARKERS_CACHE_TTL_HOURS` | Optional local cache only for performance. |

| New / Updated env vars |
|------------------------|
| `PLEX_URL` (required) |
| `PLEX_TOKEN` (required, admin) |
| `WEB_PORT` (unchanged) |
| `DASHBOARD_HTTPS`, `DASHBOARD_COOKIE_SECURE`, `DASHBOARD_TLS_HOSTS` (unchanged) |
| `SCAN_INTERVAL_MINUTES` (default `5`) – polling interval for fallback scan. |
| `WEBHOOK_SECRET` (optional) – shared secret to validate `X-Plex-Signature`. |
| `WORKER_BACKEND` (`sync`|`rq`) – choose background execution mode. |

## Error Handling & Observability
* All external calls wrapped with retries (tenacity) and structured logging (`logger.exception`).
* Scan job returns a JSON summary; failures are recorded per‑episode but do not abort the whole scan.
* Prometheus‑style metrics (optional) – `plex_autoplay_scan_duration_seconds`, `plex_autoplay_markers_created_total`, `plex_autoplay_webhook_received_total`.

## Testing Strategy
* **Unit tests** – mock `plexapi` objects for `PlexManager` methods; test `MarkerGenerator` with sample media files.
* **Integration test** – spin up a test Plex instance (Docker `linuxserver/plex`) with a small library; run a full scan via API; verify markers appear in Plex Web UI.
* **Webhook test** – POST a crafted payload to `/plex/webhook`; assert job enqueued and markers created.
* **Regression** – ensure existing dashboard pages (overview, library manager, config) still render and auth works.

## Migration Path
1. Deploy new code alongside old – both `DirectoryMonitor` and new scanner can coexist.
2. Run a one‑time full scan via `POST /api/plex/scan` to populate Plex markers.
3. Verify skip works on a few clients.
4. Disable `DirectoryMonitor` (set `ENABLE_FS_WATCHER=false` env) and remove filesystem‑related config.
5. Remove old code paths in a follow‑up PR.

## Diagram (Mermaid)

```mermaid
flowchart TD
    A[Client UI] -->|GET /api/plex/libraries| B(Flask)
    B --> C[PlexManager.list_library_sections]
    C --> D[Plex Media Server]
    D --> C
    C --> B
    B --> A

    A -->|POST /api/plex/scan| B
    B --> E[PlexLibraryScanner.scan_section]
    E --> F[PlexManager.list_episodes]
    F --> D
    D --> F
    F --> E
    E --> G[MarkerGenerator.generate]
    G --> H[PlexManager.add_marker]
    H --> D

    D -->|Webhook library.new| I[/plex/webhook]
    I --> J[Validate signature]
    J --> K[Enqueue process_episode]
    K --> L[Worker]
    L --> M[PlexManager.get_episode]
    M --> D
    D --> M
    M --> G
    G --> H

    D -->|Session event| N[SessionWatcher]
    N --> O[PlexManager.get_episode]
    O --> D
    D --> O
    O --> P[player.seekTo]
```