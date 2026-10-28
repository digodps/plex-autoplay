# Spec Delta

## Purpose

Defines the behavior for a periodic BackgroundService that scans Plex series libraries, detects missing markers, and persists them to custom.json and SQLite.

## ADDED Requirements

### Requirement: Periodic synchronization
The system SHALL run a BackgroundService that scans all configured series libraries at a configurable interval.

#### Scenario: Default sync interval
- **WHEN** the service starts with no custom interval configured
- **THEN** the first scan begins 5 minutes after startup

#### Scenario: Configurable sync interval
- **WHEN** a user sets the sync interval to 30 minutes via the UI
- **THEN** subsequent scans run every 30 minutes

#### Scenario: Sync interval persisted
- **WHEN** the sync interval is changed through the application settings
- **THEN** the new interval is saved to app-config.json and takes effect on the next scan cycle

### Requirement: Scan only libraries without markers
The system SHALL skip episodes that already have markers stored in custom.json or SQLite.

#### Scenario: Episode already has markers
- **WHEN** an episode's rating key exists in custom.json with intro and credits markers
- **THEN** the system skips ffprobe analysis for that episode

#### Scenario: Episode missing markers
- **WHEN** an episode has no markers in custom.json
- **THEN** the system runs ffprobe silence detection and stores any detected markers

### Requirement: Scan progress reporting
The system SHALL expose scan status and progress through an API endpoint and SSE stream.

#### Scenario: Scan status query
- **WHEN** a user requests `GET /api/sync/status`
- **THEN** the system returns the current scan state (queued, running, completed, failed) and processed/skipped/error counts

#### Scenario: SSE progress stream
- **WHEN** a user connects to `GET /api/sync/log/stream`
- **THEN** the system sends server-sent events with scan progress updates

### Requirement: Scan cancellation
The system SHALL allow a running scan to be cancelled.

#### Scenario: Cancel running scan
- **WHEN** a user calls `POST /api/sync/cancel` during a running scan
- **THEN** the system interrupts the scan worker and updates status to cancelled
