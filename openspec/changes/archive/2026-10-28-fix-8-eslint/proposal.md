# Proposal

## Why

Correção do achado [8] da revisão de código: ESLint detectou violações de estilo em arquivos JS/Vue

## What Changes

- Corrigir o problema identificado: ESLint detectou violações de estilo em arquivos JS/Vue
- Arquivos afetados: src/PlexAutoplay.Web/wwwroot/assets/DashboardView-BH3HcE-i.js, src/PlexAutoplay.Web/wwwroot/assets/LibrariesView-BsqcSa4t.js, src/PlexAutoplay.Web/wwwroot/assets/MarkersView-Juu8izAa.js, src/PlexAutoplay.Web/wwwroot/assets/SettingsView-DxsOwD97.js, src/PlexAutoplay.Web/wwwroot/assets/index-Cg5Ih4_b.js, tests/e2e/mock-plex/server.js, tests/e2e/tests/auth.spec.js, tests/e2e/tests/images.spec.js, tests/e2e/tests/libraries.spec.js, tests/e2e/tests/markers.spec.js, vue/plex-frontend/src/App.vue, vue/plex-frontend/src/main.js, vue/plex-frontend/src/router/index.js, vue/plex-frontend/src/stores/auth.js, vue/plex-frontend/src/views/LibrariesView.vue, vue/plex-frontend/src/views/LoginView.vue, vue/plex-frontend/src/views/MarkersView.vue, vue/plex-frontend/src/views/SettingsView.vue, vue/plex-frontend/src/views/SyncView.vue, src/PlexAutoplay.Web/wwwroot/assets/DashboardView-CjSYUmQH.js, src/PlexAutoplay.Web/wwwroot/assets/LibrariesView-CBv1lFq2.js, src/PlexAutoplay.Web/wwwroot/assets/MarkerEditorView-Cq3A4BNz.js, src/PlexAutoplay.Web/wwwroot/assets/SeriesView-BRFrZqoY.js, src/PlexAutoplay.Web/wwwroot/assets/SettingsView-BuyO3kt5.js, src/PlexAutoplay.Web/wwwroot/assets/_plugin-vue_export-helper-CGgr_-gC.js, src/PlexAutoplay.Web/wwwroot/assets/index-DCLGfyQb.js, vue/plex-frontend/src/api.js, vue/plex-frontend/src/components/AppIcon.vue, vue/plex-frontend/src/stores/markers.js, vue/plex-frontend/src/views/MarkerEditorView.vue, vue/plex-frontend/src/views/SeriesView.vue

## Capabilities

### New Capabilities

- code-review-fix-8: Correção específica do achado [8] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/wwwroot/assets/DashboardView-BH3HcE-i.js, src/PlexAutoplay.Web/wwwroot/assets/LibrariesView-BsqcSa4t.js, src/PlexAutoplay.Web/wwwroot/assets/MarkersView-Juu8izAa.js, src/PlexAutoplay.Web/wwwroot/assets/SettingsView-DxsOwD97.js, src/PlexAutoplay.Web/wwwroot/assets/index-Cg5Ih4_b.js, tests/e2e/mock-plex/server.js, tests/e2e/tests/auth.spec.js, tests/e2e/tests/images.spec.js, tests/e2e/tests/libraries.spec.js, tests/e2e/tests/markers.spec.js, vue/plex-frontend/src/App.vue, vue/plex-frontend/src/main.js, vue/plex-frontend/src/router/index.js, vue/plex-frontend/src/stores/auth.js, vue/plex-frontend/src/views/LibrariesView.vue, vue/plex-frontend/src/views/LoginView.vue, vue/plex-frontend/src/views/MarkersView.vue, vue/plex-frontend/src/views/SettingsView.vue, vue/plex-frontend/src/views/SyncView.vue, src/PlexAutoplay.Web/wwwroot/assets/DashboardView-CjSYUmQH.js, src/PlexAutoplay.Web/wwwroot/assets/LibrariesView-CBv1lFq2.js, src/PlexAutoplay.Web/wwwroot/assets/MarkerEditorView-Cq3A4BNz.js, src/PlexAutoplay.Web/wwwroot/assets/SeriesView-BRFrZqoY.js, src/PlexAutoplay.Web/wwwroot/assets/SettingsView-BuyO3kt5.js, src/PlexAutoplay.Web/wwwroot/assets/_plugin-vue_export-helper-CGgr_-gC.js, src/PlexAutoplay.Web/wwwroot/assets/index-DCLGfyQb.js, vue/plex-frontend/src/api.js, vue/plex-frontend/src/components/AppIcon.vue, vue/plex-frontend/src/stores/markers.js, vue/plex-frontend/src/views/MarkerEditorView.vue, vue/plex-frontend/src/views/SeriesView.vue
- Referências:   Clean Code - formatação e estilo JS/Vue

## Resultado da aplicação

eslint.config.js (flat config) criado na raiz: recommended + vue flat/recommended, override CommonJS para tests/** e scripts/**, ignores de artefatos de build (wwwroot, dist, node_modules, test-results); regras puramente estéticas do vue/* desligadas por conflito com Prettier. Vars mortas removidas em 8 arquivos. ESLint: 0 erros / 0 warnings.
