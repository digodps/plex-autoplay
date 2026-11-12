# Design: Remove Quick Actions from Dashboard and Move to Sync

## Context
Current state (from `dashboard-redesign` change):
- `DashboardView.vue` includes an `ActionBar` component in its header with two actions:
  - Primary: "Iniciar Escaneamento" → POST `/api/plex/scan` then navigate to `/sync`
  - Secondary: "Ver Log Completo" → navigate to `/sync`
- `SyncView.vue` has its own "Iniciar Escaneamento" / "Cancelar" action buttons but no direct link to the full log view (`/logs`).
- Full log view is at `/logs` (route `logs` with `LogsView.vue`).
- Design system: Plex-minimalist, âmbar accent (#e5a00d), CSS variables for theming, reusable components (`StatusCard`, `ActionBar`).

## Goals / Non-Goals

**Goals:**
- Remove `ActionBar` (and its two buttons) from `DashboardView.vue` header — dashboard becomes purely informational.
- Add "Ver Log Completo" button to `SyncView.vue` header (or action area) linking to `/logs`.
- Preserve dark/light theme, responsive layout, polling, and all existing dashboard cards.
- No backend changes; purely frontend view modifications.

**Non-Goals:**
- Changing the `/dashboard` route or its polling behavior.
- Modifying `LogsView.vue` or its functionality.
- Adding new CSS variables or design tokens.

## Decisions

### 1. DashboardView.vue: Strip ActionBar entirely
**Rationale:** The dashboard spec describes it as a "single glanceable view" of metrics. Quick actions are workflow-oriented and belong on the relevant workflow page (sync), not the landing summary.
**Implementation:** Remove `ActionBar` import and its slot usage from the header. Keep the theme controls (light/dark/accent) which are global settings, not quick actions.

### 2. SyncView.vue: Add "Ver Log Completo" button
**Rationale:** Users on the sync page are already in the sync workflow; a direct link to the full historical log (`/logs`) is contextually appropriate.
**Placement:** Add a secondary button in the sync view's action area (header or alongside Start/Cancel), styled with `.btn-ghost` or similar to be visually secondary to "Iniciar Escaneamento".
**Navigation:** Router push to `/logs` (existing route).

### 3. Reusable components
- `ActionBar.vue` remains in the codebase (may be used by other future views) but is no longer used by DashboardView.
- No new components needed.

### 4. Spec delta
- Dashboard spec: Remove "Quick actions" requirement and its scenarios (start scan, view full log).
- Sync spec (new delta): Add requirement for "Ver Log Completo" navigation from sync view.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Users accustomed to starting scan from dashboard | Scan can still be started from `/sync` (existing button); workflow is one extra click but more logical |
| "Ver Log Completo" less discoverable on dashboard | Log access is now contextual: on sync page, where logs are most relevant |

## Migration Plan

1. Edit `vue/plex-frontend/src/views/Dashboard/DashboardView.vue`: remove `ActionBar` import and template usage.
2. Edit `vue/plex-frontend/src/views/Sync/SyncView.vue`: add "Ver Log Completo" button (router-link to `/logs`) in action area.
3. Update spec delta: modify dashboard spec (remove quick actions), add sync spec delta (add log link).
4. Update `tasks.md` with implementation tasks.
5. Run build & QA (lint, type-check, manual visual verification: theme toggle, menu states).

**Rollback:** Revert the two view files. No DB or config changes.