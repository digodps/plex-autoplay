# Tasks — unificar-intervalo-sincronizacao

- [ ] 1. Backend: Remover `SyncCatalogIntervalMinutes` de `AppSettingKeys`, `AppSettingsSnapshot`, `UpdateAppSettingsRequest` e seeder/store.
- [ ] 2. Frontend: Atualizar `SettingsView.vue` para remover o segundo campo (`sync_catalog_interval_minutes`), mantendo apenas `Intervalo de Sincronização (minutos)`.
- [ ] 3. Backend/Sync: Unificar serviços de sincronização (`SynchronizationBackgroundService` + `SynchronizationUseCase` ou equivalente executando etapa 1 e etapa 2 em ciclo único).
- [ ] 4. Testes: Ajustar testes unitários e de integração para o único intervalo e ciclo de duas etapas.
- [ ] 5. Build & QA: Executar build, lint e validação via Playwright da tela de Ajustes e menu.
