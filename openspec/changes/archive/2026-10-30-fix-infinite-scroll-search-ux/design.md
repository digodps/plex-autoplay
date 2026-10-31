# Design

## Context

Current state (see proposal.md - Why):
- `useInfiniteScroll.js` uses `IntersectionObserver` with `root: null` (viewport), but sentinel is inside `.series-grid-wrapper` with `overflow-y: auto`. The sentinel never intersects the viewport → `loadMore` never fires.
- `useLibrarySeries.js` manages inline expansion state (`expandedLibraryKey`, `seriesList`, pagination) and search with 300ms debounce.
- `LibrariesView.vue` renders library cards + inline expansion panel (`.library-series-expansion-full`).
- Route `/libraries` with optional `?key=` param for deep-linking inline expansion.
- API `/api/series/{sectionKey}` supports `page`, `pageSize`, `query` params and returns `items`, `totalSize`.

## Goals / Non-Goals

**Goals:**
1. Fix infinite scroll by detecting correct scroll container
2. Verify search works in new full-page context
3. Implement full-page series view at `/libraries/:key/series` with breadcrumb
4. Replace inline expansion with navigation on library click
5. Add E2E test for infinite scroll regression

**Non-Goals:**
- Backend API changes (already supports required params)
- Authentication/authorization changes
- Plex connection logic changes
- Mobile-specific responsive redesign (uses existing breakpoints)

## Decisions

### 1. Infinite Scroll Fix: Auto-detect scroll container

**Decision**: Modify `useInfiniteScroll.js` to find the nearest scrollable ancestor of the sentinel element and use it as the `root` for `IntersectionObserver`.

**Rationale**: 
- Zero config for callers - works regardless of CSS structure
- Handles nested scroll containers correctly
- Matches how browsers determine scrollable areas

**Implementation**:
```js
function findScrollContainer(element) {
  let parent = element.parentElement;
  while (parent) {
    const style = window.getComputedStyle(parent);
    if (/(auto|scroll|overlay)/.test(style.overflowY) && parent.scrollHeight > parent.clientHeight) {
      return parent;
    }
    parent = parent.parentElement;
  }
  return null; // fallback to viewport
}
```

**Alternatives considered**:
- Explicit `root` ref passed by caller: More verbose, error-prone, couples composable to template structure
- Remove scroll container, use page scroll: Would require significant CSS restructuring, affects layout

### 2. Full-Page Route Structure

**Decision**: New route `/libraries/:key/series` with dedicated `SeriesView.vue`. `LibrariesView.vue` loses inline expansion, becomes pure library grid.

**Rationale**:
- Clean separation of concerns: library list vs series catalog
- Natural browser history: back button works, URLs shareable
- Breadcrumb provides explicit wayfinding (requirement)

**Route hierarchy**:
```
/libraries                    → LibrariesView.vue (grid only)
/libraries/:key/series        → SeriesView.vue (full page catalog + breadcrumb)
/series/:seriesKey/markers    → MarkerEditorView.vue (existing)
```

**Alternatives considered**:
- Keep `/libraries?key=X` with full-page conditional render: Couples two views in one component, harder to maintain
- Tab-based approach: Doesn't match "click library → navigate" mental model

### 3. Breadcrumb Component

**Decision**: Create reusable `Breadcrumb.vue` component accepting `items: Array<{ label, to?, current? }>`.

**Rationale**: 
- Reusable for future nested views (seasons, episodes)
- Accessible by default (semantic `<nav>`, `<ol>`, `aria-label`)
- Single source of truth for breadcrumb styling

**Props**:
```ts
interface BreadcrumbItem {
  label: string;
  to?: string | object; // router-link 'to' prop
  current?: boolean;    // last item, non-clickable
}
```

### 4. State Management for SeriesView

**Decision**: Refactor `useLibrarySeries.js` to `useSeriesCatalog.js` (or extend existing) to work for both contexts. The composable is stateless per-library-key; `SeriesView` passes `libraryKey` from route param.

**Key changes**:
- Remove `expandedLibraryKey`, `toggleLibrary` (inline expansion logic)
- Accept `libraryKey` as prop or read from `route.params.key`
- Keep `seriesQuery`, `loadInitialSeries`, `loadNextPage`, infinite scroll integration
- Expose `seriesList`, `hasMore`, `isLoading`, `sentinelRef` for template

**Alternatives considered**:
- Duplicate composable: More code, divergence risk
- Move all logic into `SeriesView.vue`: Less reusable, harder to test

### 5. E2E Test Strategy

**Decision**: New test file `tests/e2e/tests/infinite-scroll.spec.js` using existing mock-plex-server (already provides 50+ series in `mock-plex/server.js`).

**Test flow**:
1. Login → navigate to `/libraries/{key}/series`
2. Wait for initial 24 cards
3. Scroll to bottom (use `page.evaluate(() => window.scrollTo(0, document.body.scrollHeight))` or sentinel `scrollIntoView`)
4. Wait for network idle + new cards
5. Assert card count > 24
6. Continue scrolling until "Fim do catálogo" appears
7. Verify no extra API calls

**Mock data**: `mock-plex/server.js` already returns paginated series. Ensure it has > 50 series for 3 pages.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Auto-detect scroll container fails on dynamic CSS | Add fallback to viewport; log warning in dev; test with current `.series-grid-wrapper` |
| Route change breaks existing deep-links | Document migration; old `?key=` URLs could redirect via router guard |
| Breadcrumb not accessible | Use semantic HTML; test with axe-core in CI |
| Search debounce causes race conditions | `loadInitialSeries` cancels previous request via AbortController (future) or ignores stale responses |
| E2E test flaky due to timing | Use `page.waitForResponse` for API calls; wait for specific card count increase |
| `useLibrarySeries` breaking changes for inline expansion | Inline expansion is being removed - no consumer left after this change |

## Migration Plan

1. **Phase 1** (safe, independent): Fix `useInfiniteScroll.js` scroll container detection. Deploy, verify infinite scroll works in current inline expansion.
2. **Phase 2**: Create `SeriesView.vue`, `Breadcrumb.vue`, add route. Keep `LibrariesView` unchanged initially.
3. **Phase 3**: Update `LibrariesView.vue` to navigate on click (remove inline expansion). Update `useLibrarySeries` → `useSeriesCatalog` for new view.
4. **Phase 4**: Add E2E test. Run full test suite.
5. **Rollback**: Revert router changes and `LibrariesView` if issues; infinite scroll fix is backward compatible.

## Open Questions

1. Should `useLibrarySeries.js` be renamed to `useSeriesCatalog.js` or keep name and remove inline-specific exports? (Lean: rename for clarity)
2. Does the mock Plex server have enough series (>50) for 3-page test? If not, need to extend mock data.
3. Should breadcrumb support dynamic "current" label from route meta or pass title explicitly? (Explicit via prop for simplicity)