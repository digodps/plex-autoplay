# Tasks: Log Carousel Accordion View

## Phase 1: Core Components

- [ ] **Task 1.1**: Create `LogCard.vue` component
  - Card base structure with header and collapsible content
  - Props: log, index, isExpanded, onToggle
  - Level badge with color coding
  - Chevron icon with rotation animation
  - Hover/focus states
  - ARIA attributes for accordion pattern

- [ ] **Task 1.2**: Create `LogCardHeader.vue` sub-component
  - Timestamp display
  - Level badge
  - Job status badge (optional)
  - Message preview (truncated)
  - Chevron button with keyboard support

- [ ] **Task 1.3**: Create `LogCardContent.vue` sub-component
  - Full message display
  - Suggestion highlight (if present)
  - JobId (copyable)
  - Hierarchy tree display (Library → Series → Episode)
  - Full timestamp

- [ ] **Task 1.4**: Create hierarchy parsing utility
  - Parse library/series/episode from log messages
  - Support common message patterns
  - Return structured hierarchy object

## Phase 2: Carousel View

- [ ] **Task 2.1**: Create `LogCarouselView.vue` component
  - Replace `LogGroupedView.vue` entirely
  - Props: items, groupBy, loading
  - State: expandedIndex (single open)
  - Render LogCard for each item with staggered entrance
  - Handle toggle to expand/collapse (accordion behavior)
  - Keyboard navigation (Arrow keys, Home, End, Escape)

- [ ] **Task 2.2**: Implement groupBy visual separators
  - Date groups: section headers with date
  - Status groups: section headers with status
  - Level groups: section headers with level
  - Library groups (new): section headers with library name

- [ ] **Task 2.3**: Add entrance/exit animations
  - Staggered fade-in slide-up on mount
  - Smooth expand/collapse height animation
  - Chevron rotation transition

## Phase 3: Integration

- [ ] **Task 3.1**: Update `LogsView.vue`
  - Replace `<LogGroupedView />` with `<LogCarouselView />`
  - Pass same props
  - Remove LogGroupedView import

- [ ] **Task 3.2**: Update `useLogsStore.js`
  - Add "library" to groupBy options
  - Add computed `parsedLogs` with hierarchy info
  - Ensure filters work with new view

- [ ] **Task 3.3**: Update `LogFilterBar.vue`
  - Add "Biblioteca" to groupingOptions
  - Ensure groupBy change works with carousel

## Phase 4: Styling & Polish

- [ ] **Task 4.1**: Implement floating card styles
  - Base card with shadow, rounded corners
  - Hover elevation effect
  - Dark/light theme support
  - CSS variables for theming

- [ ] **Task 4.2**: Implement animations
  - Entrance stagger animation
  - Expand/collapse with CSS grid max-height trick
  - Chevron rotation
  - Reduced motion support

- [ ] **Task 4.3**: Responsive design
  - Mobile: full width, stacked hierarchy
  - Tablet: comfortable spacing
  - Desktop: max-width container

- [ ] **Task 4.4**: Accessibility audit
  - ARIA attributes on all interactive elements
  - Focus visible states
  - Keyboard navigation complete
  - Screen reader testing

## Phase 5: Testing & QA

- [ ] **Task 5.1**: Unit tests for hierarchy parsing
- [ ] **Task 5.2**: Unit tests for LogCard component
- [ ] **Task 5.3**: E2E tests with Playwright
  - Card rendering
  - Expand/collapse behavior
  - Single open accordion
  - Filter integration
  - Theme toggle
  - Keyboard navigation
  - Mobile layout

- [ ] **Task 5.4**: Performance validation
  - 100+ logs render smoothly
  - 60fps animations
  - No layout shift

## Phase 6: Documentation

- [ ] **Task 6.1**: Update component documentation
- [ ] **Task 6.2**: Add storybook stories (if applicable)