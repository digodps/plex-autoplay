# plex-markers Specification

## Purpose
Defines the behavior for detecting, storing, and retrieving intro and credits time markers for Plex series episodes, persisting them in PlexAutoSkip-compatible `custom.json` format.

## Requirements

### Requirement: Detect intro markers from audio silence
The system SHALL analyze a media file using ffprobe silence detection and identify contiguous silent segments at the start of the episode as candidate intro markers.

#### Scenario: Intro detected at episode start
- **WHEN** ffprobe finds silence from 0ms to approximately 90000ms in a series episode
- **THEN** the system creates a marker of kind `intro` with `start_ms=0` and `end_ms` equal to the silence end

#### Scenario: No silence at episode start
- **WHEN** ffprobe finds no significant silence in the first 120 seconds of audio
- **THEN** the system does not create an intro marker for that episode

### Requirement: Detect credits markers from audio silence
The system SHALL analyze a media file using ffprobe silence detection and identify contiguous silent segments near the end of the episode as candidate credits markers.

#### Scenario: Credits detected at episode end
- **WHEN** ffprobe finds silence from approximately 85% of duration to the end of the episode
- **THEN** the system creates a marker of kind `credits` with `start_ms` at the silence start and `end_ms` at the episode duration

#### Scenario: Episode shorter than minimum credits threshold
- **WHEN** the episode duration is less than 300000ms
- **THEN** the system does not attempt credits detection for that episode

### Requirement: Persist markers in custom.json format
The system SHALL write detected markers to `custom.json` in the PlexAutoSkip format, keyed by Plex rating key.

#### Scenario: New marker written to custom.json
- **WHEN** a marker is detected for an episode with rating key `12345`
- **THEN** `custom.json` contains an entry for rating key `12345` with the marker's start_ms, end_ms, and kind

#### Scenario: Existing marker not duplicated
- **WHEN** `custom.json` already contains a marker for rating key `12345`
- **THEN** the system does not overwrite it unless explicitly triggered by a manual re-scan

### Requirement: Manual marker creation and editing
The system SHALL provide a UI and API for users to create, update, and remove intro and credits markers per episode.

#### Scenario: User creates a manual marker
- **WHEN** a user submits `{ rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 }` via the API
- **THEN** the system stores the marker in SQLite and writes it to `custom.json`

#### Scenario: User removes a marker
- **WHEN** a user deletes a marker by rating key and kind
- **THEN** the system removes it from SQLite and rewrites `custom.json` without that entry

### Requirement: Query stored markers
The system SHALL expose endpoints to list all stored markers and retrieve markers for a specific episode or library.

#### Scenario: List all markers
- **WHEN** a user requests `GET /api/markers`
- **THEN** the system returns all markers stored in SQLite with their rating key, kind, start_ms, and end_ms

#### Scenario: Markers for a specific episode
- **WHEN** a user requests `GET /api/markers?rating_key=12345`
- **THEN** the system returns only markers associated with rating key `12345`
