# Tasks — ui-sidebar-header

## Phase 1 — Setup & Planning

### 1.1 Infrastructure

- [ ] **Change root setup**
  - Create `openspec/changes/ui-sidebar-header/`
  - Create `.openspec.yaml` with schema `spec-driven` and created date
  - Create `proposal.md` (with summary, motivation, scope, acceptance criteria)
  - Create `specs/web-ui/spec.md` (with user stories, acceptance criteria)
  - Create `design.md` (with layout, color specs, component specs)
  - Create `tasks.md` (initial skeleton with 53 tasks from apply instructions)

- [ ] **Tailwind config update**
  - Update `vue/plex-frontend/tailwind.config.cjs`:
    - Fix `gray` palette ordering (50 lightest → 900 darkest)
    - Set `gray-900: #18181B` (was `#FFFFFF`)
    - Set semantic colors to GitHub values:
      - `primary: "#0969DA"`, `secondary: "#8B949E"`
      - `stroke: "#353B45"`, `body: "#C9D1D9"`
      - `bodydark: "#8B949E"`, `bodydark1: "#57606A"`, `bodydark2: "#353B45"`
    - Update `graydark: "#353B45"`, `whiten: "#FFFFFF"`, `whiter: "#F8F9FA"`
    - Update `boxdark: "#0D1117"`, `"boxdark-2": "#353B45"`
    - Update `meta` colors: `1: "#CF222E"` (danger red), `2: "#8B949E"`, etc.
    - Set `success: "#1A7F37"`, `danger: "#CF222E"`, `warning: "#9A6700"`

- [ ] **Frontend CSS update**
  - Update `vue/plex-frontend/src/styles/style.css`:
    - Light mode `:root` to GitHub Light palette
    - Dark mode `.dark` to GitHub Dark palette
    - Ensure `--surface`, `--border`, `--text`, `--text-secondary`, `--accent`, `--danger`, `--success`, `--warning` all GitHub

### 1.2 Create the header component

- [ ] **AppHeader Component**
  - Create/modify `vue/plex-frontend/src/components/AppHeader.vue`
  - Structure:
    ```
    <header class="header" data-theme="dark">
      <button class="hamburger" @click="toggleSidebar">Menu</button>
      <nav class="breadcrumb"><span>Selected Page</span></nav>
      <div class="spacer flex-1"></div>
      <button class="theme-toggle" @click="toggleDark">Toggle Dark Mode</button>
      <button class="settings-btn" @click="openSettings">Settings</button>
      <button class="profile-menu" @click="toggleProfile">Profile</button>
      <div class="profile-dropdown" v-if="showProfile">
        <button>Profile</button>
        <button>Logout</button>
      </div>
    </header>
    ```
  - Props/Reacts: `dark`, `toggleSidebar`, `toggleDark`, `openSettings`, `showProfile`
  - Computed: `themeIcon` based on `dark` state
  - Style: use CSS vars from theme (gitHub Theme colors)

- [ ] **AppSidebar Component**
  - Create/modify `vue/plex-frontend/src/components/AppSidebar.vue`
  - Content: only Libraries item
  ```
  <aside class="sidebar">
    <ul>
      <li class="nav-item">Libraries</li>
      <!-- Other items hidden: -->
      <!-- <li class="nav-item">Home</li> -->
      <!-- <li class="nav-item">Sync</li> -->
      <!-- <li class="nav-item">Settings</li> -->
    </ul>
  </aside>
  ```
  - Hamburger menu toggles: `sidebar-visible`, `sidebar-description-visible`

### 1.3 Create the settings modal

- [ ] **SettingsModal Component**
  - Create/modify `vue/plex-frontend/src/components/SettingsModal.vue`
  - Structure:
    ```
    <Teleport to="body">
      <div class="modal-overlay" v-if="isOpen" @click="closeModal">
        <div class="modal-content" @click.stop>
          <h2>Settings</h2>
          <div class="form-group">
            <label>Theme</label>
            <select>
              <option value="light">Light</option>
              <option value="dark">Dark</option>
            </select>
          </div>
          <div class="form-group">
            <label>New Password</label>
            <input type="password" v-model="newPassword">
          </div>
          <div class="form-group">
            <label>Confirm Password</label>
            <input type="password" v-model="confirmPassword">
          </div>
          <div class="actions">
            <button class="apply-btn" @click="apply">Apply</button>
            <button class="cancel-btn" @click="closeModal">Cancel</button>
          </div>
        </div>
      </div>
    </Teleport>
    ```
  - Props: `isOpen`, `closeModal`, `applySettings`, `dark`, `setDark`
  - Form validation: new password confirmation
  - Dark mode toggle applies to global theme

### 1.4 Execute token contrast tests

