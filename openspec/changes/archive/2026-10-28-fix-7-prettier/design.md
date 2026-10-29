# Design

## Context

Achado [7] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: Prettier detectou diferenças de formatação em arquivos JS/Vue

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: vue/plex-frontend/src/App.vue, vue/plex-frontend/src/api.js, vue/plex-frontend/src/main.js, vue/plex-frontend/src/router/index.js, vue/plex-frontend/src/stores/dashboard.js, vue/plex-frontend/src/stores/markers.js, vue/plex-frontend/src/views/DashboardView.vue, vue/plex-frontend/src/views/SeriesView.vue

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
