# Spec Delta: Dashboard Button Size Standardization

## ADDED Requirements

### Requirement: Dashboard buttons use shared button classes
All buttons rendered in the Dashboard view SHALL use the shared component classes `.btn` plus exactly one size class (`.btn-md`) defined in `src/styles/style.css`, instead of ad-hoc `px-*`/`py-*`/`text-*`/`rounded-*` utilities. Every Dashboard button MUST therefore share the same size, radius, typography, focus-visible ring, and disabled state.

#### Scenario: Primary action uses shared button classes
- **WHEN** the "Iniciar Escaneamento" button is rendered
- **THEN** its class list contains `btn`, `btn-md`, and `btn-primary`
- **AND** it no longer contains `px-5`, `py-2.5`, or `rounded-xl`

#### Scenario: Secondary action uses shared button classes
- **WHEN** the "Ver Log Completo" button is rendered
- **THEN** its class list contains `btn`, `btn-md`, and `btn-secondary`

#### Scenario: Error retry buttons use shared button classes
- **WHEN** either "Tentar novamente" button is rendered (total failure or library error state)
- **THEN** its class list contains `btn` and `btn-md` plus a variant class (`btn-danger` for total failure, `btn-warning` for library error)
- **AND** it no longer contains `px-4` or `py-2`

#### Scenario: Same-tier buttons have identical computed height
- **WHEN** the computed `height` of "Iniciar Escaneamento", "Ver Log Completo", and "Tentar novamente" is measured in the browser
- **THEN** all three report the same pixel value

#### Scenario: All Dashboard buttons expose focus-visible and disabled states
- **WHEN** any Dashboard button receives keyboard focus
- **THEN** a 2px primary-colored focus ring with offset is visible in both light and dark themes
- **AND** when `:disabled`, the button shows reduced opacity on all four buttons

### Requirement: Shared button size class is defined once
`src/styles/style.css` SHALL define `.btn` and `.btn-md` inside its `@layer components` block as the single source of Dashboard button sizing.

#### Scenario: Base button class defined
- **WHEN** `style.css` is inspected
- **THEN** `.btn` declares `inline-flex`, `items-center`, `justify-center`, `gap`, `rounded-xl`, `text-sm`, `font-medium`, `transition`, `focus-visible:outline-none`, `focus-visible:ring-2`, `focus-visible:ring-primary`, `focus-visible:ring-offset-2`, and `disabled:opacity-60`

#### Scenario: Middle size class defined
- **WHEN** `style.css` is inspected
- **THEN** `.btn-md` declares horizontal padding `1rem` (`px-4`) and vertical padding `0.5rem` (`py-2`)