# navegacao-lateral Specification (Delta)

## MODIFIED Requirements

### Requirement: Canva-style rail structure

The system SHALL render the main navigation as a vertical sidebar of width
256px on viewports wider than 768px, listing one item per Plex library, where
every item renders a 20px icon beside its label, and SHALL highlight the item
matching the current route.

#### Scenario: Item layout on desktop

- **WHEN** the shell renders at a viewport wider than 768px
- **THEN** the sidebar is 256px wide and each item shows its icon beside its
  label, vertically centered

#### Scenario: Current route is highlighted

- **WHEN** the user is at `/libraries/1/series`
- **THEN** the item for library `1` carries `aria-current="page"` and renders
  with a filled background and contrasting text color

#### Scenario: Exactly one item is current

- **WHEN** any shell route is active
- **THEN** at most one navigation item has `aria-current="page"`

### Requirement: Navigation stays outside the component

The navigation component SHALL receive the library items and the collapsed flag
as props, without importing `vue-router`, stores, composables or `api.js`. The
shell SHALL perform data fetching and route changes.

#### Scenario: Component triggers navigation

- **WHEN** the user activates a library item
- **THEN** the shell navigates to `/libraries/<key>/series`

#### Scenario: Component receives items

- **WHEN** the shell renders the sidebar
- **THEN** the sidebar renders only the items passed via props

#### Scenario: Boundary lint on the component

- **WHEN** `components/Sidebar/SidebarArea.vue` imports a store or `api.js`
- **THEN** `npm run lint` fails with a boundaries error

### Requirement: Rail accessibility and responsivity

The system SHALL keep every sidebar control at least 44px tall, expose a visible
focus indicator, give icon-only controls an accessible name, honor
`prefers-reduced-motion`, and, at viewports of 768px or less, SHALL render the
sidebar in compact icon-only mode.

#### Scenario: Keyboard focus on a rail item

- **WHEN** the user tabs into the sidebar
- **THEN** the focused item shows a visible focus ring

#### Scenario: Small viewport

- **WHEN** the viewport is 768px wide or less
- **THEN** the sidebar renders icon-only and every item keeps an accessible name
  and a touch target of at least 44px

#### Scenario: Reduced motion

- **WHEN** the operating system requests reduced motion
- **THEN** the sidebar renders without transition or animation

## REMOVED Requirements

### Requirement: Material Tailwind as the styling system

**Reason**: Material Tailwind is not installed and adding it is unnecessary; the
frontend uses plain Tailwind utilities. Preflight stays disabled, covered by the
web-ui delta.

**Migration**: Use Tailwind utilities that map to existing `style.css` tokens;
do not add the Material Tailwind dependency.

### Requirement: Sidebar owns theme, logout and compact mode

**Reason**: The theme toggle and logout now live in the header, and the sidebar
only lists Plex libraries. Compact mode is controlled by the header hamburger.

**Migration**: See web-ui delta requirements `Header structure` and
`Compact sidebar`. Persist the compact preference in `localStorage` under
`sidebar.collapsed.v2`.

### Requirement: Sidebar stacking, header removal and active highlight

**Reason**: The shell keeps a header, and the sidebar starts below it. Stacking
a theme toggle inside the sidebar contradicts the requested navigation model.

**Migration**: See web-ui delta requirements `Sidebar position and spacing` and
`Sidebar lists Plex libraries`. Active highlight applies to the library matching
the current route.
