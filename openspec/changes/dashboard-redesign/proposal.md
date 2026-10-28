# Proposal

## Why

The current landing page (`/dashboard`) is a minimal placeholder showing only "Bem-vindo, administrador!" with a logout button. Users have no visibility into system health, recent sync activity, library overview, or marker coverage — the key signals that tell them "everything is working" or "action needed." A modern, minimalist Plex-style dashboard surfaces these metrics at a glance and provides quick actions, reducing the need to navigate into sub-menus for routine checks.

## What Changes

- **New dashboard view** replacing the placeholder at `/dashboard` with a responsive, card-based layout
- **Real-time polling** (30s interval) of service status, last sync job, library counts, and marker aggregates
- **Quick-action bar** with "Iniciar Escaneamento" (POST /api/plex/scan) and "Ver Log Completo" (navigate to `/sync`)
- **Navigation**: Clickable cards navigate to their respective detail views (Bibliotecas → `/libraries`, Marcadores → `/series`, Sincronização → `/sync`)
- **Visual design**: Plex-minimalist — âmbar accent (#e5a00d), elevated surface cards, system-ui typography, full dark/light support via existing CSS variables
- **Responsive**: Single-column stack on mobile (<768px), 2×2 grid on desktop; sidebar already collapses to top-bar
- **Reusable components**: `StatusCard`, `ActionBar` for consistency across views

## Capabilities

### New Capabilities

- `dashboard`: Frontend landing page that aggregates and displays real-time system health, sync status, library overview, and marker coverage with quick actions. Owns the `/dashboard` route, polling logic, and presentation components.

### Modified Capabilities

- None — this introduces a new frontend capability. Existing backend capabilities (`plex-sync`, `plex-libraries`, `plex-markers`, `telemetry`) already expose the required endpoints; no spec-level behavior changes.

## Impact

**Frontend (vue/plex-frontend):**
- `src/views/DashboardView.vue` — complete rewrite
- `src/stores/dashboard.js` — new Pinia store for polling + aggregated state
- `src/components/StatusCard.vue` — new reusable card component
- `src/components/ActionBar.vue` — new action bar component
- `src/api.js` — new helper functions for dashboard endpoints
- `src/router/index.js` — no changes (route already exists)

**Backend (src/PlexAutoplay.Web):**
- No changes required. Consumes existing endpoints:
  - `GET /api/status` (service health + last job + process metrics)
  - `GET /api/plex/libraries` (library list + series counts)
  - `GET /api/markers` (flat marker list, aggregated client-side)

**Design system:**
- Reuses existing CSS variables (`--accent`, `--surface`, `--bg-sunken`, `--radius`, `--shadow-2`, `--sans`) — no new tokens