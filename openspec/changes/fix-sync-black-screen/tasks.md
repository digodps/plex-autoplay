# Tasks — fix-sync-black-screen

- [x] Investigação: reproduzir tela preta ao iniciar escaneamento (SSE/apiFetch/401)
- [x] Corrigir tratamento de erro do EventSource (onerror) em SyncView.vue
- [x] Corrigir tratamento de erro 401/apiFetch para não deixar tela branca/preta
- [x] Garantir tema light/dark aplicado nos componentes de sincronização
- [x] Validar menus/navegação (abertura, navegação, fechamento, estado ativo)
- [x] Build: npm run build (vue) + dotnet build -c Release + docker compose build
- [x] Lint: npm run lint em vue/plex-frontend
- [x] QA independente: Playwright contra http://localhost:<port>/sync (tema + menus + scan)
- [x] openspec validate fix-sync-black-screen --strict