# Proposal

## Why

The current series catalog displays all series in a single paginated grid. For libraries with many series across different genres (Action, Comedy, Drama, Anime, etc.), users must scroll through mixed content to find shows they like. Modern streaming platforms (Netflix, Crunchyroll, Disney+) group content by genre in horizontal carousels, making discovery intuitive and visually appealing.

The backend already supports genre metadata from Plex API (added in previous change). This change leverages that data to create a Netflix-style genre carousel UI.

## What Changes

- Create a new composable `useSeriesByGenre` to fetch all series and group them by genre.
- Create a new `GenreCarousel` component with horizontal scrolling carousels per genre.
- Update `SeriesView` to use the genre carousel instead of the flat grid.
- Add smooth horizontal scroll with scroll buttons, hover scale animations, and keyboard accessibility.
- Maintain dark/light theme compatibility using CSS variables from `style.css`.

## Capabilities

### New Capabilities

- `series-genre-carousel`: Netflix-style horizontal carousels grouping series by genre, with smooth scroll, hover animations, and genre section headers.

### Modified Capabilities

- `series-full-page-view`: Replace flat grid with genre-grouped carousels on `/libraries/{key}/series`.

## Impact

**Frontend (`vue/plex-frontend`):**
- New file: `src/composables/useSeriesByGenre.js` - fetches and groups series by genre
- New file: `src/components/GenreCarousel.vue` - renders horizontal carousels per genre
- Modified: `src/views/SeriesView.vue` - uses new composable and component
- Modified: `src/views/SeriesView.css` - simplified toolbar styles only

**Backend:** No changes required (genre already exposed in `/api/series/{key}` endpoint).