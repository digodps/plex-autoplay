# Tasks: Plex API Markers Integration

## 1. Infrastructure – PlexManager extensions
- [ ] Add `list_library_sections() -> List[dict]` (title, key, type, thumb_url).
- [ ] Add `list_episodes(section_key: str) -> List[dict]` with fields: `rating_key, title, season, episode, file_path, thumb_url`.
- [ ] Add `get_episode(rating_key: str) -> Episode` (raw plexapi object).
- [ ] Add `add_marker(episode, marker_type: str, start_ms: int, end_ms: int) -> None`.
- [ ] Add `remove_marker(episode, marker_type: str) -> None`.
- [ ] Unit tests with mocked `plexapi.server.PlexServer`.

## 2. Application – MarkerGenerator
- [ ] Create `plex_autoplay/application/marker_generator.py`.
- [ ] Implement `generate(file_path: str) -> dict` returning `{'intro': {'start': ms, 'end': ms}, 'credits': {'start': ms}}`.
- [ ] Reuse existing detection logic from `AnimeMarkerService` / `ManualMarkerService` (silence detection, logo detection, AniDB lookup).
- [ ] Add CLI helper `python -m plex_autoplay.application.marker_generator <file>` for manual testing.
- [ ] Unit tests with sample media files (small fixtures).

## 3. Application – PlexLibraryScanner
- [ ] Create `plex_autoplay/application/plex_library_scanner.py`.
- [ ] Method `scan_section(section_key: str) -> ScanResult` (dataclass with processed, skipped, errors).
- [ ] Method `process_episode(rating_key: str) -> EpisodeResult`.
- [ ] Integrate `MarkerGenerator` and `PlexManager.add_marker`.
- [ ] Add retry/backoff for transient Plex errors.
- [ ] Write integration test against a test Plex instance (Docker compose fixture).

## 4. Background Worker (optional but recommended)
- [ ] Choose backend: `sync` (threading.Queue) default; optional `rq` if `REDIS_URL` provided.
- [ ] Implement `worker.py` that consumes `process_episode` jobs.
- [ ] Ensure Flask app can enqueue jobs via `current_app.worker.enqueue(...)`.

## 5. Web API – New Routes (in `infrastructure/web/app.py`)
- [ ] `POST /api/plex/scan` – body `{ "section_key": "1" }` → triggers `PlexLibraryScanner.scan_section` (async via worker) → returns job id.
- [ ] `GET /api/plex/scan/<job_id>` – poll job status/result.
- [ ] `POST /plex/webhook` – verify `X-Plex-Signature` (HMAC SHA256 with `WEBHOOK_SECRET`), parse JSON, enqueue `process_episode` for each `ratingKey` in `library.new` / `library.updated`.
- [ ] Add OpenAPI / Swagger docs (optional).

## 6. SessionWatcher Adaptation
- [ ] Modify `SessionWatcher` / `PlaybackService` to fetch markers from `episode.markers` (Plex) instead of local `markers.json`.
- [ ] Keep a tiny in‑memory cache `ratingKey -> {intro, credits}` with TTL (e.g., 5 min) to avoid hammering Plex on every event.
- [ ] Fallback: if marker not present, log warning and skip auto‑seek.

## 7. Configuration & Cleanup
- [ ] Update `plex_autoplay/config.py`:
  - Remove `*_MEDIA_DIR*`, `MEDIA_ROOT`, `CUSTOM_MARKERS_FILE`, `ANIME_MARKERS_CACHE_*`.
  - Add `SCAN_INTERVAL_MINUTES`, `WEBHOOK_SECRET`, `WORKER_BACKEND`.
- [ ] Adjust `ConfigStore.DEFAULT_CONFIG` accordingly.
- [ ] Update `config.html` template – hide removed fields, show new ones.
- [ ] Deprecate `DirectoryMonitor` and `LibraryRepository` (FS) – add feature flag `ENABLE_FS_WATCHER` (default `false`).

## 8. UI – Library Manager (already shows Plex library cards)
- [ ] Ensure cards use `thumb_url` from `plex_libraries` passed by view.
- [ ] Add “Scan this library” button per card → calls `POST /api/plex/scan`.

## 9. Documentation
- [ ] Update `README.md` with new architecture overview, required Plex token permissions, webhook setup instructions.
- [ ] Add “Migration from filesystem watcher” section.

## 10. Testing & CI
- [ ] Extend GitHub Actions workflow: spin up test Plex (docker), run full scan, assert markers exist via API.
- [ ] Add lint / type‑check for new modules.
- [ ] Ensure all existing tests still pass.

## 11. Release Checklist
- [ ] Bump version (semver minor).
- [ ] Changelog entry.
- [ ] Docker image builds with new deps (`plexapi`, `rq` optional).
- [ ] Verify `docker-compose.yml` includes `REDIS_URL` optional service.