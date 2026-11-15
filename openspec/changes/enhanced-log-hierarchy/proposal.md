# Proposal: Enhanced Log Hierarchy Display

## Summary
Extend the scan log display to show hierarchical information (Library > Series > Episode) extracted from log messages. Each log entry becomes an expandable card showing the processing hierarchy, with nested expandable sections for each hierarchy level.

## Motivation
Currently, scan logs show flat entries with messages like "Processing library: Movies" or "Processing episode: S01E01 - Pilot". Operators need to quickly understand the processing hierarchy:
- Which library was scanned
- Which series within that library
- Which episodes within that series

This enables faster debugging and better visibility into the scan process.

## Scope
**In scope:**
- Extend `logHierarchy.js` to parse library, series, episode from log messages and suggestions
- Update `LogCardContent.vue` to display hierarchical tree with expandable sections
- Each hierarchy level (Library, Series, Episode) is clickable and expands to show details
- Maintain existing grouping/filtering functionality
- Follow existing UI patterns (theme, animations, accessibility)

**Out of scope:**
- Changing backend log storage format
- Adding new API endpoints
- Changing log persistence schema

## Acceptance Criteria
1. **Hierarchy parsing**: Log messages containing library/series/episode info are parsed correctly
2. **Visual hierarchy**: Each log card shows a tree: Library → Series → Episode with icons
3. **Expandable sections**: Clicking a hierarchy level expands/collapses its children
4. **Nested expansion**: Library expands to show Series, Series expands to show Episodes
5. **Theme compliance**: Works in both light/dark themes
6. **Accessibility**: Keyboard navigation, ARIA labels, focus management
7. **Existing features preserved**: Grouping, filtering, live search still work

## Dependencies
- Existing `logHierarchy.js` parsing utilities
- `LogCard.vue`, `LogCardContent.vue`, `LogCarouselView.vue` components
- `logs` store for data fetching

## Risks
- Log message formats may vary; parser must be robust
- Performance with deeply nested hierarchies (mitigation: virtualize or limit depth)