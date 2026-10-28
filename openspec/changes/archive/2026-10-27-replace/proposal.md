# Proposal

## Why

Replace the existing Python/Flask `plex-autoplay` dashboard with a .NET 10 backend and Vue.js SPA. The current stack is limited to a single container and cannot be easily reused across future projects. The new architecture separates Domain and Application into pure class libraries so they can be reused in subsequent projects, while the presentation layer (Vue.js SPA) is replaceable without touching business rules.

## What Changes

- Replace Python/Flask backend with ASP.NET Core 10 Web API (Minimal API / Controllers)
- Replace server-rendered UI with Vue.js SPA (Vite + Pinia + Vue Router) served as static files from the same container
- Implement DDD + Clean Architecture: Domain + Application as pure class libraries; Infrastructure as adapters (EF Core + SQLite + Dapper, ffmpeg detection, Plex proxy, JWT auth)
- Reimplement PlexAutoSkip marker-skip WebSocket service in .NET (port 8081)
- Replace Redis dependency with in-memory / disk cache (Redis optional, not required at launch)
- Detect intro and credits markers via ffprobe silence detection (ffmpeg installed in Dockerfile)
- Proxy Plex images through .NET API so the Plex token is never exposed to the browser
- JWT stateless authentication (no expiration, password-based login)
- Configurable BackgroundService for periodic Plex library synchronization (initial 5 minutes, adjustable via UI)
- Only synchronize Plex libraries of type "show" (series)
- Require `MEDIA_ROOT` bind mount at Docker installation for direct access to Plex media files
- Plex URL and token provided via environment variables (`PLEX_URL`, `PLEX_TOKEN`)
- `JWT_SECRET` provided via environment variable

## Capabilities

### New Capabilities

- `plex-markers`: Detection, storage, and retrieval of intro/credits time markers for Plex series episodes via ffprobe analysis and custom.json persistence.
- `plex-libraries`: Discovery and browsing of Plex libraries (series only), with mapping to local media files via MEDIA_ROOT.
- `plex-sync`: Periodic BackgroundService that scans Plex series libraries, detects missing markers, and persists them to custom.json and SQLite.
- `plex-auth`: Stateless JWT authentication (password login, no expiration, token validation middleware).
- `plex-auto-skip`: .NET WebSocket service (port 8081) that reads custom.json and serves seek commands to compatible Plex players.
- `plex-images`: Proxy and cache of Plex thumbnail/artwork images through the .NET API.

### Modified Capabilities

- None (no existing specs in this project)

## Impact

- Full replacement of `plex-autoplay` Python/Flask codebase with .NET 10 + Vue.js
- Docker container remains single-container but swaps Python runtime for .NET runtime + ffmpeg
- SQLite replaces any prior persistence model; `custom.json` format preserved for PlexAutoSkip compatibility
- ffmpeg/ffprobe required in the Docker image for marker detection
- `MEDIA_ROOT` volume mount becomes a hard requirement for container startup
- Existing `custom.json` and `app-config.json` volume paths preserved for continuity
