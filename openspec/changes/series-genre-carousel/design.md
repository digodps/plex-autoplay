# Design

## Context

Series catalog displays shows from Plex libraries. The backend now returns `genre` for each series (populated from Plex API's `Genre[0].tag`). The current flat grid makes discovery difficult for large libraries. Modern streaming apps (Netflix, Crunchyroll) use genre-grouped horizontal carousels.

## Goals / Non-Goals

**Goals:**
- Netflix-style horizontal carousels grouped by genre
- Smooth horizontal scroll with left/right navigation buttons
- Hover/focus scale animation on cards (scale + elevation + shadow)
- Mouse wheel horizontal scrolling
- Genre section headers with series count
- "Outros" section for series without genre
- Fully responsive, dark/light theme compatible

**Non-Goals:**
- Backend API changes (already done)
- Changes to infinite scroll (fetch all series at once for grouping)
- Changes to library loading or sync logic

## Decisions

1. **Data fetching**: Fetch all series in one request (`pageSize=1000`) and group client-side by genre. This avoids multiple paginated requests and simplifies grouping logic.

2. **Composable**: New `useSeriesByGenre` composable encapsulates fetch + group logic, returns reactive `genreGroups` array.

3. **Component**: `GenreCarousel.vue` renders sections, handles scroll state, keyboard accessibility, and hover animations.

3. **Card hover animation**: 
   - Transform: `scale(1.08) translateY(-4px)`
   - Shadow: `--shadow-2` (elevated)
   - Poster image: `scale(1.05)`
   - Z-index boost to overlap neighbors

4. **Scroll buttons**: 
   - Appear only on hover of carousel wrapper
   - Fade gradient background (`--surface` to transparent)
   - Only show when scrollable in that direction
   - Scroll by 80% of viewport width per click

5. **Genre ordering**: Alphabetical, "Outros" last.

6. **Skeleton loading**: Per-genre-section skeleton with placeholder cards.

## Risks / Trade-offs

- **Large libraries**: Fetching all series at once (pageSize=1000) works for typical Plex libraries. If library exceeds 1000 shows, only first 1000 are grouped. Acceptable for MVP; can add pagination-aware grouping later.

- **Image aspect ratio**: Cards use 2/3 aspect ratio posters; some cover art may crop. Mitigated by `object-fit: cover`.

- **Horizontal scroll on mobile**: Touch drag works natively; scroll buttons provide desktop fallback.

- **Performance**: Reactive scroll state updates on scroll event; throttled by browser native scroll.