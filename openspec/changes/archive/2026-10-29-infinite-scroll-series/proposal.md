# Proposal

## Why

A navegação por páginas tradicionais (paginação com botões "Anterior/Próxima") na listagem de séries é inferior à experiência de "infinite scroll" estilo Netflix. O usuário precisa clicar, esperar carregar, perder o contexto visual. O infinite scroll com lazy loading de imagens proporciona descoberta contínua, mantém o estado visual e elimina a barra de rolagem nativa do container, criando uma experiência fluida e imersiva igual à Netflix.

## What Changes

- **Nova capacidade**: `series-infinite-scroll` - Infinite scroll sem barra de rolagem nativa no catálogo de séries
- **Modificação na `series-catalog`**: Remover paginação explícita (botões Anterior/Próxima), substituir por infinite scroll com IntersectionObserver
- **Frontend (vue/plex-frontend)**:
  - `LibrariesView.vue`: Remover controles de paginação da expansão inline de séries
  - Novo composable `useInfiniteScroll.js` com IntersectionObserver
  - CSS: ocultar scrollbar nativa (`scrollbar-width: none; -ms-overflow-style: none; ::-webkit-scrollbar { display: none; }`)
  - Lazy loading de imagens (`loading="lazy"`)
- **Backend**: Endpoint `/api/series/{sectionKey}` já suporta paginação server-side (page/pageSize) - sem mudanças necessárias

## Capabilities

### New Capabilities
- `series-infinite-scroll`: Infinite scroll contínuo no catálogo de séries estilo Netflix, com lazy loading de imagens e sem barra de rolagem visível

### Modified Capabilities
- `series-catalog`: Remover requisito de paginação explícita (botões Anterior/Próxima); adicionar requisito de infinite scroll contínuo

## Impact

**Frontend (vue/plex-frontend)**:
- `src/views/LibrariesView.vue` - Remover `series-pagination`, `series-page`, `goToSeriesPage`; adicionar `useInfiniteScroll`
- `src/composables/useLibrarySeries.js` - Substituir lógica de paginação por infinite scroll
- `src/composables/useInfiniteScroll.js` - Novo composable com IntersectionObserver
- CSS global para ocultar scrollbar

**Backend**: Nenhuma mudança (endpoint já suporta paginação)

**UX**: Experiência fluida, descoberta contínua, sem cliques desnecessários