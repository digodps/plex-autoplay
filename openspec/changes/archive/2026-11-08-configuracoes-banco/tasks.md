# Tasks — configuracoes-banco

## Fase 1 — Artefatos

- [x] EGM A/S/C com achados triados em `deficiencias_resolvidas.md`.
- [x] `openspec validate configuracoes-banco --strict` passa.
- [x] Commit somente dos artefatos da change.
- [x] Worktree `feature/spec-configuracoes-banco` criada.

## Fase 2 — Backend

- [x] Migração 7: tabela `AppSetting`; WAL e `busy_timeout`.
- [x] `AppSettingKeys`, `AppSettingDefaults`, `AppSettingsSnapshot` e `IAppSettingsStore` em Application.
- [x] `SqliteAppSettingsStore` com Dapper (`GetAsync`, `SaveAsync` transacional).
- [x] `AppSettingsSeeder`: seed único quando tabela vazia.
- [x] `UpdateAppSettingsUseCase` com validação de URL, intervalos e `media_root`.
- [x] `PlexRequestHandler` resolve `PLEX_URL`/`PLEX_TOKEN` por requisição.
- [x] `PlexApiClient` transiente via `IHttpClientFactory`, sem `BaseAddress`/token fixos.
- [x] `GetIdentityAsync` sem mutar `DefaultRequestHeaders`.
- [x] `ImageProxyService` resolve URL/token por chamada; cache key inclui URL.
- [x] `PlexAutoSkipBackgroundService` resolve URL/token por poll.
- [x] `SyncBackgroundService` resolve intervalo e `MEDIA_ROOT` por ciclo.
- [x] `CatalogSyncBackgroundService` resolve intervalo por ciclo.
- [x] Endpoints de bibliotecas/séries resolvem `MEDIA_ROOT` do store.
- [x] `GET/PUT /api/settings`; token nunca retornado.
- [x] Remover `PlexSaveEndpoint`, `ConfigEndpoints`, `IPlexConfigStore` e `PlexConfigStore`.
- [x] Remover `/api/plex/save` e `/api/plex/identity` da allowlist do middleware.
- [x] Ajustar `ArchitectureTests`.

## Fase 3 — Frontend

- [x] `SettingsView` carrega de `GET /api/settings`.
- [x] Formulário inclui URL/token do Plex, `MEDIA_ROOT` e intervalos.
- [x] Token em campo senha; vazio mantém token atual.
- [x] Salvar via `PUT /api/settings`; mensagens de sucesso/erro.
- [x] Remover configuração via `localStorage`.

## Fase 4 — Validação

- [x] Teste unitário do store, seed e validação.
- [x] `dotnet build` da solução.
- [x] `openspec validate configuracoes-banco --strict`.
- [x] `npm run lint` e `npm run build` no frontend.
- [x] QA com Playwright em `http://localhost:8080`: carregar/salvar settings, tema claro/escuro e menus.
- [x] `qa-report-1.md` com PASS/FAIL e evidências.