- [ ] **npm run test:tokens**
  - Ensure token contrast ratios pass
  - Report any failures with specific token names

### 1.5 Execute lint

- [ ] **npm run lint**
  - Resolve any lint warnings/errors
  - Follow project linting standards

## Phase 2 — Implementation

### 2.1 Create the hamburger menu functionality

- [ ] **Add hamburger toggle state**
  - Create `sidebar-visible`, `sidebar-description-visible` state in App.vue or AppLayout
  - Toggle with `setSidebarVisible()`, `setSidebarDescriptionVisible()`

- [ ] **Update App.vue/AppLayout.vue**
  - Import and use AppHeader and AppSidebar components
  - Provide theme context (global state for dark mode)
  - Implement hamburger functionality to show/hide sidebar

### 2.2 Implement the breadcrumb component

- [ ] **Breadcrumb Component**
  - Create `vue/plex-frontend/src/components/Breadcrumb.vue`
  - Props: `items`, `activeItem`
  - Render: `span.item.active` for active, `span.item` for others
  - Style: use CSS vars, hover effects, focus states

### 2.3 Implement the user profile menu

- [ ] **UserMenu Component**
  - Create `vue/plex-frontend/src/components/UserMenu.vue`
  - Props: `user`, `onProfile`, `onLogout`
  - State: `showMenu` boolean
  - Render avatar, dropdown on click

### 2.4 Implement the dark mode toggle

- [ ] **DarkModeToggle Component**
  - Create `vue/plex-frontend/src/components/DarkModeToggle.vue`
  - Props: `dark`, `toggleDark`
  - Render icon based on `dark` state (moon/sun)
  - Click toggles `toggleDark()` and updates `localStorage.theme.mode`

### 2.5 Implement settings modal

- [ ] **SettingsModal Component** (already listed in Phase 1.3)

### 2.6 Implement password change functionality

- [ ] **Password Change Logic**
  - Add password input fields to SettingsModal
  - Validate new password confirmation
  - Update user password (backend call to Plex API)
  - Show success/error messages

### 2.7 Build and test

- [ ] **npm run build**
  - Verify build passes
  - Container builds without errors

- [ ] **npm run lint**
  - Resolve any lint issues

- [ ] **npm run test:tokens**
  - Verify token contrast passes

## Phase 3 — QA

### 3.1 Spec validation

- [ ] **openspec validate ui-sidebar-header --strict**
  - Must pass with no errors

### 3.2 Tasks validation

- [ ] **tasks.md consistency**
  - All tasks `- [x]` or `- [ ]` correctly reflect completed work

### 3.3 Lint/format/build validation

- [ ] **Lint**
  - No lint errors

- [ ] **Build**
  - `npm run build` passes

- [ ] **Tests**
  - `npm run test:tokens` passes

### 3.4 Functional QA

- [ ] **UI Validation**
  - Visit http://localhost:8080/dashboard
  - Header contains hamburger, breadcrumb, profile menu, dark mode toggle, settings button
  - Sidebar displays only libraries
  - Dark mode toggle switches theme correctly
  - Settings modal opens on click
  - Hamburger menu shows/hides sidebar

- [ ] **Visual validation**
  - Header background matches GitHub Theme surface color
  - Dark mode panel backgrounds: `#353B45` (not white)
  - Light mode panel backgrounds: `#FFFFFF` (not blue-gray)

### 3.5 Docker acceptance

- [ ] **Docker build**
  - `docker build --no-cache -t plex-autoplay:ui-sidebar-header .`

- [ ] **Docker run**
  - `docker run plex-autoplay:ui-sidebar-header`
  - Validate UI at http://localhost:8080/dashboard

## Phase 4 — Loop if needed

If any QA check fails:
1. Review QA findings list
2. Create clean agents with only: spec, design, QA findings, directory
3. Fix, re-run lint/build/test, return to Phase 3
4. Max 5 iterations

## Deliverables

1. Updated `vue/plex-frontend/tailwind.config.cjs`
2. Updated `vue/plex-frontend/src/styles/style.css`
3. New components: AppHeader, AppSidebar, Breadcrumb, UserMenu, DarkModeToggle, SettingsModal
4. Modified: App.vue/AppLayout.vue (hamburger, state, theme)
5. `openspec/changes/ui-sidebar-header/qa-report-1.md` (QA report)
6. All `openspec validate --strict` checks pass

## Notes

- Theme must be GitHub Theme (VS Code GitHub Dark/Light palette)
- No blue-gray tones remaining in dark mode (`rgb(17 24 39)` must be 0 occurrences)
- Dark mode panel backgrounds: `#353B45` (was `#FFFFFF` — bug fix)
- Light mode surface: `#FFFFFF` (was blue-gray)
- Hamburger menu shows/hides sidebar and sidebar description visibility