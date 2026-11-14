# Spec: Log Carousel Accordion View

## Overview
Transform the current log view from a grouped list into a modern card-based accordion carousel view, similar to the "Questions, answered" section on reactbits.dev. Each log entry becomes a floating card that can be expanded to show details about which library/series/episode was processed.

## Requirements

### Functional
1. **Card-based layout**: Replace the current grouped table/list view with floating cards
2. **Accordion expansion**: Click a card to expand/collapse and show detailed log information
3. **Hierarchical grouping**: Group logs by Library → Series → Episode (when available in message/suggestion)
4. **Floating card effect**: Cards should appear to float on the page with subtle shadows and animations
5. **Smooth animations**: Expand/collapse with smooth height transitions
6. **Preserve existing filters**: Keep all current filter functionality (text search, date range, level filter, group by)
7. **Responsive**: Works on mobile and desktop

### Visual/UX
1. **Card design**: Modern card with header showing timestamp, level badge, and preview of message
2. **Expand indicator**: Chevron icon that rotates on expand/collapse
3. **Level color coding**: Maintain existing level colors (info, success, warning, error, skip)
4. **Hover/focus states**: Subtle elevation increase on hover
5. **Keyboard accessible**: Enter/Space to toggle, focus visible
6. **Dark/Light theme**: Full theme support

### Data Enhancement
1. **Parse log messages**: Extract library/series/episode from log messages when possible
2. **Show hierarchy**: Display as Library > Series > Episode in expanded view
3. **Job context**: Show job status and processed counts in card header

## Acceptance Criteria

### AC1: Card Carousel Display
- [ ] Logs render as individual floating cards instead of grouped sections
- [ ] Cards have subtle shadow and rounded corners
- [ ] Cards animate in on load (staggered entrance)

### AC2: Accordion Expansion
- [ ] Click card header expands to show full log details
- [ ] Click again or click another card collapses (accordion behavior - single open)
- [ ] Smooth height animation during expand/collapse
- [ ] Chevron icon rotates 180° on expand

### AC3: Hierarchical Information Display
- [ ] When log message contains library/series/episode info, display as tree: Library → Series → Episode
- [ ] Show job status badge in card header
- [ ] Show timestamp, level, and message preview in collapsed state
- [ ] Show full message, suggestion, jobId in expanded state

### AC4: Filter Integration
- [ ] All existing filters (text, date range, level, group by) still work
- [ ] Filtering updates card list in real-time
- [ ] Group by option changes card grouping visual

### AC5: Theme & Accessibility
- [ ] Light/dark theme works correctly
- [ ] Keyboard navigation (Tab, Enter, Space, Arrow keys)
- [ ] Focus visible states
- [ ] ARIA attributes for accordion pattern

### AC6: Performance
- [ ] Virtual scrolling or pagination for large log sets (>100 entries)
- [ ] Smooth 60fps animations
- [ ] No layout shift during expand/collapse