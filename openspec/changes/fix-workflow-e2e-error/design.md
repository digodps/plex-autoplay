# Design: Fix Workflow E2E Test Execution

## Architecture

- Review container orchestration in `infra/docker-compose.test.yml`.
- Review Playwright test setup in `tests/e2e/`.

## Components

- Test executor container (`Dockerfile.test-executor`)
- Mock Plex service (`mock-plex/server.cjs`)
- Playwright test specs (`tests/e2e/tests/`)
