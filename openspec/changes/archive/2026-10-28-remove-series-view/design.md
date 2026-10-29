# Design

## Context

Current state (see proposal.md for motivation):
- Frontend has separate "Séries" nav item pointing to `/series`
- SeriesView.vue handles `/series` (all series) and `/libraries/:key/series` (library-specific)
- LibrariesView.vue shows library grid, clicking navigates to SeriesView
- Router has routes: `/series`, `/libraries/:key/series`, `/series/:seriesKey/markers`, `/markers` redirect
- Components: SeriesCard, SeriesPagination with their CSS files
- Backend API endpoints at `/api/series/{sectionKey}` and `/api/series/{sectionKey}/{seriesKey}/episodes` are used

## Goals / Non-Goals

**Goals:**
- Remove "Séries" from sidebar navigation
- Remove SeriesView.vue and its routes
- Enhance LibrariesView.vue to show series inline when library is clicked
- Preserve search, pagination, and episode navigation functionality
- Keep backend API endpoints unchanged
- Maintain MarkerEditorView access via `/series/:seriesKey/markers`

**Non-Goals:**
- Change backend API contracts
- Modify authentication/authorization
- Change Plex integration logic
- Modify MarkerEditorView or marker editing flow

## Decisions

### 1. Inline Expansion Pattern for LibrariesView
**Decision**: Use an expandable/collapsible pattern where clicking a library card expands it inline to show series, rather than navigation.

**Rationale**: 
- Preserves context (user sees library grid + selected library's series)
- Simpler than managing separate view state
- Matches common UX patterns (accordion/tree expansion)

**Alternatives considered**:
- Tabs within LibrariesView: Rejected - adds complexity, less intuitive
- Modal/drawer: Rejected - breaks spatial context, harder on mobile
- Keep separate view but merge nav items: Rejected - doesn't solve duplication

### 2. State Management in LibrariesView
**Decision**: Add reactive state for `expandedLibraryKey`, `seriesList`, `seriesPagination`, `seriesQuery`, `seriesLoading`, `seriesError` directly in LibrariesView.vue.

**Rationale**:
- Keeps all series-related state local to the view
- No need for global store (Pinia) since series are scoped to one library at a time
- Simpler than lifting state to App.vue or creating new store

### 3. Route Structure
**Decision**: 
- Remove `/series` route entirely
- Keep `/libraries` as main route
- Keep `/libraries/:key/series` but handle it in LibrariesView (via router param watch)
- Keep `/series/:seriesKey/markers` for MarkerEditorView (navigates from inline series)
- Remove `/markers` redirect

**Rationale**: 
- `/libraries/:key/series` as a route enables deep-linking to a library's series
- MarkerEditor route must stay for editing workflow
- No need for `/series` (global all-series view) per requirements

### 4. Reusing API Endpoints
**Decision**: Continue using existing `/api/series/{sectionKey}` and `/api/series/{sectionKey}/{seriesKey}/episodes` endpoints.

**Rationale**: 
- Endpoints already support pagination, search, filtering
- No backend changes needed
- Frontend just calls them from LibrariesView instead of SeriesView

### 5. Component Cleanup
**Decision**: Delete SeriesCard.vue, SeriesPagination.vue, and their CSS files. Create new inline components or reuse patterns within LibrariesView.

**Rationale**: 
- SeriesCard logic moves into LibrariesView (can be extracted to local component if needed)
- SeriesPagination logic moves into LibrariesView
- Reduces component count and duplication

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Inline expansion may not work well on mobile | Use responsive design: on mobile, expanded view takes full width below library card; collapse other libraries |
| Deep linking to `/libraries/:key/series` must restore expanded state | Watch route param `key` in LibrariesView, auto-expand matching library on mount |
| Search/pagination state lost when collapsing library | Acceptable trade-off for simplicity; or persist in sessionStorage if needed later |
| Large number of series may cause performance issues | Reuse existing pagination (server-side); only load visible page |
| Existing tests for SeriesView will break | Update/remove tests as part of implementation tasks |

## Migration Plan

1. **Phase 1 - Frontend Cleanup**:
   - Remove "Séries" from App.vue navItems
   - Remove series routes from router/index.js
   - Delete SeriesView.vue, SeriesCard.vue, SeriesPagination.vue, CSS files

2. **Phase 2 - LibrariesView Enhancement**:
   - Add series state (expandedLibraryKey, seriesList, pagination, query, loading, error)
   - Add inline series rendering section in template
   - Implement loadSeries(), search, pagination methods (adapted from SeriesView)
   - Add watch on route.params.key for deep-linking

3. **Phase 3 - Navigation Integration**:
   - Update LibrariesView library cards to trigger inline expansion instead of router.push
   - Update click handler on series to navigate to MarkerEditorView

4. **Phase 4 - Cleanup & Polish**:
   - Remove unused imports
   - Update any remaining references to deleted components/routes
   - Test deep-linking, refresh, back/forward navigation

**Rollback**: Revert git commits. No database migrations or backend changes.

## Open Questions

- Should expanded library persist across route changes (e.g., user goes to markers and returns)? Current design: yes, via route param watch.
- Should multiple libraries be expandable simultaneously? Current design: single expansion (accordion pattern) for simplicity.