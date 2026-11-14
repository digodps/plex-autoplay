# Tasks

## 1. Frontend: AppLogo Component Reversion

- [ ] 1.1 Remove lightning bolt SVG path from `AppLogo.vue`
- [ ] 1.2 Remove CSS keyframes animation `electric-pulse`
- [ ] 1.3 Remove `@media (prefers-reduced-motion: reduce)` block
- [ ] 1.4 Remove random `animationDelay` ref and inline style
- [ ] 1.5 Verify existing `size` prop works correctly
- [ ] 1.6 Verify theme color inheritance works (light/dark mode)

## 2. Testing & Validation

- [ ] 2.1 Run `npm run lint` in `vue/plex-frontend` - ensure no lint errors
- [ ] 2.2 Run `npm run build` in `vue/plex-frontend` - ensure build passes
- [ ] 2.3 Test in browser: verify logo renders static (no animation)
- [ ] 2.4 Test in browser: verify theme switching (light/dark) works correctly
- [ ] 2.5 Test in browser: verify different logo sizes (24, 28, 32, 40) render correctly

## 3. Build & Integration

- [ ] 3.1 Run full build pipeline: `./scripts/build.sh`
- [ ] 3.2 Verify Docker image builds successfully
- [ ] 3.3 Run `openspec validate remover-efeito-logo --strict`