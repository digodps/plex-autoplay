# Delta: navegacao-lateral

## ADDED Requirements

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

