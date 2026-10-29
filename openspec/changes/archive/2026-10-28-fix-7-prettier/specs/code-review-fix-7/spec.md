# Spec Delta

## Purpose

Corrigir o achado [7] da revisão de código: Prettier detectou diferenças de formatação em arquivos JS/Vue

## ADDED Requirements

### Requirement: Corrigir achado [7]
O sistema SHALL corrigir o problema: Prettier detectou diferenças de formatação em arquivos JS/Vue

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos vue/plex-frontend/src/App.vue, vue/plex-frontend/src/api.js, vue/plex-frontend/src/main.js, vue/plex-frontend/src/router/index.js, vue/plex-frontend/src/stores/dashboard.js, vue/plex-frontend/src/stores/markers.js, vue/plex-frontend/src/views/DashboardView.vue, vue/plex-frontend/src/views/SeriesView.vue
- **THEN** o achado [7] não é mais reportado na próxima execução de /code-review
