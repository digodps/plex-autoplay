# Proposal: Plex API Markers Integration

## Summary
Replace the current filesystem‑based discovery + local `markers.json` approach with a **Plex‑centric** workflow:
* Query the Plex Media Server (via `plexapi`) for library sections and episodes.
* Generate/ensure **intro** and **credits** markers directly on the Plex `Episode` objects (`addMarker` / `removeMarker`).
* Optional webhook (`library.new`) for near‑real‑time processing of newly added episodes.
* Remove / deprecate the directory watcher (`DirectoryMonitor`) and the local `markers.json` cache.

## Motivation
* Eliminates the need to mount media directories inside the container and keep them in sync.
* Markers become first‑class Plex metadata – all official Plex clients instantly show “Skip Intro” buttons.
* Simpler deployment (only `PLEX_URL` + `PLEX_TOKEN` required).
* Guarantees consistency when files are moved/renamed inside Plex (markers follow `ratingKey`).

## Scope
* New `PlexManager.list_library_sections()` and `list_episodes(section_key)`.
* New application service `PlexLibraryScanner` (polling + optional webhook handler).
* Refactor marker generation (`AnimeMarkerService`, `ManualMarkerService`) to write markers to Plex instead of JSON.
* New API endpoints:
  * `GET /api/plex/libraries` (already added) – list sections with thumbnails.
  * `POST /api/plex/scan` – trigger a full or incremental scan of a section.
  * `POST /plex/webhook` – receive `library.new` / `library.updated` events (secured with `X-Plex-Signature`).
* Update `SessionWatcher` / `PlaybackService` to read markers from Plex (or a tiny local cache) instead of `markers.json`.
* Remove `DirectoryMonitor`, `LibraryRepository` (FS based) and related config vars (`*_MEDIA_DIR*`, `MEDIA_ROOT`).
* Update UI – Config page drops filesystem paths; Library Manager shows Plex libraries as cards (already done).

## Acceptance Criteria
1. Given a configured Plex URL/Token, the dashboard can list all library sections with thumbnails.
2. Running a scan (manual or webhook) creates/updates intro & credits markers on every episode in the selected section.
3. Playing an episode on any Plex client shows the native “Skip Intro” / “Skip Credits” buttons at the correct timestamps.
4. No local `markers.json` is written; the filesystem watcher is stopped.
5. All existing unit/integration tests pass; new tests cover the scanner and webhook path.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Plex token lacks admin rights → `addMarker` fails | Document requirement; validate on startup (try a harmless `addMarker`/`removeMarker` on a test episode). |
| Heavy ffmpeg analysis on many new episodes | Run marker generation in a background worker queue (RQ/Redis or simple threading) so the HTTP request returns quickly. |
| Webhook not reachable (NAT, no public URL) | Keep polling fallback (default 5 min) – webhook is optional. |
| Older PMS (<1.22) without marker API | Detect version at startup; if unsupported, fall back to legacy JSON mode and log a warning. |