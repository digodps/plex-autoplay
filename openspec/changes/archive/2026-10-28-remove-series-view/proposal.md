# Proposal

## Why

The current UI has two separate navigation items and views for "Bibliotecas" (Libraries) and "Séries" (Series), but they serve overlapping purposes. The Libraries view shows a grid of Plex libraries, and clicking one navigates to the Series view for that library. The Series view at `/series` also shows all series across all libraries. This creates redundant navigation paths and maintenance overhead. Removing the separate Series view and merging its functionality into the Libraries view simplifies the UI, reduces code duplication, and provides a more direct workflow: select a library → see its series.

## What Changes

- **Remove** "Séries" navigation item from the sidebar (App.vue)
- **Remove** SeriesView.vue and its associated routes (`/series`, `/libraries/:key/series`, `/series/:seriesKey/markers` redirect)
- **Modify** LibrariesView.vue to display series directly inline when a library is selected (replacing navigation to separate view)
- **Modify** router to remove series routes, keep only `/libraries` and `/libraries/:key/series` (now handled by enhanced LibrariesView)
- **Remove** SeriesCard.vue, SeriesPagination.vue, series.css components
- **Keep** `/api/series/{sectionKey}` and `/api/series/{sectionKey}/{seriesKey}/episodes` backend endpoints (still needed by enhanced LibrariesView)
- **BREAKING**: Frontend route `/series` and `/libraries/:key/series` navigation pattern changes

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `series-catalog`: The requirement to display series in a separate paginated view with search is changing. Series will now be displayed inline within the Libraries view when a library is selected, removing the separate Series page.
- `plex-libraries`: The library discovery capability remains, but the UI interaction changes - clicking a library no longer navigates to a separate view but expands inline to show series.

## Impact

### Frontend (vue/plex-frontend)
- `src/App.vue` - Remove "Séries" from navItems
- `src/router/index.js` - Remove series routes (`/series`, `/libraries/:key/series` as separate route, `/series/:seriesKey/markers`, `/markers` redirect)
- `src/views/LibrariesView.vue` - Major enhancement to show series inline
- `src/views/SeriesView.vue` - **Delete** entire file
- `src/views/SeriesView.css` (series.css) - **Delete**
- `src/components/SeriesCard.vue` - **Delete**
- `src/components/SeriesPagination.vue` - **Delete**
- `src/components/series-card.css` - **Delete**
- `src/components/series-pagination.css` - **Delete**

### Backend (src/PlexAutoplay.Web)
- `Endpoints/SeriesEndpoints.cs` - **Keep** (API endpoints still needed)
- No breaking changes to API contracts

### Navigation Flow (New)
1. User clicks "Bibliotecas" → sees library grid
2. User clicks a library → inline expansion shows series for that library (pagination, search preserved)
3. User clicks a series → navigates to MarkerEditorView (`/series/:seriesKey/markers` route kept)