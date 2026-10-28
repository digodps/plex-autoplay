# Design

## Context

See proposal.md for motivation. Current state:
- `DashboardView.vue` is an 18-line placeholder
- Sidebar navigation exists with 5 routes (Início, Séries, Bibliotecas, Sincronizar, Ajustes)
- CSS design system in `style.css` with CSS variables for theming (light/dark), spacing, typography, elevation
- Backend endpoints already exposed: `/api/status`, `/api/plex/libraries`, `/api/markers`
- Pinia stores used for auth (`auth.js`) and markers (`markers.js`)
- Vue 3 + Composition API + Vue Router 5

## Goals / Non-Goals

**Goals:**
- Replace placeholder with a fully functional, responsive dashboard
- Reuse existing design tokens — zero new CSS variables
- Polling-driven real-time feel (30s default) with clean start/stop lifecycle
- Reusable components (`StatusCard`, `ActionBar`) for future views
- Type-safe store with composed fetch logic

**Non-Goals:**
- WebSocket/SSE push (polling is sufficient for dashboard frequency)
- User-customizable widgets or layout persistence
- Historical charts/trends (single-point-in-time snapshot only)
- Backend changes — all endpoints exist

## Decisions

### 1. State management: New Pinia store `dashboard.js` (vs. inline in component)
**Rationale:** Separates polling lifecycle, aggregation logic, and error handling from presentation. Enables unit testing of fetch/transform logic. Follows existing pattern (`auth.js`, `markers.js`).
**Alternative considered:** Composable `useDashboard()` — rejected because polling lifecycle (start/stop on mount/unmount) maps cleanly to Pinia actions and component `onMounted`/`onUnmounted`.

### 2. Polling interval: 30 seconds, configurable via store option
**Rationale:** Matches typical sync job duration; frequent enough to feel "live" without hammering endpoints. 30s × 3 endpoints = 6 req/min, negligible load.
**Alternative considered:** 15s (too chatty), 60s (feels stale), adaptive (complexity not justified).

### 3. Data aggregation: Client-side in store (vs. new backend aggregation endpoint)
**Rationale:** `/api/markers` returns flat array (<5k items typical); reducing to counts by kind/origin is trivial in JS. Avoids backend change and keeps frontend autonomous.
**Alternative considered:** New `/api/dashboard/summary` endpoint — rejected (YAGNI, adds coupling).

### 4. Component structure: Two new reusable components
- `StatusCard.vue` — icon, label, primary value, secondary breakdown, optional badge, click handler
- `ActionBar.vue` — primary + secondary button slots, right-aligned
**Rationale:** Cards appear 4× on dashboard; actions appear in multiple future views. Extraction pays off immediately.

### 5. Navigation: Router-link wrapper on cards (vs. click handler + router.push)
**Rationale:** Semantic `<a>` for accessibility, works with browser history/back, styleable as card. `router-link` with custom component via `v-slot` API.

### 6. Loading states: Skeleton placeholders per card (vs. full-page spinner)
**Rationale:** Cards load independently (3 endpoints); per-card skeletons preserve layout stability and show partial data faster. Reuses existing `.skeleton` CSS utility.

### 7. Error handling: Per-card error badge + global retry
**Rationale:** Partial failure is likely (one endpoint down). Card-level error keeps other data visible. Global "Tentar novamente" re-triggers `fetchAll()`.

### 8. TypeScript: No (project uses vanilla JS)
**Rationale:** Consistency with existing codebase. JSDoc types in store for IDE support.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Polling drift (multiple tabs) | Single-tab typical; interval is coarse. Could add `visibilitychange` listener to pause in background tabs. |
| Marker aggregation grows with library size | Current <5k markers; client-side reduce is <5ms. Monitor and add backend aggregation if >50k. |
| Race condition: rapid route leave/enter | Store `stopPolling()` in `onUnmounted`; guard `startPolling()` with `if (!timer)` |
| Dark/light flash on load | CSS variables handle theme; no inline styles. Ensure skeleton uses `var(--bg-sunken)` / `var(--border)` |
| Accessibility: clickable cards | Use `router-link` with `role="button"` fallback, `aria-label` describing destination |

## Migration Plan

1. Create `src/stores/dashboard.js` with store definition
2. Create `src/components/StatusCard.vue` and `src/components/ActionBar.vue`
3. Rewrite `src/views/DashboardView.vue` composing store + components
4. Add API helpers to `src/api.js`
5. Verify existing route `/dashboard` works (no router changes)
6. Manual QA: light/dark, mobile/desktop, polling start/stop, error states

**Rollback:** Revert `DashboardView.vue` to placeholder; delete new files. No DB/migrations.

## Open Questions

- Should polling interval be user-configurable in Settings? (Defer — add later if requested)
- Add "last updated" timestamp in dashboard header? (Defer — trivial to add post-launch)