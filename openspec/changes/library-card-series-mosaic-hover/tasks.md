# Tasks: Library Card Series Mosaic Hover

## Phase 1: Core Implementation

- [x] **Task 1.1**: Create `useLibraryMosaic.js` composable
  - Fetch first 6 series from `/api/series/{libraryKey}`
  - Manage hover state (isHovered)
  - Implement shuffle animation logic on hover
  - Return reactive state: posters, isLoading, error, isHovered, startHover, stopHover
  - Handle caching (per libraryKey)
  - Respect `prefers-reduced-motion`

- [x] **Task 1.2**: Create `LibraryMosaic.vue` component
  - Props: libraryKey, series (optional), fallbackIcon
  - Use `useLibraryMosaic` composable
  - Render 2x3 mosaic grid of posters
  - Implement shuffle animation with CSS transitions
  - Show fallback icon (neutral color) when no series
  - Skeleton loading state
  - Accessibility attributes (aria-label, role)

- [x] **Task 1.3**: Add mosaic CSS styles
  - Mosaic container (aspect-ratio, grid layout)
  - Poster items (object-fit, border-radius, transitions)
  - Shuffle animation keyframes/classes
  - Hover states
  - Fallback icon styling (neutral color)
  - Skeleton placeholder
  - Light/dark theme support

## Phase 2: Integration

- [x] **Task 2.1**: Update `LibrariesView.vue`
  - Import `LibraryMosaic` component
  - Replace `.library-card-icon` span with `<LibraryMosaic :library-key="lib.key" />`
  - Remove old `.library-card-icon` CSS
  - Adjust card layout if needed for mosaic size

- [x] **Task 2.2**: Verify API integration
  - Ensure `/api/series/{key}` returns `thumb` (poster URL)
  - Test with libraries that have series
  - Test with libraries that have no series

## Phase 3: Polish & QA

- [x] **Task 3.1**: Theme validation
  - Verify light theme appearance
  - Verify dark theme appearance
  - Check CSS variable usage (--text-tertiary, --surface, etc.)

- [x] **Task 3.2**: Animation polish
  - Smooth shuffle transitions
  - No layout shift on load
  - Reduced motion support
  - Performance: lazy load poster images

- [x] **Task 3.3**: Accessibility
  - Keyboard navigation works
  - Screen reader labels present
  - Focus visible states

- [x] **Task 3.4**: Build & lint
  - `npm run lint` passes in vue/plex-frontend
  - `npm run build` passes
  - No TypeScript errors (if applicable)

- [x] **Task 3.5**: E2E validation (Playwright)
  - Navigate to /libraries
  - Verify mosaic appears on cards with series
  - Hover card → verify animation triggers
  - Verify fallback icon on empty library
  - Theme toggle → verify colors update
