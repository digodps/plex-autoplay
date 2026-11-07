# navegacao-lateral Specification

## Purpose

Definir a navegação principal do plex-autoplay: um rail estreito no estilo
Canva, com ícone e rótulo empilhados, estado ativo destacado, comportamento
responsivo em telas pequenas e limites claros entre o componente de navegação e
o shell que muda de rota.

## Requirements

### Requirement: Canva-style rail structure

The system SHALL render the main navigation as a vertical rail of fixed width
88px on viewports wider than 768px, where every item stacks a 24px icon above
an 11px label, and SHALL highlight the item matching the current route with a
rounded lavender block using the brand tokens.

#### Scenario: Item layout on desktop

- **WHEN** the shell renders at a viewport wider than 768px
- **THEN** the rail is 88px wide and each item shows the icon above its label,
  both horizontally centered

#### Scenario: Current route is highlighted

- **WHEN** the user is at `/settings`
- **THEN** the settings item carries `aria-current="page"` and renders with
  `background: var(--brand-soft)` and label color `var(--brand-strong)`

#### Scenario: Exactly one item is current

- **WHEN** any shell route is active
- **THEN** exactly one navigation item has `aria-current="page"`

### Requirement: Navigation stays outside the component

The navigation component SHALL receive items, the active path and the status
label as props and SHALL emit `select` and `logout` events, without importing
`vue-router`, stores, composables or `api.js`.

#### Scenario: Component triggers navigation

- **WHEN** the user activates a rail item
- **THEN** the component emits `select` with the target path and the shell
  performs the route change

#### Scenario: Boundary lint on the component

- **WHEN** `components/AppSidebar.vue` imports `vue-router` or a store
- **THEN** `npm run lint` fails with a boundaries error

### Requirement: Rail accessibility and responsivity

The system SHALL keep every rail control at least 44px tall, expose a visible
focus indicator of at least 3:1 against the rail background, give icon-only
controls an accessible name, and, at viewports of 768px or less, SHALL turn the
rail into a horizontally scrollable strip without losing labels or focus
behavior.

#### Scenario: Keyboard focus on a rail item

- **WHEN** the user tabs into the rail
- **THEN** the focused item shows an outline using `--brand` with 2px width and
  2px offset

#### Scenario: Small viewport

- **WHEN** the viewport is 768px wide or less
- **THEN** the rail becomes a horizontal strip where each item still shows its
  icon, its label and keeps a touch target of at least 44px

#### Scenario: Reduced motion

- **WHEN** the operating system requests reduced motion
- **THEN** the rail renders without transition or animation

### Requirement: Material Tailwind as the styling system

The system SHALL integrate Material Tailwind into the build through its
`withMT` helper, which merges the Material Design theme (palette, typography,
shadows) into the Tailwind configuration, and SHALL style new presentation
components with Tailwind utilities whose colors map to the project tokens.
Tailwind's preflight SHALL stay disabled so the existing views keep their CSS.

#### Scenario: Brand utilities resolve to design tokens

- **WHEN** a component renders `bg-rail`, `text-brand-strong` or `bg-brand-soft`
- **THEN** the generated CSS resolves those classes to `var(--rail-bg)`,
  `var(--brand-strong)` and `var(--brand-soft)` from `tokens.css`, with no color
  literal in the component

#### Scenario: Material theme is merged into the Tailwind config

- **WHEN** `tailwind.config.cjs` is evaluated
- **THEN** it returns the configuration produced by `withMT`, keeping
  `corePlugins.preflight` set to `false` and the `theme.extend` color mapping

#### Scenario: Preflight disabled keeps existing views intact

- **WHEN** the production bundle is inspected
- **THEN** Tailwind's preflight reset is absent, so headings, inputs and tables
  from the existing views keep their current styling

### Requirement: Sidebar owns theme, logout and compact mode

The system SHALL render the theme toggle at the top of the sidebar and the
logout action at the bottom of the sidebar, and SHALL remove both actions from
the header. The sidebar SHALL also offer a compact mode that shows only icons,
reduces the sidebar width, keeps an accessible name on every navigation link,
and persists the preference across reloads.

#### Scenario: Theme toggle lives in the sidebar

- **WHEN** the shell renders any authenticated route
- **THEN** the theme toggle is present in the sidebar header and no theme toggle
  exists in the header

#### Scenario: Logout lives at the sidebar bottom

- **WHEN** the user activates the "Sair" control at the bottom of the sidebar
- **THEN** the auth session is cleared and the app navigates to `/login`

#### Scenario: Compact mode hides labels

- **WHEN** the user activates the compact mode control
- **THEN** the sidebar narrows to icon-only width, labels are not rendered, and
  each navigation link keeps an accessible name equal to its label

#### Scenario: Compact mode persists

- **WHEN** the user enables compact mode and reloads the page
- **THEN** the sidebar renders in compact mode from the stored preference

#### Scenario: Compact mode control is accessible

- **WHEN** the compact mode control is focused or read by assistive technology
- **THEN** it exposes `aria-expanded` reflecting the expanded state and an
  `aria-label` of "Recolher menu" or "Expandir menu"

### Requirement: Sidebar stacking, header removal and active highlight

The system SHALL stack the theme toggle directly below the menu control in the
sidebar, SHALL render the shell without a title bar, and SHALL highlight the
navigation item matching the current route with a background and text color
that are visible in both light and dark themes.

#### Scenario: Theme toggle sits below the menu control

- **WHEN** the sidebar renders expanded
- **THEN** the theme toggle appears on its own row directly below the menu
  control, with its label visible

#### Scenario: No title bar

- **WHEN** the shell renders any authenticated route
- **THEN** no header/title bar is rendered and the page content starts at the
  top of the content area

#### Scenario: Active item is visually distinct

- **WHEN** the user is at `/libraries`
- **THEN** the "Bibliotecas" item renders with a filled background and a text
  color contrasting with the sidebar background, and carries
  `aria-current="page"`

#### Scenario: Only the current item is highlighted

- **WHEN** any shell route is active
- **THEN** exactly one navigation item renders the active highlight
