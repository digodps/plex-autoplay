# Design

## Context

Achado [8] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: Prettier detectou diferenças de formatação em arquivos JS/Vue

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: tests/e2e/mock-plex/server.js, tests/e2e/tests/auth.spec.js, tests/e2e/tests/container.spec.js, tests/e2e/tests/images.spec.js, tests/e2e/tests/libraries.spec.js, tests/e2e/tests/markers.spec.js, tests/e2e/utils/env.js, vue/plex-frontend/src/App.vue, vue/plex-frontend/src/components/HelloWorld.vue, vue/plex-frontend/src/main.js, vue/plex-frontend/src/router/index.js, vue/plex-frontend/src/stores/auth.js, vue/plex-frontend/src/views/DashboardView.vue, vue/plex-frontend/src/views/LibrariesView.vue, vue/plex-frontend/src/views/LoginView.vue, vue/plex-frontend/src/views/SettingsView.vue, vue/plex-frontend/src/views/SyncView.vue, eslint.config.js, tests/e2e/tests/series-editor.spec.js, tests/e2e/utils/auth.js, vue/plex-frontend/src/api.js, vue/plex-frontend/src/components/ActionBar.vue, vue/plex-frontend/src/components/AppIcon.vue, vue/plex-frontend/src/components/MarkerRow.vue, vue/plex-frontend/src/components/SeriesCard.vue, vue/plex-frontend/src/components/SeriesPagination.vue, vue/plex-frontend/src/components/StatusCard.vue, vue/plex-frontend/src/stores/dashboard.js, vue/plex-frontend/src/stores/markers.js, vue/plex-frontend/src/views/MarkerEditorView.vue, vue/plex-frontend/src/views/SeriesView.vue

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
