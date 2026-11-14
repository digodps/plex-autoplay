# Tasks

## 1. Create useSeriesByGenre composable

- [x] 1.1 Create `src/composables/useSeriesByGenre.js` that fetches all series (pageSize=1000) and groups them by genre. **Verify:** Returns reactive `genreGroups` array with genre title and series array.

## 2. Create GenreCarousel component

- [x] 2.1 Create `src/components/GenreCarousel.vue` with horizontal scroll track, left/right buttons, and genre section headers. **Verify:** Renders correctly with prop data.
- [x] 2.2 Implement scroll state tracking (reactive) for button visibility. **Verify:** Buttons appear only when scrollable in that direction.
- [x] 2.3 Implement hover scale animation on cards (transform, shadow, poster zoom). **Verify:** Cards animate on hover and focus.
- [x] 2.4 Implement mouse wheel horizontal scrolling. **Verify:** Mouse wheel scrolls carousels horizontally.
- [x] 2.5 Implement keyboard accessibility (focus visible, Enter navigation). **Verify:** Tab navigation and Enter key work.

## 3. Update SeriesView

- [x] 3.1 Update `src/views/SeriesView.vue` to use `useSeriesByGenre` and `GenreCarousel`. **Verify:** Page loads with genre carousels instead of flat grid.
- [x] 3.2 Simplify `src/views/SeriesView.css` to only toolbar styles. **Verify:** No leftover grid styles.
- [x] 3.3 Remove search input functionality (search happens at top-level if needed) or keep disabled. **Verify:** Search input shows as disabled with hint.

## 4. Validate Build and QA

- [ ] 4.1 Build frontend (`npm run build`) and backend (`dotnet build`). **Verify:** Both pass without errors.
- [ ] 4.2 Validate dark/light theme rendering. **Verify:** No hardcoded colors, all tokens respected.
- [ ] 4.3 Validate Playwright UI tests (theme toggle, menu navigation). **Verify:** Tests pass.