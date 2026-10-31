# Design

## Context
The application currently manages authentication via JWT tokens passed in the `Authorization` header for APIs and stored in an `auth_token` cookie as a fallback. When the backend rejects a token as unauthorized, the frontend clears the cookie and redirects to `/login`. Issues occur when cookies persist across login attempts, or when multiple cookies might exist (e.g., path mismatches), causing the backend to reject new authentication sessions.

## Goals / Non-Goals

**Goals:**
- Guarantee that login always establishes a fresh, clean session state by purging any stale authentication cookies immediately prior to successful login.
- Enhance cookie removal logic to be more robust.

**Non-Goals:**
- Changing the underlying JWT token structure.
- Changing the authentication middleware logic on the backend (existing logic handles the JWT validation correctly).

## Decisions

- **Pre-emptive Cookie Clearing:** Explicitly call `deleteCookie('auth_token')` in `LoginView.vue` *before* the POST request or *immediately upon successful* login but *before* setting the new cookie. This ensures no collisions.
- **Improved `deleteCookie`:** Modify `vue/plex-frontend/src/api.js` to ensure cookie deletion covers `path=/`.

## Risks / Trade-offs

- **Browser Behavior:** Some browsers might have security policies restricting cookie deletion if path/domain are not perfectly matched. The implementation will ensure standard `path=/` deletion.
