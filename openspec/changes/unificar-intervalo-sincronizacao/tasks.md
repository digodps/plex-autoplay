# Tasks — unificar-intervalo-sincronizacao

- [x] 1. Backend: Remover `SyncCatalogIntervalMinutes` de `AppSettingKeys`, `AppSettingsSnapshot`, `UpdateAppSettingsRequest` e seeder/store.
- [x] 2. Frontend: Atualizar `SettingsView.vue` para remover o segundo campo (`sync_catalog_interval_minutes`), mantendo apenas `Intervalo de Sincronização (minutos)`.
- [x] 3. Backend/Sync: Unificar serviços de sincronização (`SynchronizationBackgroundService` + `SynchronizationUseCase` executando etapa 1 e etapa 2 em ciclo único).
- [x] 4. Testes: Criar `FakePlexServer` para testes de `PlexApiClient` e ajustar suite de testes.
- [x] 5. Build & QA: `dotnet build`, `npm run build`, `npm run lint`, `docker build`, Playwright (tema + menu) — PASS.
