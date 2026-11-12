# Deficiências Resolvidas — unificar-intervalo-sincronizacao

## Revisores
- **A (Arquitetura)**: Camadas, fronteiras, DI, responsabilidades
- **S (SOLID)**: SRP, OCP, LSP, ISP, DIP
- **C (Clean Code)**: Nomes, DRY, acoplamento, complexidade

---

## Achados

### A1 — Arquitetura: Dois BackgroundServices independentes violam fronteira de domínio
**Artefato**: `src/PlexAutoplay.Infrastructure/Sync/SyncBackgroundService.cs`, `CatalogSyncBackgroundService.cs`
**Achado**: Dois `BackgroundService` rodando em paralelo sem coordenação, cada um com seu intervalo próprio. O domínio exige **um ciclo** de duas etapas ordenadas.
**Classificação**: ACEITO
**Resolução**: Substituir por `SynchronizationBackgroundService` único que executa etapa 1 → etapa 2 sequencialmente no mesmo ciclo, lendo `SyncIntervalMinutes` do banco no momento do uso.

### A2 — Arquitetura: Configuração fragmentada (`SyncCatalogIntervalMinutes` legada)
**Artefato**: `AppSettings.cs`, `UpdateAppSettingsUseCase.cs`, `SettingsEndpoints.cs`, `SettingsView.vue`
**Achado**: `AppSettingKeys` tem `SyncCatalogIntervalMinutes` (chave legada não usada); `UpdateAppSettingsRequest` aceita `SyncCatalogIntervalMinutes`; Vue expõe dois campos (`sync_interval_minutes` e `sync_catalog_interval_minutes`). O spec exige **um único campo**.
**Classificação**: ACEITO
**Resolução**: Remover `SyncCatalogIntervalMinutes` de `AppSettingKeys.All`, `AppSettingsSnapshot`, `UpdateAppSettingsRequest`, validação, endpoints, Vue. Manter apenas `SyncIntervalMinutes`.

### A3 — Arquitetura: `SyncBackgroundService` usa `MediaPathResolver` e `IMarkerDetector` — responsabilidades misturadas
**Artefato**: `SyncBackgroundService.cs`
**Achado**: O serviço de sincronização periódica de marcadores acopla detecção (`IMarkerDetector`) e resolução de caminho (`IPathResolver`). A etapa 2 (marcadores) já existe como `EpisodeMarkerSyncUseCase` que delega para `LibraryScanUseCase` → melhor separação.
**Classificação**: ACEITO
**Resolução**: `SynchronizationBackgroundService` delega para `SynchronizationUseCase` que orquestra `CatalogSyncUseCase` (etapa 1) + `EpisodeMarkerSyncUseCase` (etapa 2).

### S1 — SRP: `SyncBackgroundService` faz agendamento + varredura + detecção
**Artefato**: `SyncBackgroundService.cs`
**Achado**: Uma classe que agenda (loop + `Task.Delay`), busca bibliotecas, busca episódios, resolve caminho, detecta marcadores, persiste. Viola Single Responsibility.
**Classificação**: ACEITO
**Resolução**: Separar agendamento (`SynchronizationBackgroundService`) de lógica de domínio (`SynchronizationUseCase`, `CatalogSyncUseCase`, `EpisodeMarkerSyncUseCase`).

### S2 — DIP: `CatalogSyncBackgroundService` e `SyncBackgroundService` leem configuração só no construtor
**Artefato**: `CatalogSyncBackgroundService.cs:36`, `SyncBackgroundService.cs:37`
**Achado**: `_intervalMinutes = configuration.GetValue(...)` no construtor — não reflete alterações em runtime. `IAppSettingsStore` permite leitura dinâmica.
**Classificação**: ACEITO
**Resolução**: `SynchronizationBackgroundService` lê `AppSettingsSnapshot` do banco (`IAppSettingsStore`) a cada ciclo (ou com cache curto) via `IAppSettingsStore.GetAsync()`.

### S3 — OCP: Endpoints de sincronização duplicam lógica
**Artefato**: `SyncEndpoints.cs` (`/api/plex/scan`), novo `POST /api/sync/run`
**Achado**: Lógica de varredura espalhada em endpoints e background services. Adicionar nova etapa exige mudar múltiplos locais.
**Classificação**: ACEITO
**Resolução**: Centralizar em `SynchronizationUseCase.RunAsync(jobId, ct)`. Endpoints apenas criam job e chamam o use case.

### C1 — Clean Code: Nomes inconsistentes (`SyncBackgroundService` vs `CatalogSyncBackgroundService`)
**Artefato**: Nomes de classes
**Achado**: `SyncBackgroundService` (marcadores) e `CatalogSyncBackgroundService` (catálogo) — nomes não comunicam ordem nem relação. `SynchronizationBackgroundService` deixa explícito que é o ciclo unificado.
**Classificação**: ACEITO
**Resolução**: Renomear/remover para `SynchronizationBackgroundService`.

### C2 — Clean Code: `SettingsView.vue` expõe dois campos para um conceito (intervalo do ciclo)
**Artefato**: `vue/plex-frontend/src/views/Pages/SettingsView.vue`
**Achado**: `sync_interval_minutes` e `sync_catalog_interval_minutes` — confunde operador; viola spec "vê apenas o campo Intervalo de Sincronização".
**Classificação**: ACEITO
**Resolução**: Remover `sync_catalog_interval_minutes` do form, `applySettings`, template.

### C3 — Clean Code: `AppSettingsSnapshot` tem campo não usado `SyncCatalogIntervalMinutes`
**Artefato**: `AppSettings.cs`
**Achado**: Record expõe propriedade que não é mais usada pelo domínio. Poluição de API.
**Classificação**: ACEITO
**Resolução**: Remover do record, store, seeder, use case, endpoints.

---

## Resumo
- Total achados: 8 (A:3, S:3, C:2)
- Todos **ACEITOS** — nenhuma rejeição ou parcial.
- Próximo passo: commit dos artefatos da change e apply instructions.