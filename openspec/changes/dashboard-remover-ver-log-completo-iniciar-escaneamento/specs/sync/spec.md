# Spec Delta: Sync View — add view full log action

## Purpose
Enhances the synchronization view (`/sync`) to provide direct navigation to the full processing log view (`/logs`), relocating the workflow-oriented quick action from the dashboard to where sync operations and logs are managed.

## ADDED Requirements

### Requirement: Sync view provides navigation to full logs
The synchronization view SHALL provide a secondary action button to navigate to the full historical processing log view (`/logs`).

#### Scenario: User clicks view full log from sync view
- **WHEN** the user clicks the "Ver Log Completo" button in the sync view header or action area
- **THEN** the application navigates to the `/logs` route (`LogsView.vue`)
