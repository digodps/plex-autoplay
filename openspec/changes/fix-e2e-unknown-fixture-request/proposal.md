# Proposal

## Why

The E2E test suite fails in CI because Playwright raises an error about an unknown test fixture parameter `_request` in `infinite-scroll.spec.js`. Fixing this fixture parameter name enables successful execution of the continuous E2E test suite.

## What Changes

- Modify `tests/e2e/tests/infinite-scroll.spec.js` to replace invalid `_request` fixture parameter with valid `request` (or remove if unused).

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `series-infinite-scroll`: Fix E2E test definitions to correctly use Playwright test fixtures.

## Impact

- Test suite execution in CI (GitHub Actions container runner).
