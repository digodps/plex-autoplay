# apresentacao Delta

## MODIFIED Requirements

### Requirement: Grayscale theme tokens in light and dark modes
The system SHALL provide a single source of visual tokens in `src/styles/tokens.css` defining, for both light (`:root`) and dark (`[data-theme="dark"]`) modes, a neutral grayscale scale for text and surfaces plus a brand scale (`--brand`, `--brand-strong`, `--brand-soft`, `--rail-bg`, `--rail-border`) for navigation and accents, where every text token meets a contrast ratio of at least 4.5:1 and every non-text token used to delimit or identify a control (input borders, icons, focus ring) meets at least 3:1 against its adjacent background.

#### Scenario: Text and non-text contrast are met in both modes
- **WHEN** the interface renders in light mode or dark mode
- **THEN** body, secondary and tertiary text reach at least 4.5:1 against the surface they sit on, brand-strong labels reach at least 4.5:1 against the rail background, and input borders, icons, controls and the focus ring reach at least 3:1 against the adjacent background

#### Scenario: Brand tokens are defined in both modes
- **WHEN** `src/styles/tokens.css` is inspected
- **THEN** `--brand`, `--brand-strong`, `--brand-soft`, `--rail-bg` and `--rail-border` are declared in `:root` and in `[data-theme="dark"]`

#### Scenario: No color literal outside the token file
- **WHEN** any `.vue` or component CSS file is inspected
- **THEN** it contains no hexadecimal color literal, except the semantic tokens (`danger`, `success`, `warning`) and data-driven values

#### Scenario: Contrast regression is caught by a test
- **WHEN** a token value is changed so a text pair drops below 4.5:1 or a non-text pair below 3:1
- **THEN** `node --test tests/` fails naming the offending token pair

### Requirement: Presentation layer dependency boundaries
The system SHALL enforce one-way import boundaries such that views may import components, composables, stores and lib but never `api.js`; components may import only lib and never stores, composables, `api.js` or `vue-router`; composables may import stores and lib but never `api.js`; and stores may import `api.js` and lib but never composables. Violations SHALL fail lint.

#### Scenario: A view performs a network call
- **WHEN** a file under `views/` imports `api.js`
- **THEN** the lint step reports an error and the build gate fails

#### Scenario: A component reads global state
- **WHEN** a file under `components/` imports a store or a composable
- **THEN** the lint step reports an error and the build gate fails

#### Scenario: A navigation component changes the route
- **WHEN** a component under `components/` imports `vue-router`
- **THEN** the lint step reports an error and the shell performs the route change instead, through an emitted event
