# Proposal: Remove Quick Actions from Dashboard and Move to Sync

## Why
The landing dashboard (`/dashboard`) currently includes quick action buttons ("Iniciar Escaneamento" and "Ver Log Completo") in its header. To keep the dashboard clean and focused purely on glanceable system metrics/health, these quick actions should be removed from the dashboard header. Furthermore, the "Ver Log Completo" action should be relocated to the sync view (`/sync`), improving user workflow when monitoring or reviewing sync operations.

## What Changes
- **DashboardView.vue**: Remove the "Iniciar Escaneamento" and "Ver Log Completo" action buttons (and their enclosing ActionBar / header controls).
- **SyncView.vue**: Add a "Ver Log Completo" button (navigating to `/logs` or appropriate log view) alongside existing scan/cancel actions.
- **Spec & Design**: Update dashboard and sync specifications to reflect the removal of quick actions from the dashboard header and the addition of the log view link in the sync view.

## Capabilities

### Modified Capabilities
- `dashboard`: Glanceable metrics landing page no longer hosts quick action buttons in its header.
- `plex-sync`: Sync view now provides direct access to the full processing log ("Ver Log Completo").
