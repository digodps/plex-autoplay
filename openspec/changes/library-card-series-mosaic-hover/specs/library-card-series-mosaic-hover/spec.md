# Spec: Library Card Series Mosaic Hover

## Overview
Replace the static TV icon on library cards with a dynamic mosaic of series posters that animates/changes on hover. When a library has no series, show the TV icon in a neutral/muted color.

## Requirements

### Functional Requirements

1. **Mosaic Display**: Each library card shows a mosaic grid of series posters (up to 4-6 posters) instead of the static TV icon
2. **Hover Animation**: When user hovers over the library card, the mosaic posters cycle/animate (like a slideshow or shuffle effect)
3. **Fallback**: When library has no series, display the TV icon in a neutral/muted color (var(--text-tertiary) or similar)
4. **Data Source**: Fetch series data from existing `/api/series/{libraryKey}` endpoint (first page, 6 items)
5. **Performance**: Lazy load posters, cache results, don't block initial render

### Visual/UX Requirements

1. **Mosaic Layout**: 2x2 or 2x3 grid of small poster thumbnails
2. **Hover Effect**: Smooth transition between posters (fade, slide, or shuffle) - inspired by reactbits.dev Shape Magic / Spotlight Card hover effects
3. **Neutral Icon**: TV icon in var(--text-tertiary) when no series available
4. **Aspect Ratio**: Maintain poster aspect ratio (2:3 typical for TV posters)
5. **Responsive**: Mosaic adapts to card size

### Technical Requirements

1. **Reuse Existing Composables**: Leverage `useLibrarySeries` for fetching
2. **New Composable**: Create `useLibraryMosaic` for mosaic-specific logic (hover animation, poster selection)
3. **Component**: Create `LibraryMosaic.vue` component for the mosaic display
4. **Integration**: Replace `.library-card-icon` in LibrariesView with new component
5. **CSS Variables**: Use existing design system variables (--radius, --speed, --ease, --text-tertiary, etc.)

## Acceptance Criteria

- [ ] Library cards show mosaic of series posters (max 6) instead of TV icon
- [ ] Hovering card triggers smooth poster animation/cycling
- [ ] Libraries with no series show TV icon in neutral color
- [ ] Initial page load not blocked by poster fetching
- [ ] Posters lazy-loaded with placeholder/skeleton
- [ ] Works in both light and dark themes
- [ ] No layout shift when posters load
- [ ] Accessible: keyboard focus shows mosaic, screen readers get proper labels

## Out of Scope

- Full series detail on hover (just visual mosaic)
- Click interaction on individual posters (card click still navigates to series view)
- Custom animation configuration (use sensible defaults)