# Design

## Context

`vue/plex-frontend` is a Vue 3 + Vite + Pinia + Vue Router + Tailwind SPA served by the .NET backend. It talks to `/api/*` and `/auth/login`. The current UI has a persistent sidebar, view-scoped CSS, duplicated/incomplete token aliases, and a hand-maintained icon sprite.

The new UI is a presentation-layer rewrite only. API endpoints, authentication (token in cookie + `Authorization` header), data shapes, and route paths stay the same. The UI becomes a wizard-like flow with a breadcrumb trail as the only navigation, a neutral editorial design, and a single global stylesheet.

## Goals / Non-Goals

**Goals**
- One global design-token source: `src/styles/tokens.css`.
- One global stylesheet: `src/styles/app.css` (layout, primitives, states, transitions).
- Tailwind used for layout utilities only; colors/spacing/radii come from tokens.
- No `<style>` blocks, no `*.css` files under `views/` or `components/`.
- Breadcrumb-first navigation with real library/series titles.
- Responsive, WCAG AA, reduced-motion aware.
- Preserve auth, dashboard, libraries, series, markers CRUD, scan/SSE, settings, theme.

**Non-Goals**
- Backend changes.
- New features.
- Linear-only navigation; deep links remain valid.
- Per-page themes or user-selected accent colors.

## Decisions

### 1. Recreate `src` from zero, keep stack and API

Vue 3 + Vite + Pinia + Vue Router + Tailwind remain. `src/api.js` keeps `apiFetch`, error extraction, and endpoint helpers. Stores keep the same responsibilities, but are rewritten for the new UI. Routing paths stay identical (`/login`, `/dashboard`, `/libraries`, `/libraries/:key/series`, `/series/:seriesKey/markers`, `/sync`, `/settings`).

Rationale: avoids a risky stack migration and keeps backend contracts intact while allowing a clean presentation rewrite.

### 2. Single global stylesheet, Tailwind for layout only

`tokens.css` defines every color, spacing, radius, shadow, font and motion value for light and dark. `app.css` holds the reset, shell, primitives, states and transitions. Views/components use semantic classes from `app.css` plus Tailwind utilities for layout (`flex`, `grid`, `gap-*`, `p-*`). No `<style>` block is allowed in `.vue` files; no CSS file may be imported by a view/component.

Rationale: guarantees the same visual standard everywhere and makes drift detectable by a simple source check.

### 3. Neutral editorial palette

Grayscale only. Light mode primary actions are black (`--accent`), dark mode primary actions are white; on-accent text flips via `--accent-contrast`. Existing `--brand*` names are kept as neutral aliases so the contrast test and any remaining references stay valid, but no purple is used.

Token pairs must meet WCAG AA: text ≥4.5:1, controls/borders/focus ≥3:1.

### 4. Shell: sticky breadcrumb bar, no menu

`App.vue` renders:
- Login route: centered auth card, no shell/breadcrumb.
- Authenticated routes: sticky top bar with brand mark (button to `/dashboard`), centered breadcrumb, right-side theme toggle and logout.
- `<main>` with the routed page and a `page` transition.

No sidebar, rail, dock or nav list. Breadcrumbs are the only navigation control.

### 5. Breadcrumb contract

`Breadcrumb.vue` is presentational: receives `items` (`{ label, to?, current? }[]`), emits `select(to)` for ancestors, renders the current item as non-interactive `aria-current="page"`. It does not import `vue-router`.

`useAppShell` (composable) computes the trail from the route:
- `dashboard`: `Início`
- `libraries`: `Início → Bibliotecas`
- `series`: `Início → Bibliotecas → <library title>`
- `series-markers`: `Início → Bibliotecas → <series title> → Marcadores`
- `sync`: `Início → Sincronização`
- `settings`: `Início → Ajustes`

Real titles come from a tiny `stores/ui.js` (`setLibraryTitle`, `setSeriesTitle`) populated by the Libraries/Series views when data loads. Until loaded, fall back to `Bibliotecas`/`Séries`. Deep links are allowed; missing titles fall back gracefully.

### 6. Theme

Reuse the existing theme contract (`GET/PUT /api/theme`, `mode` in `light|dark|system`). `stores/theme.js` applies `data-theme` before mount (anti-FOUC script in `index.html`), follows the OS in `system` mode, and exposes `cycleMode()` for the top-bar toggle. Neutral only.

### 7. Icons

Use `lucide-vue-next`. Remove `public/icons.svg` sprite usage and the old `AppIcon.vue`. Icons are imported per component and sized via props/classes.

### 8. Shared state primitives

Create `StateBlock.vue` (loading skeleton / empty / error with retry) and `PageHeader.vue` (eyebrow, title, subtitle, actions). All views use them, so loading/empty/error look identical.

### 9. Data layer

Keep `api.js` helpers. Rewrite stores:
- `auth` — token cookie.
- `theme` — backend-persisted mode.
- `dashboard` — status/libraries/markers polling.
- `markers` — episodes + marker CRUD (optimistic).
- `ui` — breadcrumb titles.
Add composables:
- `useAppShell` — breadcrumb trail, navigate, logout.
- `useInfiniteScroll` — reused for series.
- `useSeriesCatalog` — search + infinite scroll.
- `useLibraries` — libraries fetch.
- `useSyncStream` — scan start/cancel + SSE log.
- `useSettings` — interval/Plex config save.

Views import composables/stores; components stay presentational (props/emits only).

### 10. Transitions and responsiveness

`<transition name="page" mode="out-in">` around the routed component. CSS lives in `app.css` (opacity + small translate). `prefers-reduced-motion` disables it. Breakpoint `max-width: 767.98px` collapses all grids to one column; touch targets ≥44px; breadcrumb wraps.

### 11. Testing/QA

- `npm run lint`, `npm run build`, `npm run test:tokens`, `openspec validate redesign-frontend-neutral --strict`.
- Source checks: no `<style` in `src/**/*.vue`; no `.css` imports from views/components; no hex/rgba outside `tokens.css`.
- Playwright E2E against the Docker app: login/logout, breadcrumb path + backtrack, marker CRUD, scan + log, settings + theme, mobile viewport.
- EGM A/S/C review before implementation; independent QA after; max 5 correction iterations.

## Risks / Trade-offs

- Source check for “no per-view CSS” may flag dynamic classes → allow Tailwind utilities and classes from `app.css` only.
- Breadcrumb titles depend on load order → fallback labels until data arrives.
- Full rewrite can break SSE/settings flows → keep composable behavior mirroring current implementation and cover with E2E.
- Icon dependency increases bundle → tree-shaken ESM imports only.

## Migration Plan

1. EGM A/S/C review of this design and specs; update artifacts.
2. Recreate styles/tokens, shell, router, stores, composables, components, views.
3. Remove legacy CSS/components/assets.
4. Run lint/build/tokens + source checks.
5. Update/add E2E specs.
6. Independent QA; iterate up to 5 times.
7. Report PASS/FAIL and files changed.

## Open Questions

- None.
