# Design: Library Card Series Mosaic Hover

## Architecture

### Components

1. **LibraryMosaic.vue** - New component for mosaic display
   - Props: `libraryKey`, `series` (optional pre-fetched), `fallbackIcon`
   - Emits: none
   - Slots: none

2. **useLibraryMosaic.js** - New composable for mosaic logic
   - Fetches initial series batch (6 items)
   - Manages hover animation state
   - Handles poster rotation/shuffle on hover

### Data Flow

```
LibrariesView.vue
  ├── loads libraries (existing)
  ├── for each library: <LibraryMosaic :library-key="lib.key" />
        ├── LibraryMosaic.vue
        │   ├── uses useLibraryMosaic(libraryKey)
        │   │   ├── fetches /api/series/{key}?page=1&pageSize=6
        │   │   ├── manages hover state (isHovered)
        │   │   ├── cycles posters on hover (setInterval)
        │   │   └── returns { posters, isLoading, error, isHovered }
        │   └── renders mosaic grid or fallback icon
        └── CSS: mosaic grid, hover animations, transitions
```

### API Usage

- Endpoint: `GET /api/series/{libraryKey}?page=1&pageSize=6`
- Response: `{ items: Series[], totalSize: number }`
- Series item needs: `ratingKey`, `thumb` (poster URL), `title`

## UI/UX Design

### Mosaic Layout

```
┌─────────────────────────┐
│  Library Card           │
│  ┌───────────────────┐  │
│  │  Mosaic Area      │  │  ← 2x2 or 2x3 grid
│  │  ┌──┐ ┌──┐        │  │
│  │  │🖼│ │🖼│        │  │
│  │  ├──┼──┼──┐      │  │
│  │  │🖼│ │🖼│ │      │  │
│  │  └──┘ └──┘ │      │  │
│  └───────────────────┘  │
│  Library Title          │
│  "Ver séries" ▶         │
└─────────────────────────┘
```

### Hover Animation

**Option A - Shuffle** (reactbits Shape Magic inspired):
- On hover: posters randomly swap positions every 300ms
- Smooth transition with CSS `transform` + `opacity`
- On hover end: settle to original arrangement

**Option B - Carousel** (simpler):
- On hover: cycle through posters in place (fade cross-fade)
- Show 1 poster at a time in hero position, others as thumbnails

**Option C - Spotlight** (reactbits Spotlight Card inspired):
- Subtle glow/spotlight follows cursor over mosaic
- Posters have slight parallax tilt on hover

**Decision**: Option A (Shuffle) - most dynamic, matches "shape magic" reference

### Fallback State

```
┌─────────────────────────┐
│  Library Card           │
│  ┌───────────────────┐  │
│  │     [TV Icon]     │  │  ← Centered, neutral color
│  │   (var(--text-    │  │
│  │    tertiary))     │  │
│  └───────────────────┘  │
│  Library Title          │
└─────────────────────────┘
```

### CSS Variables Used

- `--radius` - border radius for mosaic container
- `--speed` - transition duration (default 0.2s)
- `--ease` - easing function
- `--text-tertiary` - fallback icon color
- `--surface` - mosaic background
- `--border` - mosaic border
- `--shadow-1/2` - hover shadows

## Implementation Plan

### Phase 1: Composable & Component
1. Create `useLibraryMosaic.js` composable
2. Create `LibraryMosaic.vue` component
3. Add CSS for mosaic grid and animations

### Phase 2: Integration
1. Import and use `LibraryMosaic` in `LibrariesView.vue`
2. Replace `.library-card-icon` with `<LibraryMosaic :library-key="lib.key" />`
3. Remove old icon CSS

### Phase 3: Polish
1. Add skeleton loading state
2. Test light/dark themes
3. Verify accessibility
4. Performance check (lazy loading, caching)

## File Structure

```
vue/plex-frontend/src/
├── composables/
│   └── useLibraryMosaic.js      (NEW)
├── components/
│   └── LibraryMosaic.vue        (NEW)
├── views/
│   └── LibrariesView.vue        (MODIFY)
└── styles/
    └── library-mosaic.css       (NEW or inline in component)
```

## API Contract

### useLibraryMosaic(libraryKey)

**Returns:**
```js
{
  posters: Ref<SeriesPoster[]>,      // Array of { ratingKey, thumb, title }
  isLoading: Ref<boolean>,
  error: Ref<string>,
  isHovered: Ref<boolean>,           // For hover-triggered animation
  startHover: () => void,            // Call on @mouseenter
  stopHover: () => void,             // Call on @mouseleave
}
```

### LibraryMosaic Props

```js
{
  libraryKey: { type: String, required: true },
  series: { type: Array, default: () => [] },  // Pre-fetched (optional)
  fallbackIcon: { type: String, default: 'tv' },
}
```

## Animation Details

### Shuffle Animation (CSS + JS)

```css
.mosaic-poster {
  transition: transform var(--speed) var(--ease), opacity var(--speed) var(--ease);
}

.mosaic-poster.shuffling {
  /* JS applies random transform/opacity during shuffle */
}
```

```js
// On hover: start interval
shuffleInterval = setInterval(() => {
  posters.value.forEach(p => {
    p._shuffleOffset = { x: random(-10, 10), y: random(-10, 10), opacity: random(0.3, 1) }
  })
}, 300)

// On hover end: clear interval, reset offsets
```

### Accessibility

- `aria-label` on mosaic: "Mosaic of series posters for {libraryTitle}"
- `role="img"` on each poster with `aria-label="{seriesTitle}"`
- Keyboard focus: show mosaic static (no animation)
- Reduced motion: respect `prefers-reduced-motion`