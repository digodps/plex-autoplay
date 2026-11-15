# Design: Enhanced Log Hierarchy Display

## Overview
Extend the existing log hierarchy parsing and display to show nested expandable cards for Library → Series → Episode hierarchy extracted from scan log messages.

## Architecture

### Current State
- `logHierarchy.js` parses messages for library/series/episode using regex patterns
- `LogCardContent.vue` displays flat hierarchy parts
- `LogCarouselView.vue` groups logs by date/status/level/library

### Target State
- Enhanced parser extracts full hierarchy from message + suggestion fields
- Log cards display nested tree with expand/collapse per level
- Clicking a hierarchy level toggles its children visibility
- Maintains all existing grouping/filtering

## Component Changes

### 1. `logHierarchy.js` - Enhanced Parsing

**New exports:**
```javascript
// Parse full nested hierarchy from message + suggestion
export function parseFullHierarchy(message, suggestion) {
  // Returns: { library: { name, series: [{ name, episodes: [...] }] } }
}

// Check if log has any hierarchy info
export function hasHierarchy(hierarchy) { ... }

// Get flat parts for backward compatibility
export function getHierarchyParts(hierarchy) { ... }

// Get nested tree for new UI
export function getHierarchyTree(hierarchy) { ... }
```

**Parsing logic:**
- Extract library from message patterns (existing)
- Extract series from message patterns (existing)  
- Extract episode from message patterns (existing)
- Cross-reference with suggestion field for additional context
- Build nested structure: Library → Series[] → Episode[]

### 2. `LogCardContent.vue` - Hierarchical Display

**New structure:**
```vue
<!-- Hierarchy Tree -->
<div v-if="hierarchyTree" class="hierarchy-tree">
  <HierarchyNode 
    v-for="library in hierarchyTree.libraries" 
    :key="library.name"
    :node="library"
    :level="0"
  />
</div>
```

**HierarchyNode component (inline or extracted):**
- Renders node with icon, label, value
- Click handler toggles `expanded` state
- Recursively renders children when expanded
- Indentation by level (0=library, 1=series, 2=episode)
- Visual connectors (lines) between levels

**Props:**
- `node`: { name, value, icon, children, expanded }
- `level`: indentation depth

### 3. `LogCard.vue` - Card Expansion

**Changes:**
- Card click toggles full card expansion (existing)
- Content area now renders hierarchical tree
- Maintain smooth height animation
- Focus management for accessibility

## Data Structures

### Parsed Hierarchy (flat - backward compat)
```javascript
{
  library: "Movies",
  series: "Breaking Bad",
  episode: "S01E01 - Pilot"
}
```

### Parsed Hierarchy (nested - new)
```javascript
{
  libraries: [
    {
      name: "Movies",
      icon: "📚",
      series: [
        {
          name: "Breaking Bad",
          icon: "📺",
          episodes: [
            { name: "S01E01 - Pilot", icon: "▶️" },
            { name: "S01E02 - Cat's in the Bag...", icon: "▶️" }
          ]
        }
      ]
    }
  ]
}
```

## UI/UX Details

### Visual Design
- **Library level**: Bold, folder icon 📚, primary color
- **Series level**: Medium weight, TV icon 📺, secondary color  
- **Episode level**: Regular, play icon ▶️, muted color
- **Connectors**: Vertical lines with elbows (tree view style)
- **Hover**: Highlight row, show expand/collapse chevron
- **Expanded state**: Chevron rotated, children visible

### Interactions
- Click anywhere on hierarchy row → toggle that level's children
- Keyboard: Enter/Space toggles, Arrow keys navigate tree
- Card header click → toggle full card (existing behavior)
- Nested expansion independent of card expansion

### Animations
- Children slide down/up (200ms ease-out)
- Opacity fade in/out
- Respect `prefers-reduced-motion`

### Accessibility
- `role="tree"` on container
- `role="treeitem"` on each node
- `aria-expanded` on expandable nodes
- `aria-level` for nesting depth
- Focus visible styles
- Keyboard navigation (arrows, enter, home/end)

## Theme Support
- Uses existing CSS variables (`--color-accent`, `--color-bg`, etc.)
- Dark mode: adjusted backgrounds/borders (existing patterns)
- No hardcoded colors

## Backward Compatibility
- `getHierarchyParts()` unchanged for existing consumers
- `hasHierarchy()` unchanged
- New `getHierarchyTree()` for new UI
- Cards without hierarchy render as before

## Testing Strategy
1. Unit tests for `parseFullHierarchy()` with various log formats
2. Component tests for `LogCardContent` hierarchy rendering
3. E2E test: scan → logs page → verify hierarchy display
4. Visual regression: light/dark theme screenshots
5. Accessibility audit: axe-core in CI

## File Changes Summary
| File | Change Type |
|------|-------------|
| `src/components/Logs/cards/logHierarchy.js` | Modify - add nested parsing |
| `src/components/Logs/cards/LogCardContent.vue` | Modify - hierarchical tree display |
| `src/components/Logs/cards/LogCard.vue` | Minor - content area update |
| `tests/unit/logHierarchy.test.js` | New - parser tests |
| `tests/e2e/log-hierarchy.spec.ts` | New - E2E test |