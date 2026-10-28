# Tasks

## 1. Correção do achado [14]

- [x] 1.1 Aplicar correção nos arquivos: vue/plex-frontend/src/views/SeriesView.vue e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

SeriesView.vue (454 → 195 linhas): estilo extraído para views/series.css, cards para components/SeriesCard.vue e paginação para components/SeriesPagination.vue. Classes CSS/HTML preservadas; E2E de UI verdes (23 passed).

## Workflow follow-up

- Validar com `openspec validate --change fix-14-clean-code-/-solid` antes de seguir para archive/apply.
