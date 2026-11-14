# Spec Delta

## Purpose

Remove the electric/lightning bolt animation effect from the application logo (AppLogo) as the visual result was considered strange/distracting. Revert to the original static logo.

## REMOVED Requirements

### Requirement: Electric logo animation (added in add-electric-logo-icon-effect)

The `AppLogo` component SHALL NOT render an electric/lightning bolt path and SHALL NOT animate it.

#### Scenario: Logo renders without electric bolt

- **WHEN** the `AppLogo` component is rendered
- **THEN** the SVG contains only two paths: the original triangle (play-like) and the original vertical line
- **AND** no lightning bolt path is present

#### Scenario: No electric animation

- **WHEN** the `AppLogo` is visible on screen
- **THEN** the logo is completely static with no CSS animations
- **AND** no keyframes or animation properties are applied

#### Scenario: No reduced motion handling needed

- **WHEN** the user has `prefers-reduced-motion: reduce` set
- **THEN** the logo behaves identically (static) — no special handling required

#### Scenario: Logo size prop works without animation

- **WHEN** the `AppLogo` is used with different `size` props (e.g., 24, 28, 32, 40)
- **THEN** the logo scales proportionally
- **AND** the stroke widths remain visually consistent

#### Scenario: Theme colors work without animation

- **WHEN** the theme changes between light and dark mode
- **THEN** the logo uses `currentColor` to inherit the text/brand color automatically
- **AND** no hardcoded colors are used