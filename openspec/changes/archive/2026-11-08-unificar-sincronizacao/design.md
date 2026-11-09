# Design — unificar-sincronizacao

## Problema

Dois `BackgroundService` independentes (`SyncBackgroundService` com marcadores a
cada `sync_interval_minutes` e `CatalogSyncBackgroundService` com catálogo a
cada `sync_catalog_interval_minutes`) rodam sem ordem garantida, duplicam
intervalos e o botão **Sincronizar** duplica a lógica de varredura em
`POST /api/plex/scan` sem sincronizar o catálogo.

## Solução

Um único processo de sincronização, executando **duas etapas ordenadas por
ciclo**:

1. **Etapa 1 — Catálogo (Plex → banco):** bibliotecas e séries (`plex-sync-catalog`).
2. **Etapa 2 — Marcadores (Plex → ffprobe → banco):** episódios das bibliotecas
   de séries (`plex-sync`).

Mesmo serviço é usado pelo agendamento periódico e pelo botão **Sincronizar**
(force sync).

## Contratos

### Casos de uso de orquestração (Application, cada classe ≤ 4 dependências)

```csharp
public sealed class CatalogSyncUseCase      // IPlexApiClient, ILibraryRepository, ISeriesRepository, ILogger
public sealed class EpisodeMarkerSyncUseCase // IPlexApiClient, LibraryScanUseCase, IAppSettingsStore, ILogger
public sealed class SynchronizationOrchestrator // CatalogSyncUseCase, EpisodeMarkerSyncUseCase, IScanJobStore, ILogger
```

`SynchronizationOrchestrator.RunAsync(jobId, CancellationToken ct)` executa em
ordem:

1. **Etapa 1 — catálogo:** `CatalogSyncUseCase` reutiliza a lógica atual de
   `CatalogSyncBackgroundService.RunSyncAsync`:
   `GetLibrariesAsync` → filtro `show` → upsert biblioteca (rating key, título,
   tipo, agente) + séries (rating key, título, sumário, capa, biblioteca) → hard
   delete do que sumiu no Plex.
2. **Etapa 2 — marcadores:** `EpisodeMarkerSyncUseCase` reutiliza
   `LibraryScanUseCase.ScanLibraryAsync` sobre episódios de cada biblioteca de
   séries, usando `MediaRoots` do settings.

Falha na etapa 1 cancela a etapa 2 e o job termina em `failed`.

Logs cumulativos por etapa (níveis `info`/`skip`/`error`) no `IScanJobStore` do
`jobId`, compatível com os endpoints de status/SSE existentes.
`SyncResult` expõe `Stage1Libraries`, `Stage1Series`, `Stage2Processed`,
`Stage2Skipped`, `Stage2Errors`, `Logs`.

### `SynchronizationBackgroundService` (Infrastructure/Sync)

`BackgroundService` que, no loop:

1. Lê `AppSettingsSnapshot` do banco (resolução no momento do uso, não só no start).
2. Aguarda `SyncIntervalMinutes` (único intervalo do ciclo).
3. Chama `SynchronizationUseCase.RunAsync(jobId, ct)`.

Registra `plex_catalog_sync_total` e `plex_scan_duration_ms` mantidos nos nomes
atuais de métricas.

### Endpoints

- `POST /api/sync/run` (novo): cria `ScanJob` com `jobId`, dispara
  `SynchronizationUseCase.RunAsync(jobId, ct)` em background, retorna
  `{ job_id }`. Usado pelo botão **Sincronizar** e por qualquer outro gatilho
  sob demanda.
- `GET /api/sync/status/{jobId}`, `GET /api/sync/log/stream/{jobId}`,
  `POST /api/sync/cancel/{jobId}`: inalterados (reutilizados pelo force sync).
- `POST /api/plex/scan`: mantido como alias legado que delega para
  `POST /api/sync/run` (mesmo `jobId`/fluxo), para não quebrar consumidores
  existentes.

### Configurações (`settings-management`)

Substitui `sync_interval_minutes` + `sync_catalog_interval_minutes` por um
único **`sync_interval_minutes`** (minutos, 1..1440, default 5) aplicado ao
ciclo completo (etapa 1 + etapa 2).

