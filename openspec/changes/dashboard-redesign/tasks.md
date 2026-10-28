# Tasks

## 1. Store & API Helpers

- [x] 1.1 Create `src/stores/dashboard.js` with Pinia store: state (status, libraries, markers, loading, error), `fetchAll()` calling three endpoints, aggregation logic (library counts, marker breakdown by kind/origin), `startPolling()`/`stopPolling()` with 30s interval, `onMounted`/`onUnmounted` lifecycle. **Verify:** Store exports `useDashboardStore`; `fetchAll()` populates state with correct shapes; polling starts/stops without leaks.

- [x] 1.2 Add API helpers to `src/api.js`: `fetchStatus()`, `fetchLibraries()`, `fetchMarkers()` using existing `apiFetch`. **Verify:** Each helper returns parsed JSON or throws on non-ok; no duplicate auth logic.

## 2. Reusable Components

- [x] 2.1 Create `src/components/StatusCard.vue`: props `icon`, `label`, `value`, `breakdown?`, `badge?`, `href?`, `loading?`, `error?`. Renders icon + label, primary value, optional secondary breakdown rows, optional badge (color via prop), skeleton when loading, error badge when error. Wraps in `<router-link>` if `href` provided. **Verify:** Renders correctly in all prop combinations; clicking navigates when `href` set; skeleton matches card dimensions.

- [x] 2.2 Create `src/components/ActionBar.vue`: slots `primary`, `secondary`. Renders primary button (`.btn`) right-aligned, secondary button (`.btn-ghost`) left of primary. **Verify:** Buttons render in correct order; slots accept arbitrary content; styling matches design system.

## 3. Dashboard View Rewrite

- [x] 3.1 Rewrite `src/views/DashboardView.vue`: import store, `StatusCard`, `ActionBar`. On mount: `store.fetchAll()`, `store.startPolling()`. On unmount: `store.stopPolling()`. Template: 2×2 grid (desktop) / single column (mobile) of `StatusCard` for Service Health, Last Sync, Libraries, Markers. Each card binds store state slices. `ActionBar` with "Iniciar Escaneamento" (calls `apiFetch('/api/plex/scan', {method:'POST'})` then `router.push('/sync')`) and "Ver Log Completo" (`router.push('/sync')`). **Verify:** Page loads, shows skeletons → data; cards navigate correctly; actions trigger API + navigation; polling updates values without remount.

- [x] 3.2 Add responsive grid CSS to `DashboardView.vue` `<style scoped>`: `display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem;` with `@media (max-width: 768px) { grid-template-columns: 1fr; }`. Reuse `--gap` or hardcode 1rem. **Verify:** DevTools device toolbar shows 2-col ≥768px, 1-col <768px; no horizontal overflow.

## 4. Integration & Polish

- [x] 4.1 Verify dark/light theme: open in both `prefers-color-scheme` modes; all cards use CSS variables (no hardcoded colors). **Verify:** No flash, borders/backgrounds adapt; âmbar accent on primary button and status badges.

- [x] 4.2 Error state coverage: simulate failing `/api/status` (network tab → block request), confirm card shows error badge, other cards still render, global "Tentar novamente" appears when all fail. **Verify:** Partial failure → per-card error; total failure → full retry button.

- [x] 4.3 Accessibility spot-check: tab through dashboard; cards reachable via keyboard (router-link), buttons have focus-visible ring, color contrast passes (âmber on surface). **Verify:** `axe-core` or manual tab order; no console violations.

## 5. End-to-End Verification

- [ ] 5.1 Run dev server (`npm run dev` in `vue/plex-frontend`), log in, land on `/dashboard`. Confirm all 4 cards populate with real backend data. **Verify:** Values match backend (cross-check `/api/status`, `/api/plex/libraries`, `/api/markers` in Network tab).

- [ ] 5.2 Trigger a scan from dashboard "Iniciar Escaneamento", confirm redirect to `/sync` with job running. **Verify:** SyncView shows running status, processed count increments.

- [ ] 5.3 Leave dashboard open >60s, confirm values refresh (polling) without manual reload. **Verify:** Network tab shows repeating GET requests every ~30s; UI updates if backend data changed.

## Workflow follow-up

- Archive the change after implementation review is satisfied.
- Verify the archived result.