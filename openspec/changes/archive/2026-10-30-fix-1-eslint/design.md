# Design

## Context

Achado [1] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: ESLint detectou violações de estilo em arquivos JS/Vue

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: eslint.config.js, tests/e2e/mock-plex/server.js, vue/plex-frontend/src/api.js, vue/plex-frontend/src/composables/useInfiniteScroll.js, vue/plex-frontend/src/router/index.js, vue/plex-frontend/src/views/DashboardView.vue, vue/plex-frontend/src/views/LibrariesView.vue, vue/plex-frontend/src/views/LoginView.vue, vue/plex-frontend/vite.config.js, tests/e2e/test-login.js, tests/e2e/tests/infinite-scroll.spec.js, vue/plex-frontend/src/components/Breadcrumb.vue, vue/plex-frontend/src/composables/useSeriesCatalog.js, vue/plex-frontend/src/views/SeriesView.vue

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
