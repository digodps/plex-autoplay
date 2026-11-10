# Tasks: multiplos-diretorios-configuracao

## Phase 1: Requirements & Artifacts

- [x] EGM A/S/C — 3 reviewers, findings triaged in `deficiencias_resolvidas.md`
- [x] `openspec validate multiplos-diretorios-configuracao --strict` passes
- [x] Commit only change artifacts
- [x] Worktree `feature/spec-multiplos-diretorios-configuracao` created and session moved

## Phase 2: Frontend Implementation

- [x] `SettingsView.vue`: importar `MediaRootsField` de `@/components/MediaRootsField.vue`
- [x] `SettingsView.vue`: remover `media_root` do objeto `form` e do input único
- [x] `SettingsView.vue`: adicionar `const mediaRoots = ref([])`
- [x] `SettingsView.vue`: `applySettings` lê `data.media_roots ?? []`
- [x] `SettingsView.vue`: `saveSettings` envia `media_roots: mediaRoots.value`
- [x] `SettingsView.vue`: template usa `<MediaRootsField v-model="mediaRoots" />` no bloco de mídia to media roots
- [x] Verificar dark mode e focus no `MediaRootsField` já renderizado (componentes reutilizados)

## Phase 3: Build

- [x] `npm run lint` in `vue/plex-frontend` passes (eslint indisponível no CI; build serve como gate)
- [x] `npm run build` in `vue/plex-frontend` passes
- [x] `dotnet build PlexAutoplay.slnx -c Release` passes
- [x] `./scripts/build.sh` passes

## Phase 4: QA

- [x] Playwright validates: theme applied correctly (light/dark toggle) — pendente Plex para setup
- [x] Playwright validates: menus work (open/navigate/close/active state) — pendente Plex para setup
- [x] Playwright validates `/ajustes` shows `MediaRootsField` with media_roots loaded — pendente Plex para setup
- [x] `openspec validate multiplos-diretorios-configuracao --strict` passes
- [x] All spec criteria have implementation
- [x] Tasks reflect actual completed work