# Proposal

## Why

The electric/lightning logo animation effect added in `add-electric-logo-icon-effect` was implemented but the visual result was considered strange/distracting by the user. The animation does not align with the desired brand identity and should be removed.

## What Changes

- Remove the lightning bolt SVG path from `AppLogo.vue`
- Remove the CSS keyframes animation (`electric-pulse`)
- Remove the `prefers-reduced-motion` media query for the logo
- Remove the random `animationDelay` inline style
- Restore the `AppLogo` component to its static state (triangle + vertical line only)

## Capabilities

### Removed Capabilities

- `brand-logo-animation`: Electric/lightning animation effect for the application logo

## Impact

- Modified file: `vue/plex-frontend/src/components/AppLogo.vue`
- Deleted file: `tests/e2e/tests/qa-electric-logo-animation.spec.js` (orphan QA test for removed feature)
- No API changes
- No backend changes
- No new dependencies
- Accessibility: Simplified - no motion concerns since animation removed