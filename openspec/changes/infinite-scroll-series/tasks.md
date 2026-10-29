# Tasks

## 1. Composable Infinite Scroll

- [x] 1.1 Criar `vue/plex-frontend/src/composables/useInfiniteScroll.js` com:
  - `useInfiniteScroll(options)` que retorna `{ sentinelRef, isLoading, hasMore, loadMore }`
  - `IntersectionObserver` observando elemento sentinela
  - Throttle/debounce interno (guard `isLoading`)
  - Callbacks `onLoadMore`, `onEndReached`
  - Cleanup automático no `onUnmounted`
  - **Verificação**: Arquivo criado, exporta função, sem erros de lint

- [x] 1.2 Adicionar testes unitários básicos (opcional) ou validar manualmente no browser
  - **Verificação**: Composable funciona isoladamente

## 2. Atualizar useLibrarySeries

- [x] 2.1 Remover estado de paginação explícita de `useLibrarySeries.js`:
  - Remover: `seriesPage`, `seriesPageSize`, `totalSeriesPages`, `goToSeriesPage`
  - Manter: `seriesList`, `seriesTotalSize`, `seriesQuery`, `seriesLoading`, `seriesError`, `expandedLibraryKey`
  - **Verificação**: Arquivo compila, sem referências a variáveis removidas

- [x] 2.2 Integrar `useInfiniteScroll` em `useLibrarySeries.js`:
  - Importar e chamar `useInfiniteScroll({ onLoadMore: loadNextPage, onEndReached: setHasMoreFalse })`
  - `loadNextPage`: incrementa página interna, chama API, concatena resultados em `seriesList`
  - `hasMore`: controla se deve tentar carregar mais
  - Reset ao buscar: `seriesList = []`, `page = 1`, `hasMore = true`
  - **Verificação**: Infinite scroll carrega páginas sequenciais ao rolar

- [x] 2.3 Ajustar `loadSeries` para página 1 (inicial) vs próximas:
  - Página 1: substitui `seriesList`
  - Próximas: concatena (`seriesList.value = [...seriesList.value, ...newItems]`)
  - **Verificação**: Lista cresce corretamente sem duplicatas

## 3. Atualizar LibrariesView.vue

- [x] 3.1 Remover controles de paginação do template:
  - Remover bloco `series-pagination` (botões Anterior/Próxima, indicador de página)
  - Remover import de `SeriesPagination` (já removido no redesign)
  - **Verificação**: Template compila, sem erros de componente não encontrado

- [x] 3.2 Adicionar elemento sentinela no final do grid:
  - No `.series-grid`, adicionar `<div ref="sentinelRef" class="sentinel" />` como último filho
  - Bind `ref="sentinelRef"` do `useInfiniteScroll`
  - **Verificação**: Sentinela renderizada como último filho do grid

- [x] 3.3 Adicionar CSS para ocultar scrollbar no container:
  - Wrapper `.series-grid-wrapper` com `overflow-y: auto` + scrollbar oculta (Firefox, WebKit, IE/Edge)
  - `.series-grid` mantém `display: grid`
  - **Verificação**: Scroll funciona, barra invisível em Chrome/Firefox/Safari/Edge

- [x] 3.4 Lazy loading de imagens:
  - Adicionar `loading="lazy"` nas tags `<img>` das séries
  - Placeholder shimmer enquanto carrega (já existe `.skeleton`)
  - **Verificação**: Imagens carregam só ao entrar na viewport

- [x] 3.5 Ajustar busca para resetar infinite scroll:
  - No watcher de `seriesQuery`: resetar `seriesList = []`, `page = 1`, `hasMore = true`
  - **Verificação**: Buscar termo novo → lista reinicia do início

## 4. CSS Global para Scrollbar

- [x] 4.1 Adicionar regras globais em `src/style.css` ou scoped em `LibrariesView.vue`:
  ```css
  .series-grid-wrapper {
    overflow-y: auto;
    scrollbar-width: none; /* Firefox */
    -ms-overflow-style: none; /* IE/Edge Legacy */
  }
  .series-grid-wrapper::-webkit-scrollbar {
    display: none; /* Chrome, Safari, Edge */
  }
  ```
  - **Verificação**: Scrollbar invisível em todos browsers principais

## 5. Build, Testes e Deploy

- [x] 5.1 `npm run build` em `vue/plex-frontend` - sem erros
  - **Verificação**: Build成功, arquivos gerados em `dist/`

- [x] 5.2 Copiar build para `src/PlexAutoplay.Web/wwwroot/` e `docker compose build`
  - **Verificação**: Imagem Docker criada sem erros

- [ ] 5.3 `docker compose up -d` e smoke test:
  - Login → Bibliotecas → clicar biblioteca → rolar até fim → carrega próxima página
  - Buscar termo → lista reinicia
  - Deep-link `/libraries?key=6` → auto-expande e carrega
  - Scrollbar invisível em Chrome, Firefox, Safari
  - **Verificação**: Todos cenários funcionam no container

## Workflow follow-up

- Validar specs com `openspec validate --specs`
- Arquivar change: `openspec archive infinite-scroll-series`