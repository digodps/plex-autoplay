# plex-auth Specification

## Purpose
Defines the behavior for stateless JWT authentication using a single admin password, with no session store and no token expiration.

## Requirements

### Requirement: Login with single password
The system SHALL authenticate a user by validating a submitted password against the `ADMIN_PASSWORD` environment variable and issue a JWT token.

#### Scenario: Successful login
- **WHEN** a user submits the correct `ADMIN_PASSWORD` to `POST /auth/login`
- **THEN** the system returns a stateless JWT token signed with `JWT_SECRET`

#### Scenario: Failed login
- **WHEN** a user submits an incorrect password to `POST /auth/login`
- **THEN** the system returns HTTP 401 Unauthorized

### Requirement: Stateless JWT validation
The system SHALL validate incoming JWT tokens by signature only, with no expiration check and no server-side session store.

#### Scenario: Valid JWT token
- **WHEN** a request includes `Authorization: Bearer <valid-jwt>`
- **THEN** the system validates the token signature with `JWT_SECRET` and allows the request

#### Scenario: Invalid JWT token
- **WHEN** a request includes a tampered or invalid JWT token
- **THEN** the system returns HTTP 401 Unauthorized

#### Scenario: Token with no expiration
- **WHEN** a JWT token was issued days ago and has no `exp` claim
- **THEN** the system still accepts it as long as the signature is valid

### Requirement: Protected API endpoints
The system SHALL require a valid JWT for all mutating endpoints and for reading library and marker data.

#### Scenario: Unauthenticated request to protected endpoint
- **WHEN** a request to `GET /api/libraries` has no Authorization header
- **THEN** the system returns HTTP 401 Unauthorized
