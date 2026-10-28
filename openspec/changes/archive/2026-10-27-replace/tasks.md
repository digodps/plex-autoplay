# Tasks

## 1. Project Scaffolding

- [x] 1.1 Create .NET 10 solution with Domain, Application, Infrastructure, Web class libraries and verify `dotnet build` succeeds
- [x] 1.2 Add Dapper and Microsoft.Data.Sqlite NuGet packages to Infrastructure project and verify packages restore
- [x] 1.3 Create Dockerfile based on `mcr.microsoft.com/dotnet/aspnet:10` with ffmpeg installed and verify `docker build` succeeds
- [x] 1.4 Create docker-compose.yml with MEDIA_ROOT volume mount, PLEX_URL, PLEX_TOKEN, JWT_SECRET, ADMIN_PASSWORD environment variables and verify container starts

## 2. Domain Layer

- [x] 2.1 Define Marker entity (RatingKey, Kind, StartMs, EndMs) and verify it compiles with no external dependencies
- [x] 2.2 Define IMarkerDetector port interface in Domain and verify it references no infrastructure types
- [x] 2.3 Define IMarkerRepository port interface in Domain and verify it references no infrastructure types

## 3. Application Layer

- [x] 3.1 Define DTOs for marker detection requests/responses and verify they serialize correctly
- [x] 3.2 Implement ScanLibraries use case that orchestrates library discovery and marker detection and verify it compiles against Domain ports
- [x] 3.3 Implement SaveMarker use case for manual marker CRUD and verify it compiles against Domain ports

## 4. Infrastructure Layer

- [x] 4.1 Implement SqliteMarkerRepository using Dapper and verify it can insert/query markers against a local SQLite database
- [x] 4.2 Implement FfmpegMarkerDetector calling ffprobe via System.Diagnostics.Process with silencedetect filter and verify it parses silence output into Marker objects
- [x] 4.3 Implement JwtTokenService that signs/validates JWT with JWT_SECRET and no expiration and verify token round-trip validation
- [x] 4.4 Implement ImageCacheService using IMemoryCache or disk cache under /data/image-cache and verify cache hit/miss behavior
- [x] 4.5 Implement ImageProxyService that forwards Plex image requests with server-side PLEX_TOKEN and verify it returns image bytes without exposing the token

## 5. Web API Layer

- [x] 5.1 Implement JWT auth middleware that validates bearer token signature and rejects unauthenticated requests with 401 and verify protected endpoints return 401 without token
- [x] 5.2 Implement login endpoint POST /auth/login validating ADMIN_PASSWORD and issuing JWT and verify correct password returns token, wrong password returns 401
- [x] 5.3 Implement library endpoints (GET /api/plex/libraries filtering to series only) and verify response contains only type=show libraries
- [x] 5.4 Implement scan endpoints (POST /api/plex/scan, GET /api/sync/status, SSE log stream, POST /cancel) and verify scan lifecycle transitions
- [x] 5.5 Implement marker CRUD endpoints (GET/POST/DELETE /api/markers) and verify marker persistence in SQLite and custom.json
- [x] 5.6 Implement image proxy endpoint GET /api/images/* and verify it returns proxied image with correct Content-Type

## 6. Sync BackgroundService

- [x] 6.1 Implement SyncBackgroundService with default 5-minute interval reading SYNC_INTERVAL_MINUTES env var and verify first scan runs at 5 minutes
- [x] 6.2 Make sync interval configurable via app-config.json and UI settings page and verify interval change takes effect on next cycle
- [x] 6.3 Skip episodes already having markers in custom.json/SQLite and verify re-scan does not duplicate existing markers

## 7. PlexAutoSkip (.NET WebSocket)

- [x] 7.1 Implement WebSocket server on port 8081 reading custom.json and verify player connection is accepted
- [x] 7.2 Implement seek command delivery for intro and credits markers and verify skip commands are sent to connected players
- [x] 7.3 Watch custom.json for changes and reload markers without restart and verify updated markers are served

## 8. Vue.js SPA

- [x] 8.1 Scaffold Vue 3 + Vite project with Pinia and Vue Router and verify `npm run build` produces static assets
- [x] 8.2 Implement login page with JWT storage and verify successful login stores token
- [x] 8.3 Implement libraries page showing series cards from Plex and verify data loads from API
- [x] 8.4 Implement marker editing UI (create/edit/remove) and verify changes persist via API
- [x] 8.5 Implement sync status page with progress SSE stream and cancel button and verify scan progress updates in real time
- [x] 8.6 Implement settings page for sync interval configuration and verify interval is saved and applied

## 9. Docker & Deployment

- [x] 9.1 Configure ASP.NET to serve Vue SPA static files from wwwroot and verify SPA routes fall back to index.html
- [x] 9.2 Configure volume mapping for /data (SQLite, custom.json, image-cache) and verify data persists across container restarts
- [x] 9.3 Document deployment steps in README and verify a fresh deploy from the docs works end-to-end

## 10. Integration & Verification

- [x] 10.1 Run end-to-end scan: discover series libraries, detect markers via ffprobe, write custom.json and verify PlexAutoSkip can read it
- [x] 10.2 Run end-to-end skip: connect a mock WebSocket player, trigger intro/credits detection, verify seek command is sent
- [x] 10.3 Verify JWT auth blocks unauthenticated access to all protected endpoints

## Workflow follow-up

- Archive the change after the project's review requirements are satisfied.
- Verify the archived result.
