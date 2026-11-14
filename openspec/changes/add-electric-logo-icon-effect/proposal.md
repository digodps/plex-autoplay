# Proposal

## Why

The current `AppLogo` component is a static SVG outline (triangle + vertical line) that serves as the brand mark in the header. To enhance visual appeal and create a more distinctive brand identity, we want to add an electric/lightning effect animation to the logo. This will make the application feel more dynamic and modern, especially when users first land on the dashboard.

## What Changes

- Add an electric/lightning bolt SVG path to the existing `AppLogo` component
- Implement a subtle animated effect (CSS keyframes or SMIL animation) that triggers on hover or continuously with a low frequency
- Ensure the animation respects `prefers-reduced-motion` for accessibility
- Maintain the existing logo structure (size prop, viewBox, role, aria-label)
- The animation should be subtle and not distracting - a gentle electric pulse or lightning flicker effect

## Capabilities

### New Capabilities

- `brand/logo-animation`: Electric/lightning animation effect for the application logo

### Modified Capabilities

- `web-ui`: The logo rendering behavior changes to include animation

## Impact

- Modified file: `vue/plex-frontend/src/components/AppLogo.vue`
- No API changes
- No backend changes
- No new dependencies required (pure CSS/SVG animation)
- Accessibility: Must respect `prefers-reduced-motion`