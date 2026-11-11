# Design: Fix E2E Unknown Fixture Parameter

## Approach

1. Locate `tests/e2e/tests/infinite-scroll.spec.js`.
2. Inspect test signatures using `_request`.
3. Replace `_request` with `request` or remove it if unused.
4. Run local or containerized tests to confirm green status.

## Technical Details

- **File**: `tests/e2e/tests/infinite-scroll.spec.js`
- **Lines**: 24, 89 (`test(..., async ({ page, _request }) => ...`)
- **Fix**: Change `_request` to `request` or omit it.
