# Tasks

- [ ] 1. `SeriesView.vue`: adicionar `libraryTitleLoading`, remover fallback
      `Biblioteca ${key}`, usar "Biblioteca indisponível" em falha.
- [ ] 2. `BreadcrumbDefault.vue`: suportar item `loading` com skeleton
      (`aria-hidden`).
- [ ] 3. `SeriesView.vue`: só renderizar grid após `!libraryTitleLoading`.
- [ ] 4. `SidebarArea.vue`/`SidebarItem.vue`: `match` por item e item ativo em
      `/libraries/:key/series` e `/series/:seriesKey/markers`.
- [ ] 5. `npm run lint`, `npm run build`, `npm run test:sidebar`.
- [ ] 6. `openspec validate series-view-loading-skeleton --strict`.
