# Design

## Authentication Workflow
1. Client POSTs password to `/auth/login`.
2. Server validates password.
3. Server issues JWT as `Set-Cookie` with `HttpOnly; Secure; SameSite=Lax`.
4. Client uses `credentials: 'include'` on all `apiFetch` calls.
5. `JwtAuthMiddleware` (existing) continues reading `auth_token` from cookie (or header as fallback if required).

## Changes
- Remove `keepSigned` from `LoginView.vue`, `auth.js` store.
- Update `api.js` to rely on browser cookie handling.
- Update `AuthEndpoints.cs` to set `auth_token` cookie.
