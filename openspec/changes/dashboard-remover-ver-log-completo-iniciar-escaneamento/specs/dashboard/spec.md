# Spec Delta: Dashboard — remove quick actions

## Purpose
Refines the dashboard landing page spec: remove quick action buttons ("Iniciar Escaneamento", "Ver Log Completo") from the dashboard header so it remains a purely informational, glanceable view of system metrics. Sync-related quick actions move to the sync view instead (see `specs/sync/spec.md`).

## REMOVED Requirements

### Requirement: Dashboard provides quick actions (removed)
The dashboard SHALL NOT provide a primary action to start a new scan and a secondary action to navigate to the full sync log view in its header. Quick actions are workflow-oriented and belong on the sync page, not the landing summary.

Previously-present scenarios (removed):
- User clicks "Iniciar Escaneamento" → POST /api/plex/scan + navigate to /sync
- User clicks "Ver Log Completo" → navigate to /sync

Theme controls (light/dark toggle, accent palette) remain part of the dashboard header as global settings, not quick actions.

All other dashboard requirements (service health, last sync summary, library overview, marker coverage, card navigation, polling, loading/error states) remain unchanged and fully in effect.
