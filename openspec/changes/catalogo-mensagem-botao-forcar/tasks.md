# Tasks

## 1. Frontend - Composable `useSeriesCatalog.js`

- [x] 1.1 Adicionar estados `syncLoading`, `syncMessage`, `syncType` para feedback de sincronização — verify: estados existem no returned object.
- [x] 1.2 Implementar função `forceSyncCatalog()` que chama `POST /api/sync/catalog` — verify: função envia POST e retorna resultado com contagens.
- [x] 1.3 Expor função `refreshCatalog()` que recarrega a lista de séries após sincronização — verify: `refresh` já existe, reutilizável.

## 2. Frontend - `SeriesView.vue`

- [x] 2.1 Adicionar exibição de mensagem informativa quando `seriesTotalSize === 0` e não há erro — verify: mensagem aparece na tela vazia da biblioteca.
- [x] 2.2 Adicionar botão "Forçar sincronização" visível quando o catálogo não foi processado — verify: botão renderizado no estado vazio.
- [x] 2.3 Mostrar estado de carregamento no botão durante a sincronização — verify: botão desabilitado/loading durante o POST.
- [x] 2.4 Exibir mensagem de sucesso/erro após a sincronização — verify: mensagens de feedback aparecem na tela.

## 3. Backend - Reutilizar endpoint existente

- [x] 3.1 Confirmar que `POST /api/sync/catalog` está mapeado e funcional — verify: endpoint responde com contagens de bibliotecas e séries.
- [x] 3.2 Não alterar lógica de backend (endpoint já existe, apenas consumido pelo frontend) — verify: nenhum arquivo de backend modificado.

## 4. Testes e verificação de integração

- [x] 4.1 Executar build da solução (.NET) — verify: `dotnet build src/PlexAutoplay.Web -c Release` passa (0 errors, 0 warnings).
- [x] 4.2 Executar lint/build do frontend — verify: `npm ci` + `npm run build` em `vue/plex-frontend` passa.
- [x] 4.3 Validar UI com Playwright: tema light/dark, menus abrir/navegar/fechar/estado ativo — verify: acessar `http://localhost:8080` e validar (6/7 checks PASS; biblioteca vazia não testada por ambiente sem Plex mock).

## Workflow follow-up

- Executar `openspec validate catalogo-mensagem-botao-forcar --strict` para confirmar coerência.
- Arquivar a mudança após revisão e aprovação.