# Change: Library Card Series Mosaic Hover

## Overview
Replace static TV icon on library cards with a dynamic mosaic of series posters that animates on hover (shuffle effect).

## Files Created
- `specs/library-card-series-mosaic-hover/spec.md` - Specification
- `design.md` - Technical design
- `tasks.md` - Implementation tasks
- `proposal.md` - Proposal summary

## Commands
```bash
# Apply change (run from worktree root)
openspec instructions apply --change "library-card-series-mosaic-hover"

# Validate
openspec validate library-card-series-mosaic-hover --strict

# Build
cd vue/plex-frontend && npm run build

# Lint
cd vue/plex-frontend && npm run lint
```

## Acceptance Criteria
- [ ] Library cards show mosaic of series posters (max 6)
- [ ] Hover triggers smooth shuffle animation
- [ ] Empty libraries show neutral TV icon
- [ ] Works in light/dark themes
- [ ] Build & lint pass
- [ ] Playwright QA passes (theme, menus, mosaic)