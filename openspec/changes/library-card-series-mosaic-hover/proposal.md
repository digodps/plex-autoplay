# Proposal: Library Card Series Mosaic Hover

## Summary
Replace static TV icon on library cards with a dynamic mosaic of series posters that animates on hover (shuffle effect inspired by reactbits.dev Shape Magic). Fallback to neutral TV icon when library has no series.

## Motivation
- Current library cards show only a generic TV icon
- Users can't preview content of each library at a glance
- Mosaic hover effect adds delightful interactivity (reactbits.dev inspiration)
- Better visual differentiation between libraries

## Scope
- **In Scope**: LibrariesView.vue library cards, new LibraryMosaic component, useLibraryMosaic composable, hover shuffle animation, fallback state
- **Out of Scope**: Series detail on hover, individual poster clicks, custom animation config, backend changes

## Technical Approach
1. New composable `useLibraryMosaic` fetches 6 series posters via existing `/api/series/{key}` endpoint
2. New component `LibraryMosaic` renders 2x3 grid with CSS Grid
3. On hover: JavaScript interval shuffles poster positions with CSS transitions
4. Fallback: TV icon in `var(--text-tertiary)` when no series
5. Integration: Replace `.library-card-icon` in LibrariesView.vue

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Performance (many image loads) | Lazy load, cache per libraryKey, limit to 6 posters |
| Layout shift | Reserve space with aspect-ratio, skeleton placeholders |
| Reduced motion | Respect `prefers-reduced-motion` media query |
| API errors | Graceful fallback to icon, error boundary in component |

## Alternatives Considered
1. **Carousel instead of shuffle** - Simpler but less "shape magic" feel
2. **Static mosaic (no animation)** - Less engaging, doesn't match reference
3. **Backend endpoint for mosaic** - Unnecessary, existing API sufficient

## Effort Estimate
- Composable: 2h
- Component + CSS: 3h
- Integration: 1h
- Polish/Testing: 2h
- **Total: ~8h**

## Dependencies
- Existing `useLibrarySeries` composable (reference for API patterns)
- Existing design system CSS variables
- Plex API returns `thumb` field for series posters