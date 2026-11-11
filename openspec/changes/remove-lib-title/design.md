# Design

## Context

A página de séries (`src/views/Series/SeriesView.vue`) renderiza seu próprio título de página ("Biblioteca → Animes") com subtítulo ("Biblioteca selecionada.") no topo do conteúdo, além da migalha de pão do header renderizada por `AppHeader` (via `buildBreadcrumb` em `DefaultLayout.vue`). Isso duplica a informação da biblioteca atual.

## Goals / Non-Goals

**Goals:**
- Remover o bloco de título de página e subtítulo de `SeriesView.vue`.
- Manter a migalha de pão do header (AppHeader breadcrumb) intacta e funcional.

**Non-Goals:**
- Alterar `buildBreadcrumb`, `DefaultLayout.vue`, `AppHeader.vue`, rotas ou endpoints.
- Alterar qualquer outra view.

## Decisions

1. **Remover apenas o bloco `<div class="border-b ...">` (h1 + p) de SeriesView.vue.** Rationale: é o título de página redundante. A migalha de pão do header já comunica o contexto "Bibliotecas > Animes".
2. **Não tocar no breadcrumb.js nem no AppHeader.** Rationale: a migalha de pão do header é o único elemento desejado e já funciona.
