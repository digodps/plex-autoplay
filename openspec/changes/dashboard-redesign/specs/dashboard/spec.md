# Spec Delta

## Purpose

Provides a real-time landing dashboard that aggregates service health, recent sync activity, library overview, and marker coverage into a single glanceable view with quick actions.

## ADDED Requirements

### Requirement: Dashboard displays service health status
The dashboard SHALL display the current service health state (online/degraded/offline), uptime, and resource utilization (CPU, memory) fetched from the status endpoint.

#### Scenario: Service online with normal metrics
- **WHEN** the status endpoint returns status "ok" with uptime and process metrics
- **THEN** the dashboard shows a green status indicator, uptime formatted as human-readable duration, CPU percentage, and memory in MB

#### Scenario: Service degraded or offline
- **WHEN** the status endpoint returns a non-ok status or is unreachable
- **THEN** the dashboard shows a yellow/red status indicator and surfaces the error state

### Requirement: Dashboard displays last synchronization summary
The dashboard SHALL display the timestamp, processed count, skipped count, and error count of the most recent sync job.

#### Scenario: Recent sync completed successfully
- **WHEN** the status endpoint returns a lastJob with status "completed"
- **THEN** the dashboard shows the completion timestamp, processed count with green checkmark, skipped count, and zero errors

#### Scenario: Recent sync had errors
- **WHEN** the status endpoint returns a lastJob with errors > 0
- **THEN** the dashboard shows the error count with a red indicator alongside processed/skipped counts

#### Scenario: No sync has run yet
- **WHEN** the status endpoint returns lastJob as null
- **THEN** the dashboard shows "Nunca executado" with a neutral indicator

### Requirement: Dashboard displays library overview
The dashboard SHALL display the total number of series libraries and total series count aggregated from the libraries endpoint.

#### Scenario: Libraries loaded with series
- **WHEN** the libraries endpoint returns one or more libraries with series
- **THEN** the dashboard shows the library count and total series count

#### Scenario: No libraries configured
- **WHEN** the libraries endpoint returns an empty array
- **THEN** the dashboard shows zero libraries and zero series with a prompt to configure Plex

### Requirement: Dashboard displays marker coverage
The dashboard SHALL display total marker count broken down by kind (intro/credits) and origin (detected/manual) aggregated from the markers endpoint.

#### Scenario: Markers exist with mixed kinds and origins
- **WHEN** the markers endpoint returns markers of both kinds and origins
- **THEN** the dashboard shows total count, intro count, credits count, and a breakdown of detected vs manual

#### Scenario: No markers recorded
- **WHEN** the markers endpoint returns an empty array
- **THEN** the dashboard shows zero markers with a prompt to run a scan

### Requirement: Dashboard provides quick actions
The dashboard SHALL provide a primary action to start a new scan and a secondary action to navigate to the full sync log view.

#### Scenario: User clicks start scan
- **WHEN** user clicks "Iniciar Escaneamento" button
- **THEN** the dashboard calls POST /api/plex/scan and navigates to the sync view to show progress

#### Scenario: User clicks view full log
- **WHEN** user clicks "Ver Log Completo" button
- **THEN** the dashboard navigates to /sync route

### Requirement: Dashboard cards navigate to detail views
Each metric card SHALL be clickable and navigate to its corresponding detail view.

#### Scenario: Click libraries card
- **WHEN** user clicks the libraries card
- **THEN** the app navigates to /libraries route

#### Scenario: Click markers card
- **WHEN** user clicks the markers card
- **THEN** the app navigates to /series route (series index)

#### Scenario: Click sync card
- **WHEN** user clicks the sync card
- **THEN** the app navigates to /sync route

### Requirement: Dashboard polls for real-time updates
The dashboard SHALL automatically refresh all metrics at a configurable interval (default 30 seconds) without user interaction.

#### Scenario: Periodic refresh updates displayed data
- **WHEN** the polling interval elapses
- **THEN** the dashboard re-fetches all endpoints and updates displayed values if changed

#### Scenario: Polling pauses when dashboard not visible
- **WHEN** user navigates away from /dashboard
- **THEN** polling stops until user returns to the dashboard

### Requirement: Dashboard handles loading and error states gracefully
The dashboard SHALL show skeleton placeholders while loading and inline error messages when endpoints fail, without breaking the layout.

#### Scenario: Initial load shows skeletons
- **WHEN** dashboard mounts before first fetch completes
- **THEN** each card shows a skeleton animation matching its layout

#### Scenario: Individual endpoint failure shows inline error
- **WHEN** one endpoint fails but others succeed
- **THEN** the affected card shows an error badge while other cards display data normally

#### Scenario: Complete failure shows retry action
- **WHEN** all endpoints fail
- **THEN** the dashboard shows a full-view error with a "Tentar novamente" button