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
2. Keep `<script setup>` with `defineProps({ size: { type: Number, default: 28 } })` — required for the `size` prop API. Remove only the `import { ref }` and the `animationDelay` ref.
3. Remove the entire `<style>` section (only held animation rules)
4. Result: template with two static paths + minimal script defining `size`

### E2E test cleanup

`tests/e2e/tests/qa-electric-logo-animation.spec.js` asserts the lightning path exists and animates. It was created solely to validate the removed feature and must be deleted. Theme/menu regression coverage remains in `tests/e2e/tests/qa-theme-and-navigation.spec.js`.

## Risks / Trade-offs

- **Risk**: None — simple reversion to previously working state
- **Trade-off**: Loss of "dynamic" brand identity, but user prefers clean static logo