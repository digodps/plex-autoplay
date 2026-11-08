# Spec Delta

## ADDED Requirements

### Requirement: One visual standard across all screens
The system SHALL render every authenticated screen with the same set of design tokens (colors, spacing, radii, shadows, typography) defined exclusively in `src/styles/tokens.css` and SHALL NOT define per-screen CSS files or duplicate token values outside that file.

#### Scenario: No per-screen CSS file exists
- **WHEN** any file under `src/views/` or `src/styles/components/` is inspected
- **THEN** no `.css` file is imported and no scoped `<style>` block contains design values; every color, spacing, radius, shadow, and transition is referenced via a CSS custom property from `tokens.css`.

#### Scenario: All screens share the same visual language
- **WHEN** the user navigates through Dashboard, Libraries, Series, Marker Editor, Settings, and Sync
- **THEN** headings, cards, buttons, inputs, and grid spacing use the same token‑based values on every screen.

#### Scenario: No color literal outside the token file
- **WHEN** any `.vue` or `.css` file other than `tokens.css` is inspected
- **THEN** it contains no hexadecimal or `rgba()` color literal, except data‑driven values and the semantic tokens (`--danger`, `--success`, `--warning`).

### Requirement: Global design-token source
The system SHALL expose all design variables in `tokens.css` and SHALL NOT allow any component to define raw color/size values.

#### Scenario: Tokens declared in `:root`
- **WHEN** `tokens.css` is inspected
- **THEN** it declares every design variable (`--bg`, `--surface`, `--text`, `--accent`, `--focus-ring`, `--radius`, `--radius-sm`, `--radius-md`, `--radius-lg`, `--space-1` through `--space-8`, `--shadow-sm`, `--shadow-1`, `--shadow-2`, `--speed`, `--ease`, `--mono`, `--font`) in `:root`.

#### Scenario: Dark theme overrides in `:root`
- **WHEN** `tokens.css` is inspected under `[data-theme="dark"]`
- **THEN** every token pair has a dark‑mode override (background, surface, text, accent, shadows, focus ring) and all contrast ratios remain WCAG AA (≥4.5:1 for text, ≥3:1 for non‑text).

### Requirement: No per-view/per-component CSS
The system SHALL NOT allow a `.vue` file under `src/views/` or `src/components/` to import a scoped CSS file or contain a `<style>` block with design values.

#### Scenario: No scoped style blocks in views
- **WHEN** any file under `src/views/` is scanned for `<style` or `.css` imports
- **THEN** no scoped style block contains a CSS custom property from `tokens.css` or a hex literal.

#### Scenario: No scoped style blocks in components
- **WHEN** any file under `src/components/` is scanned for `<style` or `.css` imports
- **THEN** no scoped style block contains a CSS custom property from `tokens.css` or a hex literal.

### Requirement: Breadcrumb navigation with clickable ancestors
The system SHALL render a breadcrumb trail at the top of every authenticated screen, showing the path from the application root to the current page (`Home → Ancestor → Current`), and SHALL make every non‑current item clickable to navigate back.

#### Scenario: Breadcrumb visible on authenticated screens
- **WHEN** the user is authenticated and visits any screen except the login screen
- **THEN** a breadcrumb bar is visible at the top, listing all ancestors of the current route.

#### Scenario: Clicking an ancestor navigates back
- **WHEN** the user clicks a non‑current breadcrumb item
- **THEN** the application navigates to that ancestor route without a full page reload.

#### Scenario: Current page is not clickable
- **WHEN** the breadcrumb displays the current page
- **THEN** that item is rendered as non‑interactive text with `aria-current="page"`.

### Requirement: Real breadcrumb titles for libraries and series
The breadcrumb SHALL display the actual library title for `/libraries/:key` and the actual series title for `/series/:seriesKey/markers`, using a small UI store populated when data loads. Until data loads, fall back to generic labels (`Bibliotecas`, `Séries`).

#### Scenario: Library title appears in breadcrumb
- **WHEN** the user visits `/libraries/:key` and the library data has loaded
- **THEN** the breadcrumb shows `Início → Bibliotecas → <library title>` instead of just `Início → Bibliotecas`.

#### Scenario: Series title appears in breadcrumb
- **WHEN** the user visits `/series/:seriesKey/markers` and the series data has loaded
- **THEN** the breadcrumb shows `Início → Bibliotecas → <series title> → Marcadores` instead of a generic label.

