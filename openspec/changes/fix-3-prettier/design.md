# Design

## Context

Achado [3] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: Prettier detectou diferenças de formatação em arquivos JS/Vue

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: vue/plex-frontend/src/App.vue, vue/plex-frontend/src/composables/useDashboardAggregators.js, vue/plex-frontend/src/composables/useDashboardCards.js, vue/plex-frontend/src/router/index.js, vue/plex-frontend/src/views/LibrariesView.vue

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
