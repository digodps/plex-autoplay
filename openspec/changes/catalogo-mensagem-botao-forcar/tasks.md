# Tasks

## 1. Frontend - Composable `useSeriesCatalog.js`

- [ ] 1.1 Adicionar estados `syncLoading`, `syncMessage`, `syncType` para feedback de sincronização — verify: estados existem no returned object.
- [ ] 1.2 Implementar função `forceSyncCatalog()` que chama `POST /api/sync/catalog` — verify: função envia POST e retorna resultado com contagens.
- [ ] 1.3 Expor função `refreshCatalog()` que recarrega a lista de séries após sincronização — verify: `refresh` já existe, reutilizável.

## 2. Frontend - `SeriesView.vue`

- [ ] 2.1 Adicionar exibição de mensagem informativa quando `seriesTotalSize === 0` e não há erro — verify: mensagem aparece na tela vazia da biblioteca.
- [ ] 2.2 Adicionar botão "Forçar sincronização" visível quando o catálogo não foi processado — verify: botão renderizado no estado vazio.
- [ ] 2.3 Mostrar estado de carregamento no botão durante a sincronização — verify: botão desabilitado/loading durante o POST.
- [ ] 2.4 Exibir mensagem de sucesso/erro após a sincronização — verify: mensagens de feedback aparecem na tela.

## 3. Backend - Reutilizar endpoint existente

- [ ] 3.1 Confirmar que `POST /api/sync/catalog` está mapeado e funcional — verify: endpoint responde com contagens de bibliotecas e séries.
- [ ] 3.2 Não alterar lógica de backend (endpoint já existe, apenas consumido pelo frontend) — verify: nenhum arquivo de backend modificado.

## 4. Testes e verificação de integração

- [ ] 4.1 Executar build da solução (.NET) — verify: `dotnet build PlexAutoplay.slnx -c Release` passa.
- [ ] 4.2 Executar lint/build do frontend — verify: `npm ci` + `npm run build` em `vue/plex-frontend` passa.
- [ ] 4.3 Validar UI com Playwright: mensagem + botão funcionam — verify: acessar `http://localhost:<port>` e validar tema e menus.

## Workflow follow-up

- Executar `openspec validate catalogo-mensagem-botao-forcar --strict` para confirmar coerência.
- Arquivar a mudança após revisão e aprovação.