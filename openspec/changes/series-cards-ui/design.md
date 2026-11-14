# Design

## Context

Series catalog displays shows from Plex libraries. The current card is a basic button with thumbnail, title, and "Ver marcadores" text. The user requested an aesthetic adjustment to match modern streaming UI cards.

## Goals / Non-Goals

**Goals:**
- Clean, modern streaming card aesthetic (similar to anime/media apps).
- Rounded corners (`rounded-lg` or `rounded-xl`), overflow hidden, shadow effects.
- Title truncation, clean secondary metadata or action cue.
- Fully responsive grid matching screen sizes.

**Non-Goals:**
- Backend API changes.
- Changes to infinite scroll logic or library loading.

## Decisions

1. **Card layout**: Keep `<button>` semantic for accessibility, structure with `<div class="relative aspect-[2/3] ...">` for cover image, badge overlay container, and title below with subtle subtitle.
2. **Hover / Focus**: Scale image slightly (`scale-105`), darken/dim overlay (`bg-black/30`), highlight title.

## Risks / Trade-offs

- Image proportion changes might crop cover art differently -> mitigated by `object-cover`.
