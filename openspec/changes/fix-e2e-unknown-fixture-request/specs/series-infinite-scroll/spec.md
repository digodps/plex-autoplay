# Spec: Series Infinite Scroll (E2E Fix)

## Purpose
Ensure E2E tests for infinite scroll correctly reference supported Playwright fixtures.

## Requirements

### Requirement: Correct Playwright fixture parameters in E2E tests
The test runner SHALL execute infinite scroll tests without fixture parameter errors.

#### Scenario: Running E2E test suite
- **WHEN** the Playwright test suite runs in the CI container
- **THEN** tests like `infinite-scroll.spec.js` execute without throwing "Test has unknown parameter" errors
