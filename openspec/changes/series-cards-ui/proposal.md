# Proposal

## Why

The current series catalog cards use a basic vertical layout with title and "Ver marcadores" link. To achieve a polished, modern anime/streaming platform aesthetic (dark theme, crisp badges, prominent cover art with 2/3 aspect ratio, subtle overlays, and elegant typography), the series cards need a UI redesign.

## What Changes

- Redesign series catalog cards in `vue/plex-frontend/src/views/Series/SeriesView.vue` to match modern anime/streaming card design: rounded corners, clean aspect ratio, badge overlay support, clear typography, and refined hover/focus states.
- Ensure dark theme compatibility and CSS variable usage (`tokens.css`).
- Maintain all existing functionality (infinite scroll, lazy loading, click navigation to markers).

## Capabilities

### New Capabilities
- `series-cards-ui`: Redesigned anime/streaming style series cards with polished layout, badges, cover art proportions, and responsive grid.

### Modified Capabilities
- `series-full-page-view`: Update requirements for series card presentation and hover/focus styling.

## Impact

**Frontend (`vue/plex-frontend`):**
- `src/views/Series/SeriesView.vue`: updated series card markup and classes.
- No backend changes required.
