# Tasks

## 1. Fix Infinite Scroll (Core Bug)

- [x] 1.1 Update `useInfiniteScroll.js` to auto-detect scroll container from sentinel's nearest scrollable ancestor and use it as IntersectionObserver root. Verify: sentinel triggers loadMore when scrolled to bottom in current inline expansion.
- [x] 1.2 Add fallback to viewport if no scroll container found; log warning in development. Verify: no console errors, infinite scroll works in both inline and future full-page contexts.
- [x] 1.3 Test infinite scroll manually: open library, scroll to bottom, verify page 2 loads (24+ cards), scroll again, verify page 3, verify "Fim do catálogo" at end.

## 2. Create Full-Page Series View

- [x] 2.1 Create `vue/plex-frontend/src/components/Breadcrumb.vue` with props `items: Array<{label, to?, current?}>`, semantic `<nav aria-label="Breadcrumb"><ol>`, router-link for non-current items. Verify: component renders correctly in isolation (Storybook or manual test page).
- [x] 2.2 Create `vue/plex-frontend/src/views/SeriesView.vue` with: breadcrumb at top, toolbar (search + count), series grid with sentinel, loading skeletons, error state, empty state. Use `useSeriesCatalog` composable (see 2.3). Verify: page loads at `/libraries/test-key/series`, displays breadcrumb, search works, grid renders.
- [x] 2.3 Create `vue/plex-frontend/src/composables/useSeriesCatalog.js` (refactor from `useLibrarySeries.js`): remove `expandedLibraryKey`, `toggleLibrary`, `openSeriesMarkers`; accept `libraryKey` prop or read from route; keep `seriesQuery`, `loadInitialSeries`, `loadNextPage`, infinite scroll integration, `seriesList`, `hasMore`, `isLoading`, `sentinelRef`. Verify: composable works in SeriesView, search debounce triggers API with query, infinite scroll loads more.
- [x] 2.4 Add route `/libraries/:key/series` in `vue/plex-frontend/src/router/index.js` pointing to `SeriesView.vue`. Verify: direct navigation to URL works, route param `key` available in component.

## 3. Update LibrariesView (Remove Inline Expansion)

- [x] 3.1 Modify `vue/plex-frontend/src/views/LibrariesView.vue`: remove inline expansion panel (`.library-series-expansion-full`), remove `expandedLibraryKey` watch, remove `toggleLibrary` call on card click. Verify: library cards render, no expansion panel.
- [x] 3.2 Update library card click handler to `router.push({ name: 'series', params: { key: lib.key } })` (new route name 'series'). Verify: clicking library navigates to full-page series view.
- [x] 3.3 Remove unused imports from `useLibrarySeries` (keep only `libraries` loading logic). Verify: no console errors, LibrariesView loads cleanly.

## 4. Verify Search Filtering Works

- [x] 4.1 In `SeriesView.vue`, verify search input debounce (300ms) triggers `loadInitialSeries` with query param, resets to page 1. Verify: type in search, wait 300ms, network request includes `query=...`, results filtered.
- [x] 4.2 Verify clearing search restores full catalog (page 1, no query param). Verify: clear input, network request without query, full list returns.
- [x] 4.3 Verify search combines with infinite scroll: search → scroll → page 2 loads with same query. Verify: search term, scroll, network request includes both `page=2` and `query=...`.

## 5. E2E Test for Infinite Scroll Regression

- [x] 5.1 Create `tests/e2e/tests/infinite-scroll.spec.js` with test: login → navigate to `/libraries/{key}/series` → wait for initial cards → scroll to sentinel → wait for API response → assert card count > 24 → continue scroll until "Fim do catálogo" → assert no extra API calls. Verify: test passes locally with mock-plex-server.
- [x] 5.2 Ensure mock-plex-server (`tests/e2e/mock-plex/server.js`) returns ≥ 50 series for pagination testing (3 pages of 24). Verify: mock data has enough series.
- [x] 5.3 Add test to Playwright config if needed. Verify: `npx playwright test tests/e2e/tests/infinite-scroll.spec.js` passes.
- [x] 5.4 Test search + infinite scroll combination in E2E: search term → wait → scroll → verify page 2 has same query param. Verify: test passes.

## 6. Integration Verification

- [x] 6.1 Full manual flow: login → libraries → click library → series view → search → scroll multiple pages → breadcrumb click → back to libraries. Verify: all steps work, no console errors.
- [x] 6.2 Run full test suite: `npx playwright test`. Verify: all existing tests pass + new infinite-scroll test passes.
- [x] 6.3 Build Docker image and verify production build works: `docker build -t plex-autoplay-test . && docker run -p 8443:443 plex-autoplay-test`. Verify: HTTPS on 8443, login works, series view loads.

## Workflow follow-up

- Archive the change after implementation and review are complete.
- Verify archived specs are synced to main specs (`openspec sync`).