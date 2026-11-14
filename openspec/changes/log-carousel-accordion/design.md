# Design: Log Carousel Accordion View

## Architecture

### Component Structure
```
LogsView.vue (page)
├── LogFilterBar.vue (existing - unchanged)
└── LogCarouselView.vue (NEW - replaces LogGroupedView.vue)
    └── LogCard.vue (NEW - individual card component)
        ├── LogCardHeader.vue (collapsed view)
        └── LogCardContent.vue (expanded view)
```

### Data Flow
1. `LogsView` fetches logs via `useLogsStore`
2. `LogCarouselView` receives filtered/grouped logs as props
3. `LogCarouselView` manages expanded state (single open card)
4. `LogCard` receives individual log entry and expanded state
5. `LogCard` parses message for hierarchy (Library/Series/Episode)

## Component Specifications

### LogCarouselView.vue
**Props:**
- `items: LogEntry[]` - filtered log entries
- `groupBy: string` - "date" | "status" | "level" | "library" (new)
- `loading: boolean`

**State:**
- `expandedIndex: number | null` - index of currently expanded card (single open)

**Behavior:**
- Renders `LogCard` for each item with staggered entrance animation
- Handles click to toggle expansion (closes other open card)
- Keyboard navigation between cards (Arrow Up/Down, Home, End)
- Respects `groupBy` for visual grouping separators

### LogCard.vue
**Props:**
- `log: LogEntry`
- `index: number`
- `isExpanded: boolean`
- `onToggle: (index: number) => void`

**Computed:**
- `hierarchy: HierarchyInfo` - parsed from log.message/suggestion
- `levelColor: string` - CSS class for level badge

**Hierarchy Parsing:**
Log messages follow patterns like:
- `"Processing library: Movies"` → library: "Movies"
- `"Processing series: Breaking Bad (tvdb-123)"` → series: "Breaking Bad"
- `"Processing episode: S01E01 - Pilot"` → episode: "S01E01 - Pilot"
- `"Scan completed for library: TV Shows"` → library: "TV Shows"

Parse using regex patterns for common formats.

### LogCardHeader.vue (Collapsed)
- Timestamp (formatted time)
- Level badge (colored dot + label)
- Job status badge (if available)
- Message preview (truncated to 80 chars)
- Chevron icon (animated rotation)
- Hover: subtle shadow increase

### LogCardContent.vue (Expanded)
- Full message
- Suggestion (if present, highlighted)
- JobId (monospace, copyable)
- Hierarchy tree: Library → Series → Episode (when parsed)
- Timestamp (full ISO)
- Level

## Styling

### Card Base
```css
.log-card {
  @apply relative rounded-xl border border-gray-800 bg-[#1f2937] shadow-xl 
         transition-all duration-300 ease-out
         hover:shadow-2xl hover:-translate-y-0.5;
}
```

### Floating Effect
```css
.log-card::before {
  content: '';
  @apply absolute inset-0 rounded-xl bg-gradient-to-b from-transparent to-black/20 opacity-0
         transition-opacity duration-300;
}
.log-card:hover::before {
  @apply opacity-100;
}
```

### Animation
- Entrance: `animate-in fade-in slide-in-from-bottom-4 duration-300` with stagger delay
- Expand: `animate-in expand-height duration-300 ease-out`
- Collapse: `animate-out collapse-height duration-200 ease-in`
- Chevron: `transition-transform duration-200` rotate-180 when expanded

### Theme Colors (CSS Variables)
```css
:root {
  --card-bg: #1f2937;
  --card-border: #374151;
  --card-hover-shadow: 0 25px 50px -12px rgb(0 0 0 / 0.5);
}
.dark {
  --card-bg: #1f2937;
  --card-border: #374151;
}
```

## Accessibility (ARIA)

### Accordion Pattern
```html
<div class="log-card" role="region" aria-labelledby="card-heading-{index}">
  <header id="card-heading-{index}">
    <button 
      aria-expanded="true/false" 
      aria-controls="card-content-{index}"
      id="card-button-{index}"
    >
      <!-- header content -->
    </button>
  </header>
  <div 
    id="card-content-{index}" 
    role="region" 
    aria-labelledby="card-heading-{index}"
    hidden="!isExpanded"
  >
    <!-- expanded content -->
  </div>
</div>
```

### Keyboard Navigation
- Tab: Move between card headers
- Enter/Space: Toggle expansion
- Arrow Down/Up: Move to next/previous card header
- Home: First card
- End: Last card
- Escape: Collapse expanded card

## Integration Points

### LogsView.vue Changes
- Replace `<LogGroupedView />` with `<LogCarouselView />`
- Pass same props: `:items`, `:group-by`, `:loading`

### useLogsStore.js Changes
- Add new `groupBy` option: `"library"` 
- Add computed `parsedLogs` that adds hierarchy info to each log entry

### API Enhancement (Optional)
- Backend could return structured hierarchy data
- For now, parse from message strings on frontend

## Responsive Behavior

### Mobile (< 640px)
- Cards full width
- Reduced padding
- Stack hierarchy vertically
- Touch-friendly tap targets (min 44px)

### Desktop (≥ 1024px)
- Max width container (max-w-6xl)
- Cards with comfortable spacing
- Hover effects enabled

## Animation Performance

### CSS-only Animations
- Use `transform` and `opacity` for animations (GPU accelerated)
- Avoid animating `height` directly - use `max-height` with CSS grid trick
- `will-change: transform, opacity` on cards during animation

### Staggered Entrance
```css
.log-card:nth-child(1) { animation-delay: 0ms; }
.log-card:nth-child(2) { animation-delay: 50ms; }
.log-card:nth-child(3) { animation-delay: 100ms; }
/* ... up to 20 */
```

## Testing Strategy

### Unit Tests
- Hierarchy parsing logic
- Level color mapping
- Group by logic

### E2E Tests (Playwright)
- Card renders for each log
- Click expands/collapses
- Only one card open at a time
- Filters update card list
- Theme toggle updates card colors
- Keyboard navigation works
- Mobile responsive layout