# Tasks: Remove Quick Actions from Dashboard, Add to Sync

## 1. Dashboard View — Remove ActionBar

- [ ] 1.1 Edit `vue/plex-frontend/src/views/Dashboard/DashboardView.vue`: remove `ActionBar` import and its template usage from the header. Keep theme controls (light/dark toggle, accent palette) intact. **Verify:** Dashboard header shows only title/subtitle and theme controls; no "Iniciar Escaneamento" or "Ver Log Completo" buttons.

- [ ] 1.2 Remove `ActionBar` import if no longer used elsewhere in this file. **Verify:** No unused import warnings in dev tools / linter.

## 2. Sync View — Add "Ver Log Completo" Button

- [ ] 2.1 Edit `vue/plex-frontend/src/views/Sync/SyncView.vue`: add a secondary button "Ver Log Completo" (with AppIcon "sync" or "file-text") in the action area (header or alongside Start/Cancel). Button should router-link to `/logs` route. **Verify:** Button appears, is visually secondary (ghost/outline style), clicking navigates to `/logs` (LogsView.vue loads).

- [ ] 2.2 Ensure button is accessible: proper `aria-label`, keyboard focus visible, works with dark/light theme. **Verify:** Tab through sync view; button reachable and focused; contrast passes in both themes.

## 3. Spec Validation

- [ ] 3.1 Run `openspec validate dashboard-remover-ver-log-completo-iniciar-escaneamento --strict`. **Verify:** Passes with no errors.

## 4. Build & Lint

- [ ] 4.1 Run frontend lint/format: `npm run lint` in `vue/plex-frontend`. **Verify:** No linting errors.

- [ ] 4.2 Run full build: `./scripts/build.sh` (from repo root in worktree). **Verify:** Build completes successfully; Docker image builds.

## 5. QA — Visual / Behavioral (Playwright)

- [ ] 5.1 Launch app (via `docker compose -f infra/docker-compose.yml up -d` or dev server), log in, land on `/dashboard`.
  - Confirm no "Iniciar Escaneamento" or "Ver Log Completo" buttons in dashboard header.
  - Confirm theme toggle (light/dark) works; accent palette works.
  - Confirm 4 metric cards render correctly (service health, last sync, libraries, markers).
  - Confirm cards navigate on click (Service→/status or similar, Sync→/sync, Libraries→/libraries, Markers→/series).

- [ ] 5.2 Navigate to `/sync`.
  - Confirm "Ver Log Completo" button is present in action area.
  - Confirm clicking it navigates to `/logs` and LogsView loads.
  - Confirm "Iniciar Escaneamento" and "Cancelar" still work as before.

- [ ] 5.3 Theme/menu validation (mandatory per skill):
  - Toggle light/dark via header button → all views (dashboard, sync, logs, libraries, series) adapt without flash.
  - Sidebar/top-bar navigation: items open/close correctly; active route highlighted; mobile hamburger works.