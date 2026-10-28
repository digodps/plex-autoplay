# Tasks

## 1. Migração e persistência

- [x] 1.1 Adicionar propriedade `Origin` na entidade `Marker` e coluna `Origin TEXT NOT NULL DEFAULT 'manual'` via migração idempotente no boot (try/catch para duplicada) em `SqliteMarkerRepository`; verificar com teste de banco preexistente (criar tabela sem coluna, iniciar app, confirmar coluna e default aplicado).
- [x] 1.2 Atualizar queries do repositório (`GetAllAsync`, `GetByRatingKeyAsync`, `AddAsync`, `UpdateAsync`) para ler/gravar `Origin`; verificar com teste de roundtrip persistindo origins differentes.

## 2. API de marcadores (origem + edição)

- [x] 2.1 Gravar `Origin = "detected"` no scanner (`PlexEndpoints.cs` scan) e `Origin = "manual"` no POST de `/api/markers`; verificar com teste do endpoint POST retornando origem e scan unitário registrando detected.
- [x] 2.2 Implementar `PUT /api/markers/{ratingKey}/{kind}` com `StartMs`/`EndMs`, reusando `UpdateAsync`, reescrevendo `custom.json` e respondendo 404 se marcador não existe; verificar com teste xUnit (200 em existente, 404 em inexistente, custom.json atualizado).
- [x] 2.3 Expor `Origin` nas respostas de GET/POST/PUT mantendo POST com contrato atual (query string + body, 409 em duplicado) para não quebrar o E2E Playwright; verificar rodando os testes E2E existentes.

## 3. API de catálogo de séries

- [x] 3.1 Adicionar em `PlexApiClient` o acesso paginado a `/library/sections/{key}/all?type=show` com cabeçalhos `X-Plex-Container-Start`/`X-Plex-Container-Size`, extraindo ratingKey, title, summary, thumb e totalSize; verificar testes com resposta de exemplo do Plex Json.
- [x] 3.2 Criar `SeriesEndpoints.cs`: `GET /api/series/{sectionKey}?page&pageSize&query` (thumb reescrita para `/api/images/...`, filtro por título) retrorna 502 com mensagem amigável se Plex falhar; verificar com teste de integração e teste de Plex indisponível.
- [x] 3.3 Implementar `GET /api/series/{sectionKey}/{seriesKey}/episodes` via `allLeaves` (temporada, número, título, ratingKey, caminho local resolvido); verificar com teste da junção episódio-caminho local.

## 4. Frontend — shell e tema

- [x] 4.1 Adicionar símbolos de UI em `public/icons.svg` (home, tv, book, sync, settings, search, edit, trash, chevrons) e verificar renderização via uso `<svg><use href="...#id">` em uma view de teste.
- [x] 4.2 Estender `style.css` com tema minimalista light/dark (tokens de cor/spacing/tipografia) reutilizando variáveis existentes; verificar em dark e light.
- [x] 4.3 Criar shell de navegação no `App.vue` (sidebar fixo com ícones + labels, responsivo) e atualizar rotas: `/libraries/:key/series`, `/series/:seriesKey/markers` lazy-loaded; verificar navegação em todas as rotas autenticadas.

## 5. Frontend — página de séries

- [x] 5.1 Criar `SeriesView.vue`: grade de capas (contexto `loading="lazy"`, skeleton) com título e descrição (2 linhas), busca com debounce 300ms e paginação; verificar com dados reais do Plex e estado vazio/busca sem resultado.
- [x] 5.2 Conectar navegção card→editor (`/series/:seriesKey/markers`) e tratamento de erros 502 com retry; verificar fluxo manual e mockando falha de rede.

## 6. Frontend — editor de marcadores estilo Plex

- [x] 6.1 Criar store Pinia de marcadores (merge episódios×marcadores por ratingKey) consumindo GET `/api/series/.../episodes` + `/api/markers`; verificar união em teste de store.
- [x] 6.2 Implementar `MarkerEditorView.vue`: temporadas agrupadas, marcadores intro/créditos por episódio, badges de origem (manual/detected), badges de estado vazio por episódio; verificar renderização com marcadores mistos.
- [x] 6.3 Implementar edição inline com inputs de timecode, salvamento otimista via PUT e rollback otimista em 4xx/5xx, indicios Ambiente (toast/spinner local), sem `confirm()` nativo; verificar com teste de componente (salvar, falhar, rollback).
- [x] 6.4 Remover o antigo formulário de ratingKey manual de `MarkersView.vue` (redirect da rota antiga para a página de séries); verificar acessibilidade da rota substituída.

## 7. Frontend — sessão e autenticação

- [x] 7.1 Refatorar `stores/auth.js` para gerir token via cookie (persistente de 30 dias vs. cookie de sessão conforme flag `keepSigned`) em vez de `localStorage`, removendo o token no logout; verificar com teste de store (login persistente, login de sessão, logout limpa cookie).
- [x] 7.2 Adicionar guard global no router: rota interna sem autenticação → `/login?redirect=<rota>`; token inválido em cookie retorna 401 → limpar cookie e redirecionar ao login; verificar manualmente acessando rotas sem sessão.
- [x] 7.3 Adicionar checkbox "manter-me conectado" na `LoginView.vue` e no login bem-sucedido redirecionar à `redirect` da query (fallback `/dashboard`); verificar fluxo manual: login direto → dashboard; login após acesso bloqueado → volta à rota original.
- [x] 7.4 Migração do token existente: primeira visita com token em `localStorage` move-o para o cookie e limpa o `localStorage`; verificar manualmente com navegador que já tinha sessão anterior.

## 8. Verificação de integração

- [x] 8.1 Sessão E2E Playwright completa: login → sidebar → página de séries paginada → abrir série → editar/criar/remover marcador → confirmar em `custom.json` + SQLite com origem correta.
- [x] 8.2 E2E de sessão: acessar rota interna sem autenticação → login → retorno à rota original → "manter-me conectado" mantém a sessão em nova visita.
- [x] 8.3 Verificar performance com biblioteca grande (>=1k séries): respostas paginadas, sem travamento da UI (média <500ms por página no log de telemetria).
