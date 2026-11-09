# Tasks — unificar-sincronizacao

## Phase 1 — Requisitos e artefatos

- [x] **EGM A/S/C** — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [x] `openspec validate unificar-sincronizacao --strict` passa.
- [ ] Commit somente dos artefatos da change.
- [x] Worktree `feature/spec-unificar-sincronizacao` criada e sessão movida.

## Phase 2 — Backend (Application + Infrastructure)

- [ ] **Remover chaves antigas**: `AppSettingKeys.SyncCatalogIntervalMinutes` e
  `AppSettingDefaults.SyncCatalogIntervalMinutes`; `AppSettingsSnapshot`
  (campo + `Defaults`); `UpdateAppSettingsRequest` (campo + validação);
  `UpdateAppSettingsUseCase` (validação única); `SqliteAppSettingsStore`
  (GetAsync/SaveAsync/Seeder); `CompleteSetupUseCase` (salva só `SyncIntervalMinutes`).
- [ ] **Settings endpoints**: `SettingsEndpoints.cs` `SettingsRequest` e
  `ToResponse` sem `sync_catalog_interval_minutes`.
- [ ] **Caso de uso de orquestração** (Application):
  - `src/PlexAutoplay.Application/UseCases/CatalogSyncUseCase.cs`
    `(IPlexApiClient, ILibraryRepository, ISeriesRepository, ILogger)`:
    `GetLibrariesAsync` → filtro `show` → upsert biblioteca (rating key, título,
    tipo, agente) + upsert séries (rating key, título, sumário, capa, biblioteca)
    → hard delete órfãs (mesma lógica de `CatalogSyncBackgroundService.RunSyncAsync`).
  - `src/PlexAutoplay.Application/UseCases/EpisodeMarkerSyncUseCase.cs`
    `(IPlexApiClient, LibraryScanUseCase, IAppSettingsStore, ILogger)`:
    para cada biblioteca `show`, `GetEpisodesAsync` e
    `LibraryScanUseCase.ScanLibraryAsync(seeds, mediaRoots)`.
  - `src/PlexAutoplay.Application/UseCases/SynchronizationOrchestrator.cs`
    `(CatalogSyncUseCase, EpisodeMarkerSyncUseCase, IScanJobStore, ILogger)`:
    `RunAsync(jobId, CancellationToken ct)` executa etapa 1, depois etapa 2;
    falha etapa 1 impede etapa 2; logs cumulativos no `IScanJobStore` com nível
    `info`/`skip`/`error` e contagens; retorna `SyncResult`
    (`Stage1Libraries`, `Stage1Series`, `Stage2Processed`, `Stage2Skipped`,
    `Stage2Errors`, `Logs`).
- [ ] **Background service unificado**:
  `src/PlexAutoplay.Infrastructure/Sync/SynchronizationBackgroundService.cs`
  que herda `BackgroundService`; loop lê settings do banco, aguarda
  `SyncIntervalMinutes`, chama `SynchronizationOrchestrator.RunAsync(jobId, ct)`,
  logs/métricas.
- [ ] **Remover serviços antigos**: apagar
  `src/PlexAutoplay.Infrastructure/Sync/SyncBackgroundService.cs` e
  `src/PlexAutoplay.Infrastructure/Sync/CatalogSyncBackgroundService.cs`.
- [ ] **Sync endpoints**: adicionar `POST /api/sync/run` em `SyncEndpoints.cs`
  que cria `ScanJob`, dispara `SynchronizationOrchestrator.RunAsync(jobId)` em
  `Task.Run`, retorna `{ job_id }`.
- [ ] **Plex endpoints alias**: em `PlexEndpoints.cs`, `POST /api/plex/scan`
  delega para `POST /api/sync/run` (reutiliza `SynchronizationOrchestrator`
  via injeção, cria job, dispara e retorna `{ job_id }`); remove lógica inline
  de varredura duplicada.
- [ ] **DI**: `ServiceCollectionExtensions.cs`:
  - remover `AddHostedService<SyncBackgroundService>()`,
    `AddHostedService<CatalogSyncBackgroundService>()`;
  - registrar os três casos de uso como `AddSingleton` e
    `services.AddHostedService<SynchronizationBackgroundService>();`.
- [ ] **Migração de configuração**: `AppSettingsSeeder` sem `SyncCatalogIntervalMinutes`;
  chave antiga no banco ignorada silenciosamente (teste de loja: leitura sem exceção
  com `SYNC_CATALOG_INTERVAL_MINUTES` presente).
- [ ] **Testes existentes afetados**: adaptar `SettingsPersistenceTests.cs` (instância
  de `AppSettingsSnapshot` e `UpdateAppSettingsRequest` sem `SyncCatalogIntervalMinutes`;
  expectativas de validação atualizadas).

## Phase 3 — Frontend

- [ ] `vue/plex-frontend/src/views/Pages/SettingsView.vue`:
  - remover campo `sync_catalog_interval_minutes` (input, label, hint, validação).
  - `form` default `sync_interval_minutes: 5`; `loadSettings`/`saveSettings`
    só usam `sync_interval_minutes`.
  - API response/response de `PUT /api/settings` sem campo catálogo.
- [ ] `vue/plex-frontend/src/views/Sync/SyncView.vue`:
  - `startScan` chama `POST /api/sync/run` (ou helper `api.syncRun()`).
  - UI inalterada (status/processados/pulados/erros/logs SSE funcionam igual).
- [ ] `vue/plex-frontend/src/api.js` (ou módulo correspondente):
  - adicionar `syncRun()` → `apiFetch("/api/sync/run", { method: "POST" })`.
  - usar o helper em `SyncView.vue` e em `src/composables/useDashboardActions.js`.
- [ ] Grep em `vue/` por `sync_catalog_interval_minutes` / `catalog-interval`:
  corrigir qualquer referência remanescente (dashboard/cards).
- [ ] Rebuild frontend: `npm run build` em `vue/plex-frontend` e copiar `dist`
  para `src/PlexAutoplay.Web/wwwroot`.

## Phase 4 — Build

- [ ] Testes unitários: `CatalogSyncUseCase`, `EpisodeMarkerSyncUseCase`,
  `SynchronizationOrchestrator` (ordem, falha etapa 1 impede etapa 2, logs
  cumulativos, cancelamento), settings único e `SettingsPersistenceTests` adaptado.
- [ ] `dotnet build PlexAutoplay.slnx -c Release` passa.
- [ ] `dotnet test` passa.
- [ ] `npm run lint` e `npm run build` passam em `vue/plex-frontend`.
- [ ] `docker build --no-cache -f infra/Dockerfile -t plex-autoplay:unificar-sincronizacao .` passa.

## Phase 5 — QA

- [ ] `openspec validate unificar-sincronizacao --strict` passa.
- [ ] Container isolado: settings persistidas têm apenas `SYNC_INTERVAL_MINUTES`;
  página `/settings` mostra um único campo de intervalo.
- [ ] Playwright em `http://localhost:<port>`:
  - **Tema**: light/dark toggle aplicado corretamente em todos componentes.
  - **Menus**: abertura, navegação, fechamento, estado ativo.
  - **Sincronização**: abrir `/sync`, clicar **Iniciar Escaneamento**, job criado,
    status → running → completed, logs mostram etapa 1 (catálogo) e etapa 2 (marcadores),
    contagens cumulativas.
  - **Forçar sincronização**: botão funciona e job completo rodando etapas em ordem.
- [ ] Evidência de URL acessada e screenshots/asserts.
- [ ] `qa-report-1.md` com PASS/FAIL e achados numerados.