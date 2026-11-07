# Proposal

## Why

The current frontend grew organically: every view carries its own scoped CSS or external stylesheet, token aliases (`--speed`, `--ease`, `--shadow-1`, `--shadow-2`, `--bg-sunken`, `--mono`) are referenced but not consistently defined, and the persistent sidebar duplicates navigation that a breadcrumb can express. The result is visual drift between screens, duplicated styling, and a navigation model that does not match the requested wizard-style backtracking.

This change recreates the frontend presentation from zero with a single neutral/editorial visual language, one global token source, no menu, and breadcrumb-first navigation. Existing backend API contracts and user flows are preserved.

## What Changes

- **Recreate `vue/plex-frontend/src` from scratch** (views, components, styles, stores, composables, router, entrypoint), keeping Vue 3 + Vite + Pinia + Vue Router + Tailwind and the existing backend API contracts.
- **Delete legacy UI assets**: scoped styles, per-view CSS files (`SeriesView.css`, `marker-editor.css`, `sync-view.css`), `src/styles/components/`, unused Vite demo components/assets.
- **Introduce one global stylesheet** composed of `tokens.css` (single source of truth) + `app.css` (shared layout, components, states, transitions). Tailwind utilities are allowed for layout only; no per-view or scoped CSS.
- **Replace sidebar navigation** with a sticky top bar: brand mark, breadcrumb trail (`Dashboard → Bibliotecas → Séries → Marcadores`), theme toggle, and logout. No menu, no rail, no dock.
- **Add real breadcrumb titles** for libraries and series using a small UI store populated by the loaded data.
- **Neutral editorial visual system**: grayscale palette, black primary in light mode / white primary in dark mode, no brand color. Dark/light follows the OS with a manual toggle persisted through the existing `GET/PUT /api/theme`.
- **Add an icon library** (Lucide for Vue) and remove the hand-maintained icon sprite.
- **Unify states**: shared skeleton, empty, and error components; fluid fade/slide route transitions honoring `prefers-reduced-motion`; responsive grids collapsing to one column at ≤768px with ≥44px touch targets.
- **Rebuild all 7 screens**: Login, Dashboard, Libraries, Series, Marker Editor, Sync, Settings, preserving auth, scan, SSE log, marker CRUD, settings and theme flows.
- **Update E2E coverage** to the new UI using stable `data-testid` hooks: login/logout, full breadcrumb, marker CRUD, scan + log, settings + theme, mobile responsiveness.

## Capabilities

### New Capabilities

- `web-ui`: Unified neutral editorial frontend with global design tokens, breadcrumb-first navigation, real titles in the trail, fluid transitions, responsive layouts, and shared loading/empty/error states.

### Modified Capabilities

- None at spec level. Backend contracts (`/api/status`, `/api/plex/*`, `/api/markers`, `/api/sync/*`, `/api/theme`, `/auth/login`) are unchanged.
- Existing `navegacao-lateral`, `dashboard`, `apresentacao`, `series-*` specs are superseded for the frontend presentation; they will be archived/updated after this change is complete.

## Impact

**Frontend (`vue/plex-frontend`):**
- Rewrite: `src/App.vue`, `src/main.js`, `src/router/index.js`, `src/api.js`, all `src/views/*`, all `src/components/*`, all `src/stores/*`, all `src/composables/*`, `src/styles/*`.
- Remove: `src/styles/components/*`, `src/views/*.css`, demo `HelloWorld.vue`, unused Vite assets.
- Add: `src/stores/ui.js` (breadcrumb titles), shared `PageHeader`, `Breadcrumb`, `AppShell`, `StateBlock`, `DataCard` components, icon library dependency.
- `package.json`: add icon library, remove unused Material Tailwind integration if verified unused.
- `tailwind.config.cjs`: neutral token mapping, keep preflight disabled, no brand purple.

**Tests:**
- `tests/e2e/tests/*` updated/added with stable selectors for the new UI.
- `vue/plex-frontend/tests/tokens.contrast.test.mjs` updated to cover the new neutral token pairs (WCAG AA).

**Backend:** no changes.

## Goals / Non-Goals

**Goals**
- One visual standard across all 7 screens.
- Zero per-view/per-component CSS files or scoped style blocks.
- Breadcrumb-only navigation with real titles and clickable ancestors.
- Fluid, responsive, accessible UI (WCAG AA).
- Preserve all functional flows and backend contracts.

**Non-Goals**
- Backend/API changes beyond consuming existing endpoints.
- New product features.
- Forced linear wizard; deep links stay allowed.
- Per-page theming or accent color selection.

## Risks / Trade-offs

- Full recreation can regress flows → mitigated by E2E coverage and preserving API/store contracts.
- Real breadcrumb titles need view→store wiring → mitigated with a tiny `ui` store and route watches.
- Tailwind + global CSS can drift → mitigated by a lint/QA check that forbids scoped styles and per-view CSS files.
- Icon library adds a dependency → mitigated by using one well-known package and removing the sprite.

## Open Questions

- None. Requirements confirmed with the user across three refinement rounds.
