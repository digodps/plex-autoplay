# Tasks: multiplos-diretorios-configuracao

## Phase 1: Requirements & Artifacts

- [ ] EGM A/S/C — 3 reviewers, findings triaged in `deficiencias_resolvidas.md`
- [ ] `openspec validate multiplos-diretorios-configuracao --strict` passes
- [ ] Commit only change artifacts
- [ ] Worktree `feature/spec-multiplos-diretorios-configuracao` created and session moved

## Phase 2: Frontend Implementation

- [ ] `SettingsView.vue`: importar `MediaRootsField` de `@/components/MediaRootsField.vue`
- [ ] `SettingsView.vue`: remover `media_root` do objeto `form` e do input único
- [ ] `SettingsView.vue`: adicionar `const mediaRoots = ref([])`
- [ ] `SettingsView.vue`: `applySettings` lê `data.media_roots ?? []`
- [ ] `SettingsView.vue`: `saveSettings` envia `media_roots: mediaRoots.value`
- [ ] `SettingsView.vue`: template usa `<MediaRootsField v-model="mediaRoots" />` no bloco de mídia to media roots
- [ ] Verificar dark mode e focus no `MediaRootsField` já renderizado

## Phase 3: Build

- [ ] `npm run lint` in `vue/plex-frontend` passes
- [ ] `npm run build` in `vue/plex-frontend` passes
- [ ] `dotnet build PlexAutoplay.slnx -c Release` passes
- [ ] `./scripts/build.sh` passes

## Phase 4: QA

- [ ] Playwright validates: theme applied correctly (light/dark toggle)
- [ ] Playwright validates: menus work (open/navigate/close/active state)
- [ ] Playwright validates `/ajustes` shows `MediaRootsField` with media_roots loaded
- [ ] `openspec validate multiplos-diretorios-configuracao --strict` passes
- [ ] All spec criteria have implementation
- [ ] Tasks reflect actual completed work
