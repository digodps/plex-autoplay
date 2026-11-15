# Tasks

## 1. Backend API Implementation

- [ ] 1.1 Create SeriesEpisode entity in `src/PlexAutoplay.Domain/Series/SeriesEpisode.cs`
- [ ] 1.2 Add SeriesEpisode DbContext configuration in `src/PlexAutoplay.Infrastructure/Data/Configurations/SeriesEpisodeConfiguration.cs`
- [ ] 1.3 Create SeriesEpisodesDto in `src/PlexAutoplay.Shared/Series/SeriesEpisodesDto.cs`
- [ ] 1.4 Create GetSeriesEpisodesQuery/Handler in `src/PlexAutoplay.Application/Series/Queries/GetSeriesEpisodes/`
- [ ] 1.5 Create UpdateWatchProgressCommand/Handler in `src/PlexAutoplay.Application/Series/Commands/UpdateWatchProgress/`
- [ ] 1.6 Create GetAutoPlayPreferenceQuery/Handler in `src/PlexAutoplay.Application/UserPreferences/Queries/GetAutoPlayPreference/`
- [ ] 1.7 Create SetAutoPlayPreferenceCommand/Handler in `src/PlexAutoplay.Application/UserPreferences/Commands/SetAutoPlayPreference/`
- [ ] 1.8 Add endpoints to SeriesController in `src/PlexAutoplay.Web/Controllers/SeriesController.cs`
- [ ] 1.9 Add UserPreferencesController endpoints for auto-play preference
- [ ] 1.10 Write unit tests for all handlers in `tests/PlexAutoplay.Application.Tests/Series/`

## 2. Frontend State Management

- [ ] 2.1 Create SeriesEpisode type in `vue/plex-frontend/src/types/series.ts`
- [ ] 2.2 Create useSeriesStore in `vue/plex-frontend/src/stores/series.ts`
- [ ] 2.3 Implement fetchEpisodes action with API integration
- [ ] 2.4 Implement setCurrentEpisode action
- [ ] 2.5 Implement toggleAutoPlay action with persistence
- [ ] 2.6 Implement updateWatchProgress action
- [ ] 2.7 Implement setViewMode action with breakpoint detection
- [ ] 2.8 Write unit tests for store in `vue/plex-frontend/src/stores/__tests__/series.test.ts`

## 3. Frontend Components - Series Page Layout

- [ ] 3.1 Create SeriesPage.vue main layout component
- [ ] 3.2 Implement responsive CSS Grid layout (desktop/tablet/mobile)
- [ ] 3.3 Add breakpoint detection composable `useBreakpoint.ts`
- [ ] 3.4 Create EpisodeList.vue with virtual scrolling
- [ ] 3.5 Create EpisodeItem.vue with watch status indicators
- [ ] 3.6 Add drawer/sheet components for tablet/mobile episode list
- [ ] 3.7 Style components with Tailwind CSS matching design system

## 4. Frontend Components - Video Player Enhancements

- [ ] 4.1 Enhance VideoPlayer.vue with auto-play integration
- [ ] 4.2 Create AutoPlayCountdown.vue component
- [ ] 4.3 Implement 5-second countdown with circular progress
- [ ] 4.4 Add cancel functionality (button + Esc key)
- [ ] 4.5 Add screen reader announcements (aria-live)
- [ ] 4.6 Create EpisodeNavigation.vue component
- [ ] 4.7 Implement keyboard shortcuts (Space, Arrows, M, F, Esc)
- [ ] 4.8 Add focus management for accessibility

## 5. Auto-Play Logic

- [ ] 5.1 Implement getNextEpisode utility in `vue/plex-frontend/src/utils/series.ts`
- [ ] 5.2 Implement auto-play trigger on episode ended event
- [ ] 5.3 Integrate countdown overlay with player
- [ ] 5.4 Handle auto-play cancellation
- [ ] 5.5 Persist auto-play preference to backend
- [ ] 5.6 Load auto-play preference on app initialization

## 6. Watch Status Indicators

- [ ] 6.1 Add watched/unwatched/in-progress/partial icons to EpisodeItem
- [ ] 6.2 Implement progress ring for partially watched episodes
- [ ] 6.3 Add pulsing animation for currently playing episode
- [ ] 6.4 Sync watch status with backend on progress updates
- [ ] 6.5 Add visual feedback for watch completion

## 7. Responsive Layout Polish

- [ ] 7.1 Test and fix desktop layout (>=1024px)
- [ ] 7.2 Test and fix tablet layout (768px-1023px) with slide-over drawer
- [ ] 7.3 Test and fix mobile layout (<768px) with bottom sheet
- [ ] 7.4 Ensure smooth transitions between breakpoints
- [ ] 7.5 Test touch interactions on mobile/tablet
- [ ] 7.6 Verify theme (light/dark) consistency across layouts

## 8. Accessibility

- [ ] 8.1 Add ARIA labels to all interactive elements
- [ ] 8.2 Implement focus trapping in drawer/sheet
- [ ] 8.3 Add keyboard navigation for episode list
- [ ] 8.4 Test with screen reader (NVDA/VoiceOver)
- [ ] 8.5 Verify color contrast ratios (WCAG AA)
- [ ] 8.6 Add reduced motion support

## 9. Testing

- [ ] 9.1 Write component tests for SeriesPage, EpisodeList, EpisodeItem
- [ ] 9.2 Write component tests for VideoPlayer, AutoPlayCountdown, EpisodeNavigation
- [ ] 9.3 Write integration tests for API endpoints
- [ ] 9.4 Write E2E tests for auto-play flow (Playwright)
- [ ] 9.5 Write E2E tests for responsive layouts (Playwright)
- [ ] 9.6 Write E2E tests for keyboard shortcuts (Playwright)
- [ ] 9.7 Run lint: `npm run lint` in `vue/plex-frontend`
- [ ] 9.8 Run typecheck: `npm run typecheck` in `vue/plex-frontend`
- [ ] 9.9 Run backend build: `dotnet build PlexAutoplay.slnx -c Release`

## 10. Documentation

- [ ] 10.1 Update API documentation (OpenAPI/Swagger)
- [ ] 10.2 Add component usage examples in Storybook
- [ ] 10.3 Update CHANGELOG.md
