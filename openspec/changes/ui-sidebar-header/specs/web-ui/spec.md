# UI — Sidebar & Header Spec (Delta)

## ADDED Requirements

### Requirement: Sidebar lists Plex libraries

The sidebar MUST render one item per library returned by
`GET /api/plex/libraries`, and MUST NOT render any static navigation item,
"Menu" control or theme toggle.

#### Scenario: Libraries rendered from API

- **WHEN** the authenticated shell loads and the API returns libraries
  `[{ key: "1", title: "Anime" }, { key: "2", title: "Filmes" }]`
- **THEN** the sidebar renders exactly two items, labelled `Anime` and `Filmes`

#### Scenario: No static items or controls

- **WHEN** the sidebar is rendered
- **THEN** it contains no item labelled `Bibliotecas`, no control labelled
  `Menu` and no theme toggle control

#### Scenario: Navigation target

- **WHEN** the user activates the item for library `1`
- **THEN** the app navigates to `/libraries/1/series`

#### Scenario: Active library

- **WHEN** the active route is `/libraries/1/series`
- **THEN** the item for library `1` carries `aria-current="page"`

#### Scenario: Empty and error states

- **WHEN** the API returns an empty list
- **THEN** the sidebar renders no navigation item and no error
- **WHEN** the API request fails
- **THEN** the sidebar renders an alert with a short error message

### Requirement: Sidebar position and spacing

The sidebar MUST render below the header, span the viewport height minus the
header, scroll independently, and MUST keep a vertical margin between library
items.

#### Scenario: Sidebar starts below header

- **WHEN** the authenticated shell renders at a viewport of 1280x800
- **THEN** the sidebar's top edge equals the header's bottom edge and the
  sidebar does not overlap the header

#### Scenario: Items are separated

- **WHEN** two or more libraries are rendered
- **THEN** consecutive items are separated by a vertical gap of at least 4px

### Requirement: Compact sidebar

The sidebar MUST support a compact mode that hides labels, keeps each item
icon-only with an accessible name, and persists the preference.

#### Scenario: Hamburger collapses sidebar

- **WHEN** the user activates the hamburger control in the header
- **THEN** the sidebar width changes from `16rem` to `4rem`, labels are not
  rendered, and each item keeps an accessible name equal to its library title

#### Scenario: Preference persists

- **WHEN** the user collapses the sidebar and reloads the page
- **THEN** the sidebar renders collapsed from `localStorage`

#### Scenario: Control state exposed

- **WHEN** the hamburger control is focused or read by assistive technology
- **THEN** it exposes `aria-expanded` reflecting the expanded state and an
  `aria-label` of `Recolher menu` or `Expandir menu`

### Requirement: Header structure

The header MUST contain a hamburger control, a separator, the breadcrumb of the
active route, a theme toggle, a profile menu with logout and a settings control.
Every control MUST have an accessible name.

#### Scenario: Header contains expected controls

- **WHEN** the user is on `/dashboard`
- **THEN** the header contains a hamburger, a separator, a breadcrumb, a theme
  toggle, a profile menu and a settings control, all labelled

#### Scenario: Theme toggle switches and persists

- **WHEN** the user activates the theme toggle
- **THEN** the `.dark` class on `<html>` flips and the preference is stored in
  `localStorage`

#### Scenario: Logout clears session

- **WHEN** the user activates `Sair` in the profile menu
- **THEN** the session cookie is cleared and the app navigates to `/login`

### Requirement: Header breadcrumb composition

The header MUST render a breadcrumb whose only fixed item is `Início`; every
following item MUST be derived from the active route and loaded data. The
header is the single owner of the breadcrumb and no page under it MAY render a
second breadcrumb.

#### Scenario: Library route

- **WHEN** the user is on `/libraries/1/series` and library `1` has title `Animes`
- **THEN** the header breadcrumb reads `Início > Animes`, with `Início` linking
  to `/dashboard` and `Animes` marked `aria-current="page"`

#### Scenario: Series route

- **WHEN** the user is on `/series/42/markers` with `libraryTitle=Animes` and
  `seriesTitle=One Piece`
- **THEN** the header breadcrumb reads `Início > Animes > One Piece`, with
  `Animes` linking back to that library and `One Piece` marked
  `aria-current="page"`

#### Scenario: Unknown library title

- **WHEN** the route references a library key absent from the loaded list and no
  `libraryTitle` query is present
- **THEN** the breadcrumb renders `Início > Biblioteca indisponível` after the
  list finishes loading

#### Scenario: No duplicate breadcrumb

- **WHEN** the user is on `/libraries/1/series` or `/series/42/markers`
- **THEN** exactly one `<nav aria-label="Breadcrumb">` exists in the page

### Requirement: Quality gates

The frontend MUST build, lint and pass its tests, and the change MUST validate
under OpenSpec strict mode.

#### Scenario: Frontend gates pass

- **WHEN** `npm run build`, `npm run lint`, `npm run test:tokens` and
  `npm run test:sidebar` run in `vue/plex-frontend`
- **THEN** all commands exit with code 0

#### Scenario: Spec validates

- **WHEN** `openspec validate ui-sidebar-header --strict` runs
- **THEN** it reports no errors
