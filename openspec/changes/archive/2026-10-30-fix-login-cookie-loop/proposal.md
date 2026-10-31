# Proposal

## Why

Currently, if an invalid or stale `auth_token` cookie exists, the system enters an infinite login loop. This happens because the frontend attempts a new login but may fail to properly overwrite the existing invalid cookie, or the backend continues to validate the stale cookie before the new one is accepted, leading to repeated 401 Unauthorized responses and redirects.

## What Changes

- **Automatic Cookie Cleansing**: The login process will now explicitly delete any existing `auth_token` cookie before attempting to set a new one upon successful authentication.
- **Robust Logout/Cleanup**: Enhancing the cookie deletion logic to ensure all potential variations (path, protocol) of the `auth_token` cookie are cleared.
- **Login Recovery**: Ensuring that a failed token validation always results in a clean state for the next login attempt, preventing the redirection loop.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `auth-session`: Added requirement to explicitly clear existing session state/cookies before establishing a new session during the login flow to prevent authentication loops.

## Impact

- `vue/plex-frontend/src/api.js`: Update `deleteCookie` and potentially `setCookie` for robustness.
- `vue/plex-frontend/src/stores/auth.js`: Modify `login` action to clear state before setting new token.
- `src/PlexAutoplay.Web/Middleware/JwtAuthMiddleware.cs`: Backend behavior check (already allows `/auth/login`, but ensures no side-effects from stale cookies during the login POST itself).
