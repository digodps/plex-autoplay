# Tasks

## 1. Frontend Cleanup - Remove Series Navigation & Routes

- [x] 1.1 Remove "Séries" nav item from `vue/plex-frontend/src/App.vue` navItems array and verify sidebar no longer shows "Séries" link
- [x] 1.2 Remove series routes from `vue/plex-frontend/src/router/index.js`:
  - Remove `/series` (series-index)
  - Remove `/libraries/:key/series` (series) 
  - Remove `/series/:seriesKey/markers` (series-markers) - **WAIT**: Keep this for MarkerEditorView access
  - Remove `/markers` redirect
  - Verify router only has: `/`, `/login`, `/dashboard`, `/libraries`, `/sync`, `/settings`
- [x] 1.3 Delete `vue/plex-frontend/src/views/SeriesView.vue` and verify no import errors
- [x] 1.4 Delete `vue/plex-frontend/src/views/series.css` and verify no import errors
- [x] 1.5 Delete `vue/plex-frontend/src/components/SeriesCard.vue` and `vue/plex-frontend/src/components/series-card.css`
- [x] 1.6 Delete `vue/plex-frontend/src/components/SeriesPagination.vue` and `vue/plex-frontend/src/components/series-pagination.css`
- [x] 1.7 Run `npm run build` in `vue/plex-frontend` and verify successful compilation with no missing component errors

## 2. Enhance LibrariesView for Inline Series Display

- [x] 2.1 Add series state to `vue/plex-frontend/src/views/LibrariesView.vue`:
  - `expandedLibraryKey` (string | null) - tracks which library is expanded
  - `seriesList` (array) - series for expanded library
  - `seriesTotalSize` (number) - total series count for pagination
  - `seriesPage` (number) - current page
  - `seriesPageSize` (number) - page size (24)
  - `seriesQuery` (string) - search query
  - `seriesLoading` (boolean)
  - `seriesError` (string)
- [x] 2.2 Add `loadSeries(libraryKey)` method adapted from SeriesView:
  - Call `/api/series/${libraryKey}?page=${page}&pageSize=${pageSize}&query=${query}`
  - Handle pagination, search, loading, error states
  - Reset page to 1 when query or library changes
- [x] 2.3 Add `toggleLibrary(libraryKey)` method:
  - If same library clicked: collapse (set expandedLibraryKey = null)
  - If different library: expand new one, load its series
- [x] 2.4 Add search input with debounce (300ms) for series filtering within expanded library
- [x] 2.5 Add pagination controls (adapted from SeriesPagination) for series list
- [x] 2.6 Add inline series rendering in template:
  - When `expandedLibraryKey === lib.key`, render series grid below library card
  - Use existing SeriesCard markup/styling patterns inline
  - Each series card navigates to MarkerEditorView via router.push({ name: 'series-markers', params: { seriesKey: series.ratingKey } })
- [x] 2.7 Add watch on `route.params.key` for deep-linking:
  - On mount, if route has `key` param, auto-expand that library
  - Update expandedLibraryKey when route param changes
- [x] 2.8 Update library card click handler: replace router.push with `toggleLibrary(lib.key)`
- [x] 2.9 Run `npm run build` and verify no errors; test inline expansion, search, pagination manually

## 3. Preserve MarkerEditorView Access Route

- [x] 3.1 Verify `/series/:seriesKey/markers` route still exists in router (named `series-markers`)
- [x] 3.2 Update series click handler in LibrariesView to navigate to MarkerEditorView using existing route name
- [x] 3.3 Verify MarkerEditorView still works end-to-end: library → series → markers → edit → save

## 4. Backend - No Changes Required (Verification Only)

- [x] 4.1 Verify `/api/series/{sectionKey}` endpoint still works (used by enhanced LibrariesView)
- [x] 4.2 Verify `/api/series/{sectionKey}/{seriesKey}/episodes` endpoint still works (used by MarkerEditorView)
- [x] 4.3 Run backend tests (if any) and verify no regressions

## 5. Integration Testing & Polish

- [x] 5.1 Test full user flow: Login → Bibliotecas → click library → see series inline → search → paginate → click series → edit markers → return
- [x] 5.2 Test deep-linking: direct URL to `/libraries/{key}/series` expands correct library
- [x] 5.3 Test browser back/forward navigation preserves expanded state
- [x] 5.4 Test mobile responsiveness: expanded series view works on small screens
- [x] 5.5 Test refresh on expanded library preserves state
- [x] 5.6 Run full build: `cd vue/plex-frontend && npm run build` and `docker compose build` - verify both succeed
- [x] 5.7 Deploy and smoke test: `docker compose up -d` → access https://localhost:8443 → verify full flow

## Workflow follow-up

- Archive the change after the project's review requirements are satisfied
- Verify the archived result