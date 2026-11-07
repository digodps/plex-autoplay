# UI — Sidebar & Header Design

## Layout Structure

```
+--------------------------------------------------------------+
|  Header (dark mode toggle, breadcrumb, profile, settings)  |
|  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  ▲  |
|  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |  |
|  +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+ |
|                                                                 |
|  | Sidebar (libraries only)                                  |
|  |  • Libraries                                               |
|  |  • (other items hidden)                                    |
|  +-----------------------------------------------------------|
|                                                                 |
|  Main content area                                           |
+--------------------------------------------------------------+
```

## Color Palette

### Light Mode (`:root`)
| Variable | Value | Description |
|---|---|---|
| `--surface` | `#FFFFFF` | White |
| `--border` | `#D0D7DE` | GitHub light border |
| `--text` | `#24292F` | GitHub light text |
| `--text-secondary` | `#57606A` | GitHub muted text |
| `--accent` | `#0969DA` | GitHub blue |
| `--danger` | `#CF222E` | GitHub red |
| `--success` | `#1A7F37` | GitHub green |
| `--warning` | `#9A6700` | GitHub yellow |

### Dark Mode (`.dark`)
| Variable | Value | Description |
|---|---|---|
| `--surface` | `#0D1117` | GitHub canvas |
| `--border` | `#30363D` | GitHub dark border |
| `--text` | `#E6EDF3` | GitHub fg |
| `--text-secondary` | `#8B949E` | GitHub muted |
| `--accent` | `#58A6FF` | GitHub blue (lighter) |
| `--danger` | `#F85149` | GitHub red |
| `--success` | `#3FB950` | GitHub green |
| `--warning` | `#D29922` | GitHub yellow |

## Component Specifications

### AppHeader
- Fixed height, full width
- Left: hamburger menu (toggle sidebar)
- Center: breadcrumb with selected page
- Right: profile avatar, logout, dark mode toggle, settings button
- Background: `:root` var(--surface), dark mode `.dark` var(--surface)

### AppSidebar
- Fixed width, full height minus header
- Contains: Libraries item only
- Other items hidden via CSS `display: none` or removed from markup
- Hamburger menu toggles: sidebar visibility, description visibility
- Background: `:root` var(--surface), dark mode `.dark` var(--surface)

### Breadcrumb
- Display: flex, gap-2
- Active item: text var(--text) or var(--text-secondary)
- Inert non-active items

### UserMenu (Profile)
- Dropdown on click
- Items: Profile, Settings, Logout
- Settings opens modal

### Settings Modal
- Fixed position, overlay backdrop
- Sections: Password change, Appearance (dark mode toggle)
- Buttons: Apply, Cancel
- Close on escape or overlay click

### Tailwind Config Updates
- `gray` palette: 50→900 correct ordering (NOT inverted)
- `gray-900: #18181B` (was `#FFFFFF` — the bug fix)
- All semantic colors redefined to GitHub Theme values
- `primary: "#0969DA"`, `secondary: "#8B949E"`, etc.