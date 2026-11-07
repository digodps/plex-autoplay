# Change Proposal — ui-sidebar-header

## Description
Redesign the Plex Autoplay UI header/sidebar component with the following requirements:
- **Title bar** with hamburger menu button to toggle sidebar description visibility
- **Breadcrumb** showing the selected item on the right side
- **Profile menu** and **Logout** button on the far right
- **Dark mode** toggle button in the header
- **Settings** option that opens in a modal dialog
- **Sidebar** should display only the **libraries** list
- Theme should use **GitHub Theme** colors (matching VS Code GitHub Dark/Light)

## Motivation
User reported that dark mode menu/panel backgrounds appear light/white. New UI requirements for consistent GitHub Theme styling across all components.

## Why
This change is needed to fix the dark mode background issue (user complaint) and implement the new UI requirements for the sidebar/header as specified. The GitHub Theme restyling ensures visual consistency with VS Code GitHub Dark/Light themes.

## Scope
- Frontend Vue 3 components: AppHeader, AppSidebar, Breadcrumb, UserMenu
- Tailwind CSS configuration updates to GitHub Theme palette
- CSS variable updates for dark mode
- Modal dialog for settings

## Out of Scope
- Backend API changes
- Plex integration changes
- Database schema changes

## Acceptance Criteria
- Header renders with hamburger menu, breadcrumb, profile menu, dark mode toggle, settings button
- Sidebar displays only libraries section
- Dark mode uses GitHub Theme palette (#0d1117 surface, #353b45 panels)
- Light mode uses GitHub Light palette
- Settings opens in modal with form fields
- All `openspec validate --strict` checks pass