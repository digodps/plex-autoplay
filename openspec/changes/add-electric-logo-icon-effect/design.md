# Design

## Context

The current `AppLogo` component (`vue/plex-frontend/src/components/AppLogo.vue`) renders a static SVG with two paths:
1. A triangle (play-like shape): `M18 18 38 32 18 46Z`
2. A vertical line: `M48 20v24`

Both use `stroke="currentColor"` and `fill="none"` to inherit the theme color. The component accepts a `size` prop (default 28) for width/height.

## Goals / Non-Goals

**Goals:**
- Add a lightning bolt path to the SVG
- Implement a subtle electric pulse/flicker animation using CSS keyframes
- Respect `prefers-reduced-motion` for accessibility
- Maintain existing API (size prop, viewBox, aria attributes)
- No new dependencies

**Non-Goals:**
- Complex particle effects or WebGL
- Animation on click/hover only (should be ambient/continuous)
- Changing the existing logo paths
- Adding configuration options for animation speed/intensity

## Decisions

### Animation Approach: CSS Keyframes

Use CSS `@keyframes` with `animation` property on the lightning bolt path. This is:
- Lightweight (no JS)
- Respects `prefers-reduced-motion` via `@media (prefers-reduced-motion: reduce)`
- Works with SVG `stroke-dashoffset` / `stroke-dasharray` for a "drawing" effect, or `opacity`/`filter` for a pulse/flicker effect

### Lightning Bolt Path

Add a new `<path>` element with a lightning bolt shape:
```svg
<path
  d="M32 12 L26 28 L30 28 L22 48 L38 30 L34 30 L42 12 Z"
  fill="none"
  stroke="currentColor"
  stroke-width="2.5"
  stroke-linejoin="round"
  stroke-linecap="round"
  class="logo-lightning"
/>
```

Positioned between the existing two paths, centered in the 64x64 viewBox.

### Animation: Subtle Opacity Pulse + Flicker

Use a combination of:
1. **Opacity pulse**: 0.6 → 1 → 0.6 over ~2.5s (ease-in-out)
2. **Subtle flicker**: Very brief opacity drops (0.1s at 0.3, 0.6, 0.9 of cycle) to simulate electric flicker

CSS:
```css
.logo-lightning {
  animation: electric-pulse 2.5s ease-in-out infinite;
}

@keyframes electric-pulse {
  0%, 100% { opacity: 0.6; }
  50% { opacity: 1; }
  /* Flicker moments */
  30%, 31% { opacity: 0.3; }
  60%, 61% { opacity: 0.4; }
  90%, 91% { opacity: 0.35; }
}

@media (prefers-reduced-motion: reduce) {
  .logo-lightning {
    animation: none;
    opacity: 1;
  }
}
```

### Random Delay Variation

Add a small random animation-delay (0-500ms) via inline style on the component to avoid synchronized animation across multiple logos if ever rendered together.

## Risks / Trade-offs

- **Risk**: Animation may be too distracting if too intense
  - **Mitigation**: Keep opacity range narrow (0.3-1), duration slow (2.5s), flicker very brief
- **Risk**: `prefers-reduced-motion` not tested in all browsers
  - **Mitigation**: Test in Chrome, Firefox, Safari; fallback to static opacity: 1
- **Trade-off**: CSS animation vs SMIL
  - **Decision**: CSS preferred for better `prefers-reduced-motion` support and easier maintenance
- **Trade-off**: Random delay via inline style
  - **Decision**: Acceptable minor inline style for visual polish; alternative is CSS `animation-delay` with `nth-child` but not applicable for single element