# Design — unificar-intervalo-sincronizacao

## Problema
Dois intervalos (`sync_interval_minutes` e `sync_catalog_interval_minutes`) e dois `BackgroundService` independentes (`SyncBackgroundService` a cada 5 min e `CatalogSyncBackgroundService` a cada 30 min) rodam sem ordem garantida, duplicando configuração e violando o spec `synchronization` (um único processo de duas etapas governado por um único intervalo).

## Solução
Um único intervalo `sync_interval_minutes` que governe um ciclo de duas etapas ordenadas:
1. Etapa 1 — catálogo: busca bibliotecas no Plex (`CatalogSyncUseCase`).
2. Etapa 2 — marcadores: busca episódios das bibliotecas de séries e detecta marcadores (`EpisodeMarkerSyncUseCase`).

## Contratos

### Configurações
- `AppSettingKeys`: remove `SyncCatalogIntervalMinutes`; mantém `SyncIntervalMinutes`.
- `AppSettingsSnapshot`: campo único `SyncIntervalMinutes` (default 5, 1..1440).
- `UpdateAppSettingsRequest`: campo único `SyncIntervalMinutes`; validação única de intervalo.
- Endpoint `PUT /api/settings` e resposta: campo único `sync_interval_minutes`.
- Chave antiga `SYNC_CATALOG_INTERVAL_MINUTES` ignorada se existir no banco (retrocompatibilidade).

### Processamento (duas etapas ordenadas)
- Etapa 1: `CatalogSyncUseCase.RunAsync(ct)` — upsert bibliotecas e séries do Plex.
- Etapa 2: `EpisodeMarkerSyncUseCase.RunAsync(ct)` — varredura de episódios e detecção de marcadores.
- Falha na etapa 1 cancela etapa 2; job em `failed`.
- Mesmo ciclo reutilizado por agendamento periódico e `POST /api/sync/run`.

### DI
Registra `SynchronizationBackgroundService` (único `BackgroundService`, lê `SyncIntervalMinutes` do banco no momento do uso) e remove `CatalogSyncBackgroundService` e `SyncBackgroundService` como hosted services.

### Frontend
`SettingsView.vue`: um único campo numérico `Intervalo de Sincronização (minutos)` (`sync_interval_minutes`); remove o segundo campo de catálogo.

## Camadas
| Camada | Artefato |
|---|---|
| Application | `SynchronizationUseCase`, `SyncResult` (orquestra etapa 1 → etapa 2) |
| Infrastructure/Sync | `SynchronizationBackgroundService` (novo, único intervalo) |
| Infrastructure | remove `SyncBackgroundService.cs`, `CatalogSyncBackgroundService.cs` |
| Web | `SyncEndpoints` (`/api/sync/run`), DI |
| Frontend | `SettingsView.vue` |
| Testes | testes de `SynchronizationUseCase`, `UpdateAppSettingsUseCase`, loja `SyncIntervalMinutes` |