### Requirement: Theme toggle persisted via backend
The system SHALL allow the authenticated user to toggle between light, dark, and system-follow mode, with the choice persisted through the existing `GET /api/theme` / `PUT /api/theme` endpoints and the `localStorage` fallback.

#### Scenario: Mode toggle in top bar
- **WHEN** the user clicks the theme toggle button in the top bar
- **THEN** the mode cycles `system → light → dark → system`, the `data-theme` attribute on `<html>` updates immediately, and the choice is saved on the backend via `PUT /api/theme { "mode": "light" | "dark" | "system" }`.

#### Scenario: Persistence across sessions
- **WHEN** the user reloads the page or opens the application in a new browser tab
- **THEN** the previously saved mode is loaded from the backend and applied without requiring re‑selection.

### Requirement: Neutral editorial palette
The visual system SHALL use a grayscale palette only, with black primary actions in light mode and white primary actions in dark mode, and SHALL NOT use brand purple (`#7d2ae8`) or any other accent color.

#### Scenario: Primary actions in light mode
- **WHEN** the interface renders in light mode
- **THEN** primary actions have text color `#111113` and background `transparent`, achieving ≥4.5:1 contrast with the surface.

#### Scenario: Primary actions in dark mode
- **WHEN** the interface renders in dark mode
- **WHEN** the interface renders in dark mode
- **THEN** primary actions have text color `#f6f6f7` and background `transparent`, achieving ≥4.5:1 contrast with the surface.

### Requirement: Responsive grid collapses at ≤768px
The system SHALL render a fluid layout that adapts from desktop to mobile, collapsing all card/grid layouts to a single column at viewports of 768px or less, and SHALL keep all interactive controls at a minimum touch target of 44px.

#### Scenario: Mobile grid collapse
- **WHEN** the viewport width is 768px or less
- **THEN** card grids on Dashboard, Libraries, Series and Sync collapse to a single column.

#### Scenario: Touch targets
- **WHEN** the interface renders on a touch device
- **THEN** every button, link, and breadcrumb item has a hit area of at least 44×44px.

### Requirement: Fluid page transitions
The system SHALL apply a single fade/slide transition to every route change, defined once in the global stylesheet, and SHALL disable meaningful animation when `prefers-reduced-motion` is set.

#### Scenario: Route change fades
- **WHEN** the user navigates between any two screens
- **THEN** the old screen fades out and the new screen fades in using the same `--transition-fast` duration and `--ease` curve.

#### Scenario: Reduced motion is honored
- **WHEN** the operating system requests reduced motion
- **THEN** route changes occur without noticeable fade or motion.

### Requirement: E2E coverage
The system SHALL have Playwright E2E tests covering the new UI flows against the running application.

#### Scenario: Login and logout
- **WHEN** an E2E test runs the login flow
- **THEN** the test fills the password, submits, verifies the auth cookie, and confirms the redirect to `/dashboard`, and a separate test verifies logout clears the cookie and returns to `/login`.

#### Scenario: Breadcrumbs complete
- **WHEN** an E2E test runs the full breadcrumb flow
- **THEN** the test navigates through Dashboard → Libraries → Series → Marker Editor and clicks each ancestor breadcrumb to verify navigation back.

#### Scenario: Marker CRUD
- **WHEN** an E2E test runs the marker CRUD flow
- **THEN** the test creates an intro marker, verifies it appears in the list, edits it, and removes it, all with stable selectors.

#### Scenario: Scan + log
- **WHEN** an E2E test runs the scan + log flow
- **THEN** the test starts a scan from the dashboard, verifies the progress panel updates, and closes the scan after a reasonable interval.

#### Scenario: Settings + theme
- **WHEN** an E2E test runs the settings + theme flow
- **THEN** the test opens Settings, updates the sync interval, saves, toggles dark/light mode, and verifies the mode persists after reload.

### Requirement: Max iterations 5
The EGM/QA loop SHALL terminate after 5 correction iterations (configurable) and SHALL NOT continue looping indefinitely.

#### Scenario: Iteration limit respected
- **WHEN** the QA reviewer marks `FAIL` on the fourth iteration
- **THEN** the system records the iteration count and stops before the fifth attempt unless new fixes are applied, and reports the block with history of QA.