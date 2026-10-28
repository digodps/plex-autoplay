# Spec Delta

## Purpose

Defines the behavior for a .NET WebSocket service that reads `custom.json` and serves seek commands to compatible Plex players, replacing the Python PlexAutoSkip component.

## ADDED Requirements

### Requirement: Serve WebSocket on port 8081
The system SHALL expose a WebSocket endpoint on port 8081 for Plex Companion protocol-compatible players.

#### Scenario: WebSocket connection accepted
- **WHEN** a compatible Plex player connects to `ws://localhost:8081`
- **THEN** the system accepts the WebSocket connection

### Requirement: Read custom.json for markers
The system SHALL read marker data from `custom.json` (the same file written by the marker detection service) to determine skip points.

#### Scenario: custom.json loaded at startup
- **WHEN** the PlexAutoSkip service starts
- **THEN** the system loads all markers from `custom.json` into memory

#### Scenario: custom.json updated during runtime
- **WHEN** `custom.json` is modified by the marker scanning service
- **THEN** the system picks up the updated markers without requiring a restart

### Requirement: Serve seek commands to players
The system SHALL respond to player control messages with seek commands that skip intro or credits segments.

#### Scenario: Player requests skip for intro
- **WHEN** a connected player signals playback of an episode with an intro marker (0ms to 90000ms)
- **THEN** the system sends a seek command to jump to 90000ms

#### Scenario: Player requests skip for credits
- **WHEN** a connected player signals playback of an episode with a credits marker (85% to end)
- **THEN** the system sends a seek command to jump to the credits end

#### Scenario: Player incompatible with WebSocket protocol
- **WHEN** a Plex Web App or iOS client attempts to connect
- **THEN** the system cannot serve skip commands (known Plex Companion protocol limitation)
