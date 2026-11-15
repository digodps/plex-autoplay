# Tasks: Enhanced Log Hierarchy Display

## Task List

### Phase 1: Backend - Enhanced Hierarchy Parsing

- [x] **TASK-001**: Extend `logHierarchy.js` with `parseFullHierarchy()` function
  - Parse library, series, episode from message + suggestion
  - Build nested structure: Library → Series[] → Episode[]
  - Export `getHierarchyTree()` for UI consumption
  - Maintain backward compatibility with `getHierarchyParts()`
  - Add unit tests for various log message formats

- [x] **TASK-002**: Add unit tests for hierarchy parser
  - Test library-only logs
  - Test library + series logs
  - Test library + series + episode logs
  - Test multiple series per library
  - Test multiple episodes per series
  - Test malformed/edge case messages
  - Test suggestion field cross-reference

### Phase 2: Frontend - Hierarchical Display

- [x] **TASK-003**: Update `LogCardContent.vue` to render hierarchical tree
  - Import `getHierarchyTree` from logHierarchy.js
  - Create recursive `HierarchyNode` component (inline)
  - Render tree with proper indentation and connectors
  - Add click handlers for expand/collapse per level
  - Maintain existing metadata display (jobId, status, level)

- [x] **TASK-004**: Style hierarchy tree with CSS
  - Tree connectors (vertical lines, elbows)
  - Level-based indentation (0, 1, 2)
  - Icons per level (📚, 📺, ▶️)
  - Hover/focus states
  - Expanded/collapsed chevron animation
  - Dark/light theme support
  - Reduced motion support

- [x] **TASK-005**: Add accessibility attributes to hierarchy tree
  - `role="tree"` on container
  - `role="treeitem"` on nodes
  - `aria-expanded` on expandable nodes
  - `aria-level` for depth
  - Keyboard navigation (enter, space)
  - Focus visible styles

- [x] **TASK-006**: Update `LogCard.vue` content area integration
  - Ensure content wrapper accommodates variable-height tree
  - Smooth height animation for card expansion
  - No layout shift when hierarchy expands/collapses

### Phase 3: Testing & Validation

- [x] **TASK-007**: Add E2E test for log hierarchy display
  - Trigger scan to generate logs
  - Navigate to `/logs` page
  - Verify hierarchy tree renders correctly
  - Test expand/collapse interactions
  - Test keyboard navigation
  - Verify theme toggle works
  - Verify menu navigation works

- [x] **TASK-008**: Visual regression tests
  - Light theme screenshot
  - Dark theme screenshot
  - Expanded hierarchy screenshot
  - Collapsed hierarchy screenshot

- [x] **TASK-009**: Run full test suite and lint
  - `npm run lint` in vue/plex-frontend
  - `npm run test` in vue/plex-frontend
  - `dotnet build PlexAutoplay.slnx -c Release`
  - `docker compose -f infra/docker-compose.yml build`

### Phase 4: Documentation

- [x] **TASK-010**: Update any relevant documentation
  - Component README if exists
  - Changelog entry

## Task Dependencies

```
TASK-001 → TASK-002
TASK-001 → TASK-003
TASK-003 → TASK-004
TASK-003 → TASK-005
TASK-004 → TASK-006
TASK-005 → TASK-006
TASK-006 → TASK-007
TASK-001 → TASK-007 (parser tested)
TASK-007 → TASK-008
TASK-007 → TASK-009
```

## Parallel Execution Groups

**Group 1 (after TASK-001):**
- TASK-002 (tests)
- TASK-003 (frontend component)

**Group 2 (after TASK-003):**
- TASK-004 (styles)
- TASK-005 (accessibility)

**Group 3 (after TASK-004, TASK-005):**
- TASK-006 (integration)

**Group 4 (after TASK-006):**
- TASK-007 (E2E)
- TASK-008 (visual)

**Group 5 (after TASK-007):**
- TASK-009 (full build)
- TASK-010 (docs)