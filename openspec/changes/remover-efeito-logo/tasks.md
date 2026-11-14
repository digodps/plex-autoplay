# Tasks

## 1. Frontend: AppLogo Component Reversion

- [x] 1.1 Remove lightning bolt SVG path from `AppLogo.vue`
- [x] 1.2 Remove CSS keyframes animation `electric-pulse`
- [x] 1.3 Remove `@media (prefers-reduced-motion: reduce)` block
- [x] 1.4 Remove random `animationDelay` ref and inline style (keep `defineProps` for `size`)
- [x] 1.5 Verify existing `size` prop works correctly
- [x] 1.6 Verify theme color inheritance works (light/dark mode)
- [x] 1.7 Delete orphan E2E test `tests/e2e/tests/qa-electric-logo-animation.spec.js`

## 2. Testing & Validation

- [x] 2.1 Run `npm run lint` in `vue/plex-frontend` - ensure no lint errors (AppLogo.vue clean; pre-existing unrelated errors)
- [x] 2.2 Run `npm run build` in `vue/plex-frontend` - ensure build passes
- [x] 2.3 Test in browser: verify logo renders static (no animation)
- [x] 2.4 Test in browser: verify theme switching (light/dark) works correctly
- [x] 2.5 Test in browser: verify different logo sizes (24, 28, 32, 40) render correctly

## 3. Build & Integration

- [x] 3.1 Run full build pipeline: `./scripts/build.sh`
- [x] 3.2 Verify Docker image builds successfully
- [x] 3.3 Run `openspec validate remover-efeito-logo --strict`