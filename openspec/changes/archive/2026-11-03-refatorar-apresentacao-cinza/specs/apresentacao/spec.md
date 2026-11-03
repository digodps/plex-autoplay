# Spec Delta

## Purpose

Define o comportamento da camada de apresentação: tema claro/escuro em escala
de cinza aplicado globalmente e antes do primeiro paint, fonte única de tokens
de estilo, ausência de estilo inline, dependências de camada em um sentido só
(verificadas por lint), estados de carregando/vazio/erro padronizados e
acessibilidade (contraste, foco, alvos de toque, movimento reduzido).

## ADDED Requirements

### Requirement: Grayscale theme tokens in light and dark modes
The system SHALL provide a single source of visual tokens in `src/styles/tokens.css` defining a neutral grayscale scale for both light (`:root`) and dark (`[data-theme="dark"]`) modes, where every text token meets a contrast ratio of at least 4.5:1 and every non-text token used to delimit or identify a control (input borders, icons, focus ring) meets at least 3:1 against its adjacent background.

#### Scenario: Text and non-text contrast are met in both modes
- **WHEN** the interface renders in light mode or dark mode
- **THEN** body, secondary and tertiary text reach at least 4.5:1 against the surface they sit on, and input borders, icons, controls and the focus ring reach at least 3:1 against the adjacent background

#### Scenario: No color literal outside the token file
- **WHEN** any `.vue` or component CSS file is inspected
- **THEN** it contains no hexadecimal color literal, except the semantic tokens (`danger`, `success`, `warning`) and data-driven values

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
