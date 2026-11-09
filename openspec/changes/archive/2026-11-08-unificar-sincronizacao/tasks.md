# Tasks — unificar-sincronizacao

## Phase 1 — Requisitos e artefatos

- [x] **EGM A/S/C** — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [x] `openspec validate unificar-sincronizacao --strict` passa.
- [x] Commit somente dos artefatos da change.
- [x] Worktree `feature/spec-unificar-sincronizacao` criada e sessão movida.

## Phase 2 — Backend (Application + Infrastructure)

- [x] **Remover chaves antigas**: `AppSettingKeys.SyncCatalogIntervalMinutes` e
  `AppSettingDefaults.SyncCatalogIntervalMinutes`; `AppSettingsSnapshot`
  (campo + `Defaults`); `UpdateAppSettingsRequest` (campo + validação);
  `UpdateAppSettingsUseCase` (validação única); `SqliteAppSettingsStore`
  (GetAsync/SaveAsync/Seeder); `CompleteSetupUseCase` (salva só `SyncIntervalMinutes`).
- [x] **Settings endpoints**: `SettingsEndpoints.cs` `SettingsRequest` e
  `ToResponse` sem `sync_catalog_interval_minutes`.
- [x] **Caso de uso de orquestração** (Application):
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
- [x] **Background service unificado**:
  `src/PlexAutoplay.Infrastructure/Sync/SynchronizationBackgroundService.cs`
  que herda `BackgroundService`; loop lê settings do banco, aguarda
  `SyncIntervalMinutes`, chama `SynchronizationOrchestrator.RunAsync(jobId, ct)`,
  logs/métricas.
- [x] **Remover serviços antigos**: apagar
  `src/PlexAutoplay.Infrastructure/Sync/SyncBackgroundService.cs` e
  `src/PlexAutoplay.Infrastructure/Sync/CatalogSyncBackgroundService.cs`.
- [x] **Sync endpoints**: adicionar `POST /api/sync/run` em `SyncEndpoints.cs`
  que cria `ScanJob`, dispara `SynchronizationOrchestrator.RunAsync(jobId)` em
  `Task.Run`, retorna `{ job_id }`.
- [x] **Plex endpoints alias**: em `PlexEndpoints.cs`, `POST /api/plex/scan`
  delega para `POST /api/sync/run` (reutiliza `SynchronizationOrchestrator`
  via injeção, cria job, dispara e retorna `{ job_id }`); remove lógica inline
  de varredura duplicada.
- [x] **DI**: `ServiceCollectionExtensions.cs`:
  - remover `AddHostedService<SyncBackgroundService>()`,
    `AddHostedService<CatalogSyncBackgroundService>()`;
  - registrar os três casos de uso como `AddSingleton` e
    `services.AddHostedService<SynchronizationBackgroundService>();`.
- [x] **Migração de configuração**: `AppSettingsSeeder` sem `SyncCatalogIntervalMinutes`;
  chave antiga no banco ignorada silenciosamente (teste de loja: leitura sem exceção
  com `SYNC_CATALOG_INTERVAL_MINUTES` presente).
- [x] **Testes existentes afetados**: adaptar `SettingsPersistenceTests.cs` (instância
  de `AppSettingsSnapshot` e `UpdateAppSettingsRequest` sem `SyncCatalogIntervalMinutes`;
  expectativas de validação atualizadas).

## Phase 3 — Frontend

- [x] `vue/plex-frontend/src/views/Pages/SettingsView.vue`:
  - remover campo `sync_catalog_interval_minutes` (input, label, hint, validação).
  - `form` default `sync_interval_minutes: 5`; `loadSettings`/`saveSettings`
    só usam `sync_interval_minutes`.
  - API response/response de `PUT /api/settings` sem campo catálogo.
- [x] `vue/plex-frontend/src/views/Sync/SyncView.vue`:
  - `startScan` chama `POST /api/sync/run` (ou helper `api.syncRun()`).
  - UI inalterada (status/processados/pulados/erros/logs SSE funcionam igual).
- [x] `vue/plex-frontend/src/api.js` (ou módulo correspondente):
  - adicionar `syncRun()` → `apiFetch("/api/sync/run", { method: "POST" })`.
  - usar o helper em `SyncView.vue` e em `src/composables/useDashboardActions.js`.
- [x] Grep em `vue/` por `sync_catalog_interval_minutes` / `catalog-interval`:
  corrigir qualquer referência remanescente (dashboard/cards).
- [x] Rebuild frontend: `npm run build` em `vue/plex-frontend` e copiar `dist`
  para `src/PlexAutoplay.Web/wwwroot`.

## Phase 4 — Build

- [x] Testes unitários: `CatalogSyncUseCase`, `EpisodeMarkerSyncUseCase`,
  `SynchronizationOrchestrator` (ordem, falha etapa 1 impede etapa 2, logs
  cumulativos, cancelamento), settings único e `SettingsPersistenceTests` adaptado.
- [x] `dotnet build PlexAutoplay.slnx -c Release` passa.
- [x] `dotnet test` passa (falhas pré-existentes em `PlexModernClientDetectorTests` presentes na base).
- [x] `npm run lint` e `npm run build` passam em `vue/plex-frontend`.
- [x] `docker build --no-cache -f infra/Dockerfile -t plex-autoplay:unificar-sincronizacao .` passa.

## Phase 5 — QA

- [x] `openspec validate unificar-sincronizacao --strict` passa.
- [x] Container isolado: settings persistidas têm apenas `SYNC_INTERVAL_MINUTES`;
  página `/settings` mostra um único campo de intervalo.
- [x] Playwright em `http://localhost:8099`:
  - **Tema**: light/dark toggle aplicado corretamente em todos componentes.
  - **Menus**: abertura, navegação, fechamento, estado ativo.
  - **Sincronização**: abrir `/sync`, clicar **Iniciar Escaneamento**, job criado,
    status → running → completed, logs mostram etapa 1 (catálogo) e etapa 2 (marcadores),
    contagens cumulativas.
  - **Forçar sincronização**: botão funciona e job completo rodando etapas em ordem.
- [x] Evidência de URL acessada e screenshots/asserts (screenshot `/tmp/opencode/qa/unificar-sincronizacao.png`).
- [x] `qa-report-1.md` com PASS/FAIL e achados numerados.