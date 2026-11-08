# Tasks — configuracoes-banco

## Fase 1 — Artefatos

- [ ] EGM A/S/C com achados triados em `deficiencias_resolvidas.md`.
- [ ] `openspec validate configuracoes-banco --strict` passa.
- [ ] Commit somente dos artefatos da change.
- [ ] Worktree `feature/spec-configuracoes-banco` criada.

## Fase 2 — Backend

- [ ] Migração 7: tabela `AppSetting`; WAL e `busy_timeout`.
- [ ] `AppSettingKeys`, `AppSettingDefaults`, `AppSettingsSnapshot` e `IAppSettingsStore` em Application.
- [ ] `SqliteAppSettingsStore` com Dapper (`GetAsync`, `SaveAsync` transacional).
- [ ] `AppSettingsSeeder`: seed único quando tabela vazia.
- [ ] `UpdateAppSettingsUseCase` com validação de URL, intervalos e `media_root`.
- [ ] `PlexRequestHandler` resolve `PLEX_URL`/`PLEX_TOKEN` por requisição.
- [ ] `PlexApiClient` transiente via `IHttpClientFactory`, sem `BaseAddress`/token fixos.
- [ ] `GetIdentityAsync` sem mutar `DefaultRequestHeaders`.
- [ ] `ImageProxyService` resolve URL/token por chamada; cache key inclui URL.
- [ ] `PlexAutoSkipBackgroundService` resolve URL/token por poll.
- [ ] `SyncBackgroundService` resolve intervalo e `MEDIA_ROOT` por ciclo.
- [ ] `CatalogSyncBackgroundService` resolve intervalo por ciclo.
- [ ] Endpoints de bibliotecas/séries resolvem `MEDIA_ROOT` do store.
- [ ] `GET/PUT /api/settings`; token nunca retornado.
- [ ] Remover `PlexSaveEndpoint`, `ConfigEndpoints`, `IPlexConfigStore` e `PlexConfigStore`.
- [ ] Remover `/api/plex/save` e `/api/plex/identity` da allowlist do middleware.
- [ ] Ajustar `ArchitectureTests`.

## Fase 3 — Frontend

- [ ] `SettingsView` carrega de `GET /api/settings`.
- [ ] Formulário inclui URL/token do Plex, `MEDIA_ROOT` e intervalos.
- [ ] Token em campo senha; vazio mantém token atual.
- [ ] Salvar via `PUT /api/settings`; mensagens de sucesso/erro.
- [ ] Remover configuração via `localStorage`.

## Fase 4 — Validação

- [ ] Teste unitário do store, seed e validação.
- [ ] `dotnet build` da solução.
- [ ] `openspec validate configuracoes-banco --strict`.
- [ ] `npm run lint` e `npm run build` no frontend.
- [ ] QA com Playwright em `http://localhost:8080`: carregar/salvar settings, tema claro/escuro e menus.
- [ ] `qa-report-1.md` com PASS/FAIL e evidências.
