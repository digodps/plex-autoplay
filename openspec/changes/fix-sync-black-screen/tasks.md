# Tasks — fix-sync-black-screen

- [ ] Investigação: reproduzir tela preta ao iniciar escaneamento (SSE/apiFetch/401)
- [ ] Corrigir tratamento de erro do EventSource (onerror) em SyncView.vue
- [ ] Corrigir tratamento de erro 401/apiFetch para não deixar tela branca/preta
- [ ] Garantir tema light/dark aplicado nos componentes de sincronização
- [ ] Validar menus/navegação (abertura, navegação, fechamento, estado ativo)
- [ ] Build: npm run build (vue) + dotnet build -c Release + docker compose build
- [ ] Lint: npm run lint em vue/plex-frontend
- [ ] QA independente: Playwright contra http://localhost:<port>/sync (tema + menus + scan)
- [ ] openspec validate fix-sync-black-screen --strict
