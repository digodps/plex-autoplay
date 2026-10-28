# plex-libraries Specification

## Purpose
Defines the behavior for discovering Plex libraries, filtering to series-only libraries, and mapping library content to local media file paths via MEDIA_ROOT.

## Requirements

### Requirement: Discover Plex libraries
The system SHALL query the Plex Media Server API to list all available library sections.

#### Scenario: Successful library discovery
- **WHEN** the system calls `GET /library/sections` with a valid Plex token
- **THEN** the system returns a list of libraries with their key, title, type, and agent

#### Scenario: Plex server unreachable
- **WHEN** the Plex server at `PLEX_URL` is not reachable
- **THEN** the system returns an error indicating the Plex server is unavailable

### Requirement: Filter to series libraries only
The system SHALL include only libraries of type "show" (series) in the synchronized set.

#### Scenario: Series library included
- **WHEN** a Plex library section has `type="show"`
- **THEN** the system includes it in the series library list

#### Scenario: Movie library excluded
- **WHEN** a Plex library section has `type="movie"`
- **THEN** the system does not include it in the synchronized libraries

### Requirement: Map episodes to local media files
The system SHALL resolve each episode's media file path using the Plex `MediaPart.file` value and the configured `MEDIA_ROOT` bind mount.

#### Scenario: File path resolved via MEDIA_ROOT
- **WHEN** an episode's `MediaPart.file` is `/Volumes/Media/Series/Show/Season 1/Episode 1.mkv` and `MEDIA_ROOT=/media`
- **THEN** the system resolves the local path as `/media/Volumes/Media/Series/Show/Season 1/Episode 1.mkv`

#### Scenario: Media file not accessible
- **WHEN** the resolved media file path does not exist on disk
- **THEN** the system logs the missing file and skips marker detection for that episode
