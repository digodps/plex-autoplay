# Tasks: dashboard-botoes-tamanhos

## Phase 1: Requirements & Artifacts

- [x] EGM A/S/C — 3 reviewers, findings triaged in `deficiencias_resolvidas.md`
- [x] `openspec validate dashboard-botoes-tamanhos --strict` passes
- [x] Commit only change artifacts
- [x] Worktree `feature/spec-dashboard-botoes-tamanhos` created and session moved

## Phase 2: Frontend Implementation

- [x] Define shared button classes in `style.css`: `.btn`, `.btn-md`, `.btn-primary`, `.btn-secondary`, `.btn-danger`, `.btn-warning` (ponytail: `.btn-sm` deferido)
- [x] Update `DashboardView.vue`: "Ver Log Completo" → `btn btn-md btn-secondary`
- [x] Update `DashboardView.vue`: "Iniciar Escaneamento" → `btn btn-md btn-primary`
- [x] Update `DashboardView.vue`: retry (total failure) → `btn btn-md btn-danger` + `:disabled` + focus-visible
- [x] Update `DashboardView.vue`: retry (library error) → `btn btn-md btn-warning` + `:disabled` + focus-visible
- [x] Verify dark mode, focus-visible, hover, disabled on all 4 buttons

## Phase 3: Build

- [x] `npm run lint` in `vue/plex-frontend` passes
- [x] `npm run build` in `vue/plex-frontend` passes
- [x] `dotnet build PlexAutoplay.slnx -c Release` passes
- [x] `docker compose -f infra/docker-compose.yml build` passes

## Phase 4: QA

- [x] Playwright validates: theme applied correctly (light/dark toggle)
- [x] Playwright validates: menus work (open/navigate/close/active state)
- [x] `openspec validate dashboard-botoes-tamanhos --strict` passes
- [x] All spec criteria have implementation
- [x] Tasks reflect actual completed work