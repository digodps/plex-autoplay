# Design

## Context

The `AppLogo` component (`vue/plex-frontend/src/components/AppLogo.vue`) currently renders a static SVG with three paths:
1. A triangle (play-like shape): `M18 18 38 32 18 46Z`
2. A vertical line: `M48 20v24`
3. A lightning bolt path with CSS animation

The component accepts a `size` prop (default 28) for width/height. The animation uses CSS `@keyframes` with `animation` property on the lightning bolt path, and respects `prefers-reduced-motion` via `@media (prefers-reduced-motion: reduce)`.

## Goals / Non-Goals

**Goals:**
- Remove the lightning bolt path from the SVG
- Remove all animation-related CSS (keyframes, media query)
- Remove the random animation delay logic
- Maintain existing API (size prop, viewBox, aria attributes)
- Keep the existing two paths (triangle + vertical line)

**Non-Goals:**
- Changing the existing logo paths
- Adding new logo variants
- Modifying any other components

## Decisions

### Reversion Approach

Simply remove the added elements. The original static logo was:
- Two paths with `stroke="currentColor"` and `fill="none"`
- No animation
- No reduced-motion handling needed

### Implementation

Edit `vue/plex-frontend/src/components/AppLogo.vue`:
1. Remove the `<path class="logo-lightning" ... />` element
2. Remove the `<script>` section entirely (no refs needed)
3. Remove the entire `<style>` section
4. Result: pure template with two static paths

## Risks / Trade-offs

- **Risk**: None — simple reversion to previously working state
- **Trade-off**: Loss of "dynamic" brand identity, but user prefers clean static logo