- `AppSettingKeys.All`: remove `SyncCatalogIntervalMinutes`; mantém
  `SyncIntervalMinutes`.
- `AppSettingsSnapshot`: remove `SyncCatalogIntervalMinutes`.
- `UpdateAppSettingsRequest`: remove `SyncCatalogIntervalMinutes`; validação de
  intervalo única.
- `SqliteAppSettingsStore`: lê/grava apenas `SyncIntervalMinutes` (com
  `GetInt(... defaults.SyncIntervalMinutes)`). Chave antiga `SYNC_CATALOG_INTERVAL_MINUTES`
  é ignorada se existir no banco existente.
- `AppSettingsSeeder`: semeia apenas `SyncIntervalMinutes` (env ou default 5).
- `CompleteSetupUseCase`: salva apenas `SyncIntervalMinutes`.
- Endpoint `PUT /api/settings` e resposta: campo único `sync_interval_minutes`.

### DI (`ServiceCollectionExtensions.cs`)

Remove `AddHostedService<CatalogSyncBackgroundService>()` e
`AddHostedService<SyncBackgroundService>()`; registra
`AddHostedService<SynchronizationBackgroundService>()` + singleton
`SynchronizationUseCase`.

### Frontend

- `SettingsView.vue`: um único campo numérico `Intervalo de Sincronização
  (minutos)` (`sync_interval_minutes`); remove o segundo campo de catálogo.
- `SyncView.vue`: chama `POST /api/sync/run` (mantém status/processados/pulados/
  erros/console SSE; o `jobStatus` agora reflete as duas etapas nos logs).
- `api.js`: adiciona `syncRun()` (POST `/api/sync/run`).

### Bancos/tabelas

Sem novas tabelas. Apenas alteração de chave de settings.

## Camadas

| Camada | Artefato |
|---|---|
| Application | `SynchronizationUseCase`, `SyncResult` |
| Infrastructure/Sync | `SynchronizationBackgroundService` (novo) |
| Infrastructure | remove `SyncBackgroundService.cs`, `CatalogSyncBackgroundService.cs` |
| Web | `SyncEndpoints` (`/api/sync/run`), `PlexEndpoints` (`/api/plex/scan` alias), DI |
| Frontend | `SettingsView.vue`, `SyncView.vue`, `api.js` |
| Testes | testes unitários do `SynchronizationUseCase`; ajuste testes que usam os dois serviços/intervalos |

## Riscos

- Usuário com intervalo de catálogo customizado (30) perde o valor: passa a ser
  o intervalo único do ciclo. Aceito: o ciclo agora faz as duas etapas, então
  um único intervalo é a unidade correta. Teste de loja garante leitura sem exceção
  com a chave antiga ainda no banco (valor ignorado).
- `POST /api/sync/run` executado enquanto um job agendado roda: jobs correm
  concorrentemente; o `scanJobs` armazena por `jobId` separado, sem perda de
  dados. Aceito.
- Testes e2e existentes que usam `/api/plex/scan`: mantidos via alias.

## Testes

- Unitário de `CatalogSyncUseCase.RunAsync`: upsert/atualiza/remove bibliotecas e séries; ordem dentro da etapa.
- Unitário de `EpisodeMarkerSyncUseCase.RunAsync`: processa episódios via `LibraryScanUseCase`, pega com marcadores, exceção de biblioteca é skip.
- Unitário de `SynchronizationOrchestrator.RunAsync` com mocks: etapa 1 antes da etapa 2; falha na etapa 1 impede etapa 2 e job `failed`; logs cumulativos no `IScanJobStore`; cancelamento pára próximo ponto seguro.
- Unitário de `UpdateAppSettingsUseCase` sem `sync_catalog_interval_minutes`.
- Teste de loja: leitura/gravação de `SyncIntervalMinutes` único.
- Build gate: `dotnet build`, `dotnet test`, `npm run lint`, `npm run build`,
  cópia para `wwwroot`, `docker build --no-cache`.
