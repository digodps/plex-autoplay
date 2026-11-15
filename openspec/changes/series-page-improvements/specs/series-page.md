# Series Page Specification

## Overview

The series page provides a comprehensive view for browsing and playing TV series episodes with auto-play functionality.

## Functional Requirements

### FR-1: Auto-Play Next Episode
- When an episode ends, automatically load and play the next episode in the series
- Display a 5-second countdown overlay before auto-playing
- Allow user to cancel auto-play during countdown
- Respect user preference for auto-play (on/off setting)

### FR-2: Episode Navigation
- Previous/Next episode buttons in player controls
- Episode list with click-to-play functionality
- Keyboard shortcuts: Left Arrow (previous), Right Arrow (next)
- Visual indication of current episode in list

### FR-3: Watch Status Indicators
- Watched episodes: checkmark icon, dimmed appearance
- Unwatched episodes: play icon, normal brightness
- Currently playing: pulsing indicator
- Partially watched: progress ring indicator

### FR-4: Responsive Layout
- Desktop: Side-by-side episode list and player (70/30 split)
- Tablet: Collapsible episode list drawer, full-width player
- Mobile: Full-screen player, episode list in bottom sheet
- Breakpoints: 1024px (desktop), 768px (tablet), <768px (mobile)

### FR-5: Keyboard Shortcuts
- Space: Play/Pause
- Left Arrow: Previous episode
- Right Arrow: Next episode
- M: Mute/Unmute
- F: Fullscreen toggle
- Esc: Exit fullscreen, close drawers

## Non-Functional Requirements

### NFR-1: Performance
- Episode list loads in <500ms
- Auto-play transition <2 seconds
- Smooth 60fps animations

### NFR-2: Accessibility
- WCAG 2.1 AA compliance
- Focus indicators on all interactive elements
- Screen reader announcements for auto-play countdown
- Keyboard navigable episode list

### NFR-3: Browser Support
- Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- Progressive enhancement for older browsers

## Data Models

### SeriesEpisode
```typescript
interface SeriesEpisode {
  id: string;
  seriesId: string;
  seasonNumber: number;
  episodeNumber: number;
  title: string;
  summary: string;
  thumb: string;
  duration: number; // seconds
  watched: boolean;
  watchProgress: number; // 0-100
  addedAt: string; // ISO 8601
}
```

### SeriesPageState
```typescript
interface SeriesPageState {
  seriesId: string;
  episodes: SeriesEpisode[];
  currentEpisodeId: string | null;
  autoPlayEnabled: boolean;
  viewMode: 'desktop' | 'tablet' | 'mobile';
}
```

## API Endpoints

### GET /api/series/{seriesId}/episodes
Returns all episodes for a series ordered by season/episode number.

### POST /api/series/{seriesId}/episodes/{episodeId}/progress
Updates watch progress for an episode.

### GET /api/user/preferences/autoplay
Returns user's auto-play preference.

### PUT /api/user/preferences/autoplay
Updates user's auto-play preference.
