# Series Page Improvements - Technical Design

## Architecture Overview

Following the existing Vue 3 + .NET Clean Architecture pattern:
- Frontend: Vue 3 Composition API with TypeScript, Pinia for state
- Backend: ASP.NET Core Controllers with MediatR for CQRS
- Shared: TypeScript interfaces in `src/PlexAutoplay.Shared/`

## Frontend Components

### SeriesPage.vue (Main Layout)
- Responsive grid layout using CSS Grid
- Desktop: `grid-template-columns: 1fr 300px` (player | episode-list)
- Tablet: Player full width, episode list in slide-over drawer
- Mobile: Player full width, episode list in bottom sheet

### EpisodeList.vue
- Virtual scrolling for performance (vue-virtual-scroller)
- EpisodeItem component for each episode
- Highlight current episode with accent border
- Watch status icons: ✓ (watched), ▶ (unwatched), ⟳ (in-progress), ◷ (partial)

### VideoPlayer.vue (Enhanced)
- Auto-play countdown overlay component
- Keyboard event handlers for shortcuts
- Integration with existing Plex player API
- Event emits: `episode-ended`, `auto-play-cancelled`

### AutoPlayCountdown.vue
- 5-second circular progress timer
- Cancel button with keyboard support (Esc)
- Screen reader announcements via aria-live

### EpisodeNavigation.vue
- Previous/Next buttons with disabled states
- Keyboard shortcut hints (tooltips)
- Integrates with player controls

## State Management (Pinia)

### useSeriesStore.ts
```typescript
interface SeriesState {
  seriesId: string | null;
  episodes: SeriesEpisode[];
  currentEpisode: SeriesEpisode | null;
  autoPlayEnabled: boolean;
  viewMode: ViewMode;
  loading: boolean;
  error: string | null;
}
```

Actions:
- `fetchEpisodes(seriesId)` - Load episodes from API
- `setCurrentEpisode(episode)` - Update current playing
- `toggleAutoPlay()` - Toggle user preference
- `updateWatchProgress(episodeId, progress)` - Sync with backend
- `setViewMode(mode)` - Handle responsive breakpoints

## Backend API

### SeriesController.cs
```csharp
[HttpGet("{seriesId}/episodes")]
public async Task<ActionResult<List<SeriesEpisodeDto>>> GetEpisodes(string seriesId)

[HttpPost("{seriesId}/episodes/{episodeId}/progress")]
public async Task<IActionResult> UpdateProgress(string seriesId, string episodeId, [FromBody] WatchProgressDto dto)

[HttpGet("user/preferences/autoplay")]
public async Task<ActionResult<bool>> GetAutoPlayPreference()

[HttpPut("user/preferences/autoplay")]
public async Task<IActionResult> SetAutoPlayPreference([FromBody] bool enabled)
```

### MediatR Handlers
- `GetSeriesEpisodesQuery` → `SeriesEpisodesDto`
- `UpdateWatchProgressCommand` → `Unit`
- `GetAutoPlayPreferenceQuery` → `bool`
- `SetAutoPlayPreferenceCommand` → `Unit`

## Data Access

### SeriesEpisode Entity (EF Core)
```csharp
public class SeriesEpisode
{
    public string Id { get; set; }
    public string SeriesId { get; set; }
    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string ThumbUrl { get; set; }
    public int DurationSeconds { get; set; }
    public bool Watched { get; set; }
    public int WatchProgress { get; set; } // 0-100
    public DateTime AddedAt { get; set; }
}
```

## Responsive Breakpoints (CSS)

```css
/* Desktop: >= 1024px */
@media (min-width: 1024px) { .series-page { grid-template-columns: 1fr 300px; } }

/* Tablet: 768px - 1023px */
@media (max-width: 1023px) and (min-width: 768px) { 
  .episode-list { position: fixed; right: 0; width: 320px; transform: translateX(100%); }
  .episode-list.open { transform: translateX(0); }
}

/* Mobile: < 768px */
@media (max-width: 767px) {
  .episode-list { position: fixed; bottom: 0; height: 50%; transform: translateY(100%); }
  .episode-list.open { transform: translateY(0); }
}
```

## Auto-Play Logic

```typescript
// In VideoPlayer.vue
const handleEpisodeEnded = async () => {
  if (!autoPlayEnabled.value) return;
  
  const nextEpisode = getNextEpisode(currentEpisode.value);
  if (!nextEpisode) return;
  
  showCountdown.value = true;
  await countdown(5000); // 5 seconds
  
  if (!cancelled.value) {
    await playEpisode(nextEpisode);
  }
  showCountdown.value = false;
};
```

## Accessibility

- ARIA labels on all controls
- `aria-live="polite"` for countdown announcements
- Focus trap in episode list drawer/sheet
- High contrast mode support via CSS custom properties
- Reduced motion support via `prefers-reduced-motion`

## Testing Strategy

- Unit: Pinia store actions, auto-play logic, responsive hooks
- Integration: API endpoints with TestServer
- E2E: Playwright tests for auto-play flow, navigation, responsive layouts
