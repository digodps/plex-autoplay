# Proposal: Series Page Improvements

## Problem Statement

The series page needs improvements to enhance user experience and functionality. Current implementation lacks:
- Auto-play functionality for series episodes
- Improved navigation between episodes
- Better visual indicators for watched/unwatched episodes
- Responsive design improvements for mobile/tablet

## Goals

1. Implement auto-play next episode functionality
2. Add episode navigation controls (previous/next)
3. Add visual watched/unwatched indicators
4. Improve responsive layout for all screen sizes
5. Add keyboard shortcuts for navigation

## Scope

- Frontend: Vue 3 components in `vue/plex-frontend/src/components/series/`
- Backend: API endpoints in `src/PlexAutoplay.Web/Controllers/SeriesController.cs`
- Types: Shared types in `src/PlexAutoplay.Shared/Series/`

## Out of Scope

- Authentication changes
- Database schema changes
- Plex server integration changes

## Success Criteria

- Auto-play works seamlessly between episodes
- Navigation controls are accessible and responsive
- Visual indicators clearly show watch status
- All components work on mobile, tablet, desktop
- Keyboard shortcuts functional (Space=play/pause, Left/Right=prev/next)
