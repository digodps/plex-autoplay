# plex-images Specification

## Purpose
Defines the behavior for proxying and caching Plex thumbnail and artwork images through the .NET API so the Plex token is never exposed to the browser.

## Requirements

### Requirement: Proxy Plex images
The system SHALL act as a proxy for Plex image URLs, adding the `X-Plex-Token` server-side and returning image bytes to the client.

#### Scenario: Client requests a thumbnail
- **WHEN** the Vue frontend requests `GET /api/images/thumb/{rating_key}`
- **THEN** the system fetches the image from Plex using the server-side `PLEX_TOKEN` and returns it with the correct `Content-Type`

#### Scenario: Plex token not exposed
- **WHEN** the browser inspects network requests to `/api/images/`
- **THEN** the `PLEX_TOKEN` value is not visible in request headers or URLs

### Requirement: Cache proxied images
The system SHALL cache proxied images to avoid repeated calls to the Plex server for the same image.

#### Scenario: Image served from cache
- **WHEN** the same image is requested twice within the cache TTL
- **THEN** the second request is served from memory or disk cache without calling Plex

#### Scenario: Cache miss
- **WHEN** an image is not in the cache
- **THEN** the system fetches it from Plex, stores it in cache, and returns it to the client
