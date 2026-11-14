# Spec Delta

## Purpose

Add an electric/lightning bolt animation effect to the application logo (AppLogo) to create a dynamic and distinctive brand identity.

## ADDED Requirements

### Requirement: Electric logo animation

The `AppLogo` component SHALL render an electric/lightning bolt path in addition to the existing triangle and vertical line paths, and SHALL animate it with a subtle electric pulse/flicker effect.

#### Scenario: Logo renders with electric bolt

- **WHEN** the `AppLogo` component is rendered
- **THEN** the SVG contains three paths: the original triangle (play-like), the original vertical line, and a new lightning bolt path
- **AND** the lightning bolt path uses `stroke="currentColor"` and `fill="none"` to inherit the theme color

#### Scenario: Electric animation plays

- **WHEN** the `AppLogo` is visible on screen
- **THEN** the lightning bolt path animates with a subtle electric pulse/flicker effect using CSS keyframes or SMIL
- **AND** the animation duration is approximately 2-3 seconds per cycle
- **AND** the animation repeats indefinitely with a slight random delay variation

#### Scenario: Reduced motion disables animation

- **WHEN** the user has `prefers-reduced-motion: reduce` set
- **THEN** the electric animation is disabled (static lightning bolt)
- **AND** the lightning bolt path remains visible but static

#### Scenario: Logo size prop works with animation

- **WHEN** the `AppLogo` is used with different `size` props (e.g., 24, 28, 32, 40)
- **THEN** the animation scales proportionally with the logo size
- **AND** the stroke widths remain visually consistent

#### Scenario: Animation respects theme colors

- **WHEN** the theme changes between light and dark mode
- **THEN** the lightning bolt uses `currentColor` to inherit the text/brand color automatically
- **AND** no hardcoded colors are used in the animation