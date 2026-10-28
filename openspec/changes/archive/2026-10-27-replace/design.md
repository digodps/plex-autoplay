# Design

## Context

The existing `plex-autoplay` is a Python/Flask dashboard running in a single Docker container with supervisord (Flask + Redis + PlexAutoSkip). It exposes a web UI, a REST API, SSE scan progress, and a WebSocket-based PlexAutoSkip service on port 8081. The new system replaces this with .NET 10 + Vue.js SPA in a single container. Domain and Application layers are pure class libraries for reuse in future projects. `MEDIA_ROOT` bind mount is mandatory for direct file access to Plex media. Plex URL and token come from environment variables.

## Goals / Non-Goals

**Goals:**
- Replace the full Python/Flask stack with .NET 10 Web API + Vue.js SPA
- Preserve all current features: library discovery, marker scanning/editing, custom.json output, PlexAutoSkip WebSocket skip, SSE progress, webhook handling
- Make Domain and Application reusable across future projects
- Run in a single Docker container with no Redis dependency at launch
- Serve Vue SPA as static files from the same ASP.NET process

**Non-Goals:**
- Multi-container deployment
- Redis-based distributed cache at launch (may be added later)
- Support for Plex Web App or iOS app (PlexAutoSkip protocol limitation remains)
- Movies or music libraries (series only per scope)

## Decisions

**1. Dapper over EF Core**
- Rationale: Lightweight micro-ORM, full SQL control, minimal overhead for SQLite read-heavy access patterns
- Alternatives considered: EF Core (rejected: heavier, migrations complexity, unnecessary abstraction for this scope)

**2. ffprobe via System.Diagnostics.Process (not FFmpeg.AutoGen)**
- Rationale: Simpler to debug and maintain; ffmpeg/ffprobe installed in Dockerfile; avoids unsafe bindings and manual memory management
- Alternatives considered: FFmpeg.AutoGen (rejected: complex unsafe API, steeper learning curve, no clear benefit for silence detection)

**3. JWT stateless, no expiration**
- Rationale: Simplest internal-use authentication; no session store needed; token validation is pure signature check
- Alternatives considered: ASP.NET Identity, cookie sessions, OAuth (rejected: over-engineering for single-user internal tool)

**4. Vue.js SPA served from ASP.NET static files**
- Rationale: Single-container deployment simplicity; team already knows Vue; backend remains API-agnostic
- Alternatives considered: Blazor WASM (rejected: larger initial payload), separate Nginx container (rejected: multi-container complexity)

**5. BackgroundService for sync, interval configurable via UI**
- Rationale: Built-in .NET hosting abstraction; interval stored in app-config.json/SQLite and editable through the Vue settings page
- Alternatives considered: Quartz.NET, Hangfire (rejected: extra dependencies for a simple periodic task)

## Risks / Trade-offs

[Silence-only marker detection may miss credits with background music] → Accept for v1; visual logo detection can be added later as a separate detector implementation behind `IMarkerDetector`

[JWT with no expiration is valid forever if leaked] → Mitigate by keeping deployment internal-only and rotating `JWT_SECRET` periodically

[SQLite write concurrency during scans] → Use WAL mode and serialize scan writes; acceptable for internal single-user use

[PlexAutoSkip WebSocket protocol may not work on iOS/Web] → Known upstream limitation; document clearly; no workaround without Plex protocol changes

## Migration Plan

1. Build new .NET 10 Docker image with ffmpeg installed
2. Mount existing `config/` volume (contains `app-config.json`, `custom.json`) to `/data`
3. Set `MEDIA_ROOT` to the host path of Plex media files
4. Set `PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`
5. Deploy container; existing `custom.json` remains compatible with new PlexAutoSkip
6. Decommission Python/Flask container

## Open Questions

- Whether to add refresh token support in a future version if JWT rotation becomes necessary
- Whether to add visual logo detection for credits (requires ffmpeg video filters, more CPU)
