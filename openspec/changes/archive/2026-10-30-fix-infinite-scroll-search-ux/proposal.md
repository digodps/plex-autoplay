# Proposal

## Why

The series catalog has three critical UX issues blocking user productivity:
1. **Infinite scroll stops at 25 items** - The `IntersectionObserver` sentinel is inside a scrollable container (`.series-grid-wrapper` with `overflow-y: auto`), but the observer watches the viewport (`root: null`), so the sentinel never intersects and `loadMore` never triggers.
2. **Search doesn't filter in real-time** - The debounced watcher exists in `useLibrarySeries.js` but may not trigger correctly due to the infinite scroll bug and state management issues.
3. **Inline expansion UX is confusing** - Clicking a library expands series inline within the same page, requiring a "back" button. Users expect navigation to a full-page series view with breadcrumb for wayfinding.

Additionally, there's **no E2E test** covering infinite scroll, so regressions go undetected.

## What Changes

- **Fix infinite scroll** - Update `useInfiniteScroll.js` to observe the correct scroll container (auto-detect from sentinel's nearest scrollable ancestor) instead of the viewport.
- **Fix search filtering** - Ensure the debounced `seriesQuery` watcher correctly resets pagination and calls the API with the query parameter.
- **NEW: Full-page series view** - Add route `/libraries/:key/series` with `SeriesView.vue` showing series in a full-page grid with breadcrumb navigation (`[← Bibliotecas] Library Title`).
- **Modify LibrariesView** - Remove inline expansion; clicking a library navigates to `/libraries/:key/series` instead of expanding inline.
- **NEW: Breadcrumb component** - Reusable `Breadcrumb.vue` component for wayfinding.
- **NEW: E2E test** - Playwright test that scrolls the series view and verifies more items load (regression guard for infinite scroll).

**BREAKING**: URL structure changes from `/libraries?key=X` (inline) to `/libraries/X/series` (full page). Deep-links to inline expansion will need updating.

## Capabilities

### Modified Capabilities

- `series-infinite-scroll`: The infinite scroll requirement is not being met due to the scroll container bug. The implementation must be fixed to work with scrollable containers.
- `series-catalog`: Search filtering requirement exists but the real-time filtering behavior needs to be verified/fixed in the new full-page view context.

### New Capabilities

- `series-full-page-view`: Full-page series catalog view with breadcrumb navigation, replacing inline expansion. Covers routing, navigation, and wayfinding UX.
- `series-e2e-regression`: E2E test coverage for infinite scroll behavior to prevent regressions.

## Impact

**Frontend (Vue 3)**:
- `vue/plex-frontend/src/composables/useInfiniteScroll.js` - Fix scroll container detection
- `vue/plex-frontend/src/composables/useLibrarySeries.js` - Adapt for full-page view, verify search
- `vue/plex-frontend/src/views/LibrariesView.vue` - Remove inline expansion, add navigation
- `vue/plex-frontend/src/views/SeriesView.vue` - New full-page series view
- `vue/plex-frontend/src/components/Breadcrumb.vue` - New breadcrumb component
- `vue/plex-frontend/src/router/index.js` - Add `/libraries/:key/series` route

**Testing**:
- `tests/e2e/tests/infinite-scroll.spec.js` - New E2E test for infinite scroll regression

**Backend**: No changes needed (API already supports pagination, query, and totalSize).