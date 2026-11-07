# UI — Sidebar & Header Spec (Delta)

## ADDED Requirements

### Requirement: 1. Header Structure

The header MUST contain the following elements: hamburger menu button, separator, breadcrumb with selected item, profile menu with avatar and logout button, dark mode toggle button, and settings button. All elements MUST be accessible and properly labeled.

#### Scenario: Header contains expected elements
- **WHEN** the user is on the dashboard page
- **THEN** the header must contain: hamburger menu button, separator, breadcrumb with selected item, profile menu with avatar and logout button, dark mode toggle button, and settings button. All elements MUST be accessible and properly labeled.

#### Scenario: Breadcrumb displays selected page
- **WHEN** the user is on a sub-page (e.g., libraries)
- **THEN** the breadcrumb must display the currently selected page text (e.g., "Libraries"). The text should match the active route/page.

#### Scenario: Profile menu displays avatar and logout
- **WHEN** the user is authenticated
- **THEN** the profile menu must display a user avatar and provide a logout button. Clicking logout must clear the session and redirect to the login page.

#### Scenario: Dark mode toggle switches theme
- **WHEN** the user clicks the dark mode toggle button
- **THEN** the theme must switch between light and dark modes. The toggle must persist the preference via localStorage.

#### Scenario: Settings button opens modal
- **WHEN** the user clicks the settings button
- **THEN** a modal dialog must open containing settings options. The modal must be closable by clicking outside or on a cancel button.

### Requirement: 2. Sidebar Structure

The sidebar MUST display only the Libraries navigation item. All other navigation items MUST be hidden or removed.

#### Scenario: Sidebar displays only libraries
- **WHEN** the user is on the dashboard
- **THEN** the sidebar must display only the Libraries navigation item. All other navigation items (Home, Sync, Settings) must be hidden or removed from the sidebar markup.

#### Scenario: Sidebar collapses via hamburger menu
- **WHEN** the user is in expanded sidebar mode
- **THEN** clicking the hamburger menu must collapse the sidebar to icon-only mode. Descriptions should be hidden, and only icons should be visible.

#### Scenario: Hamburger menu toggles sidebar visibility
- **WHEN** the sidebar is in collapsed mode
- **THEN** clicking the hamburger menu must expand the sidebar to show all navigation items. The transition should be smooth.

### Requirement: 3. GitHub Theme Colors

The system MUST use the GitHub Theme color palette for both light and dark modes. All blue-gray tones MUST be replaced with proper GitHub gray tones.

#### Scenario: Light mode uses GitHub Light palette
- **WHEN** the user is on the dashboard with light theme
- **THEN** the following CSS variables must be set: `--surface: #FFFFFF`, `--border: #D0D7DE`, `--text: #24292F`, `--text-secondary: #57606A`, `--accent: #0969DA`.

#### Scenario: Dark mode uses GitHub Dark palette
- **WHEN** the user is on the dashboard with dark theme
- **THEN** the following CSS variables must be set: `--surface: #0D1117`, `--border: #30363D`, `--text: #E6EDF3`, `--text-secondary: #8B949E`, `--accent: #58A6FF`.

#### Scenario: No old blue-dark in generated CSS
- **WHEN** the container is built and running
- **THEN** the generated CSS must have 0 occurrences of `rgb(17 24 39)` (old blue-dark). All dark mode backgrounds must use proper gray values.

#### Scenario: Tailwind gray palette correctly ordered
- **WHEN** the Tailwind config is applied
- **THEN** the gray palette must be correctly ordered from 50 (lightest) to 900 (darkest), with `gray-900: #18181B`. The old inverted ordering (gray-900: #FFFFFF) must be fixed.

#### Scenario: Settings modal functionality
- **WHEN** the user clicks the settings button
- **THEN** a modal dialog must open containing: password change form with new password and confirmation fields, a dark mode toggle, and Apply/Cancel buttons. The modal must be closable by clicking outside or on cancel.

### Requirement: 4. Implementation

All components MUST use the `.dark` CSS class for theme switching and MUST meet all technical requirements.

#### Scenario: All components use .dark CSS class
- **WHEN** the Vue components are rendered
- **THEN** all Vue components must properly use the `.dark` CSS class for theme switching. The `.dark` class must be toggled on the `<html>` element or a parent element.

#### Scenario: openspec validate --strict passes
- **WHEN** `openspec validate ui-sidebar-header --strict` is run
- **THEN** it must pass with no errors.

#### Scenario: Container builds and runs successfully
- **WHEN** `docker build --no-cache -t plex-autoplay:ui-sidebar-header .` completes
- **WHEN** `docker run plex-autoplay:ui-sidebar-header` starts
- **THEN** the UI must be accessible at http://192.168.18.5:8080/dashboard and display correct GitHub Theme colors.