# apresentacao Specification

## Purpose
Define o comportamento da camada de apresentação: tema claro/escuro em escala
de cinza aplicado globalmente e antes do primeiro paint, fonte única de tokens
de estilo, ausência de estilo inline, dependências de camada em um sentido só
(verificadas por lint), estados de carregando/vazio/erro padronizados e
acessibilidade (contraste, foco, alvos de toque, movimento reduzido).

## Requirements

### Requirement: Grayscale theme tokens in light and dark modes
The system SHALL define every visual token in `src/styles/tokens.css`, for both light (`:root`) and dark (`[data-theme="dark"]`) modes: a neutral grayscale scale for text and surfaces plus a brand scale for navigation and accents, with text pairs at 4.5:1 or better and control-delimiting non-text pairs at 3:1 or better.

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

### Requirement: Global theme applied before first paint
The system SHALL apply the user's theme by setting `data-theme="light"|"dark"` on the root element before the application bundle executes, resolving `system` through `prefers-color-scheme`, so that no flash of incorrect theme occurs on any route.

#### Scenario: Direct navigation to a non-dashboard route
- **WHEN** the user loads the application directly at `/login` or `/settings`
- **THEN** the theme is already applied to the root element on first paint, with no flash of the default theme

#### Scenario: System mode follows the operating system
- **WHEN** `mode` is `system` and the operating system preference changes
- **THEN** the application switches between light and dark accordingly without reload

#### Scenario: Server preference is reconciled after load
- **WHEN** the application finishes loading and `GET /api/theme` returns a stored preference
- **THEN** the root element attribute is reconciled with the stored value

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

### Requirement: No inline styles in components
The system SHALL express all static styling through tokens and CSS classes, leaving inline `style` attributes only for justified dynamic bindings.

#### Scenario: Static style attribute present
- **WHEN** a `.vue` file under `src` is searched for `style="`
- **THEN** no match is found, except bindings whose value is computed at runtime

### Requirement: Standardized loading, empty and error states
The system SHALL render every data-loading view through the shared state components, using a skeleton with `aria-busy="true"` while loading and an error state that offers a retry action.

#### Scenario: Data loading fails
- **WHEN** a view's data request fails
- **THEN** the view shows the shared error state with a retry control that re-runs the request

### Requirement: Accessibility of the shell and controls
The system SHALL declare the document language as pt-BR, expose a visible focus indicator that does not depend on the user's accent preference, provide a touch target of at least 44px for controls in the shell, use decorative emoji only with `aria-hidden="true"`, and honor `prefers-reduced-motion`.

#### Scenario: Keyboard focus
- **WHEN** a user tabs to an interactive element
- **THEN** a visible focus ring meeting at least 3:1 against the adjacent background is shown

#### Scenario: Reduced motion requested
- **WHEN** the operating system requests reduced motion
- **THEN** transitions and animations are effectively disabled
