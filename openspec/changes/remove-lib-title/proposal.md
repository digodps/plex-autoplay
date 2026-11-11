# Proposal: Remover título de biblioteca da página de séries

## Why

A página de séries exibe um título duplicado ("Biblioteca → Animes") com subtítulo "Biblioteca selecionada." logo abaixo do breadcrumb do header (AppHeader, migalha de pão "Bibliotecas > Animes"). Essa redundância polui a tela; o usuário só precisa da migalha de pão do header para saber onde está.

## What Changes

- Remover o bloco de título de página (`<h1>Biblioteca → {título}</h1>` + `<p>Biblioteca selecionada.</p>`) de `src/views/Series/SeriesView.vue`.
- Manter apenas a migalha de pão do header (AppHeader, gerada por `buildBreadcrumb`).

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `series-page-view`: página de séries deixa de duplicar a biblioteca no título da página.

## Impact

- Apenas `src/views/Series/SeriesView.vue` (template). Sem alteração de rotas, endpoints, bancos ou breadcrumb.js.
