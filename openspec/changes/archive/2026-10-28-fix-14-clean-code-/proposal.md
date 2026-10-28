# Proposal

## Why

Correção do achado [14] da revisão de código: Classe com mais de 200 linhas (432) - possível violação do SRP (Responsabilidade Única)

## What Changes

- Corrigir o problema identificado: Classe com mais de 200 linhas (432) - possível violação do SRP (Responsabilidade Única)
- Arquivos afetados: vue/plex-frontend/src/views/SeriesView.vue

## Capabilities

### New Capabilities

- code-review-fix-14: Correção específica do achado [14] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: vue/plex-frontend/src/views/SeriesView.vue
- Referências:   SOLID - S (Single Responsibility Principle)

## Resultado da aplicação

SeriesView.vue (454 → 195 linhas): estilo extraído para views/series.css, cards para components/SeriesCard.vue e paginação para components/SeriesPagination.vue. Classes CSS/HTML preservadas; E2E de UI verdes (23 passed).
