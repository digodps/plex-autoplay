# Tasks

## 1. Plex Library Discovery Service

- [ ] 1.1 Create `LibraryDiscoveryService` with method `DiscoverLibrariesAsync()` calling Plex API `GET /library/sections` with valid token. Verify: unit test mocks Plex response returns list of libraries with key, title, type, agent.
- [ ] 1.2 Implement filtering for `type="show"` only. Verify: unit test ensures only show-type libraries returned.
- [ ] 1.3 Persist discovered series libraries to local database (rating key, title, type, agent). Verify: integration test with test DB confirms insert/update on discover.
- [ ] 1.4 Handle Plex server unreachable: throw/capture specific exception bubbled to caller. Verify: unit test simulates HTTP failure returns error "servidor Plex indisponível".

## 2. Episode Mapping Service

- [ ] 2.1 Create `EpisodeMappingService` with method `GetEpisodesAsync(sectionKey)` calling Plex API `/library/sections/{sectionKey}/all?type=4`. Verify: unit test mocks Plex episode list response.
- [ ] 2.2 Implement `MEDIA_ROOTS` resolution: for each episode `MediaPart.file`, find first existing path under configured roots. Return `local_path` or null. Verify: unit test with multiple roots + missing files returns correct local_path.
- [ ] 2.3 Build episode DTO: rating key, title, season, episode number, `local_path`, `thumb` as `/api/images/{Thumb}` (or null). Verify: unit test confirms DTO shape matches spec.
- [ ] 2.4 Filter non-episode items from Plex response. Verify: unit test includes non-episode items, confirms they're excluded.
- [ ] 2.5 Handle Plex non-2xx on episode query: throw explicit error. Verify: unit test simulates 500 response, confirms error logged and bubbled.

## 3. API Endpoints

- [ ] 3.1 Add `GET /api/libraries` endpoint returning persisted series libraries. Verify: integration test calls endpoint, returns JSON array of libraries.
- [ ] 3.2 Add `GET /api/libraries/{sectionKey}/episodes` endpoint returning episodes with resolved `local_path`. Verify: integration test calls endpoint, returns episodes with local_path and thumb.

## 4. Configuration

- [ ] 4.1 Ensure `MEDIA_ROOTS` configurable via env/config (comma-separated or JSON array). Verify: app starts with custom MEDIA_ROOTS, episode mapping uses them.

## 5. Verification & Testing

- [ ] 5.1 Run full test suite: `dotnet test PlexAutoplay.slnx -c Release` passes. Verify: all tests green.
- [ ] 5.2 Run linter/formatter: `dotnet format --verify-no-changes`. Verify: no formatting issues.
- [ ] 5.3 Run `openspec validate plex-libraries --strict` passes. Verify: validation succeeds.