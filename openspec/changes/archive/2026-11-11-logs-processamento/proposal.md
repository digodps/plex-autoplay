# Change Proposal — logs-processamento

## Why

Atualmente os logs de varredura (scan jobs) são mantidos apenas em memória (`ScanJobStore` usando `ConcurrentDictionary`). Ao reiniciar o container, todo o histórico é perdido. Não há persistência, não há limpeza automática de registros antigos, e a UI (`SyncView.vue`) mostra apenas o job atual via SSE sem histórico, agrupamento ou filtro.

O operador precisa:
- Ver histórico de execuções passadas (persistido no SQLite)
- Limpar logs manualmente quando desejar
- Limpeza automática diária removendo registros > 30 dias
- Na tela de logs: visualização agrupada por data, status e tipo (level)
- Filtro de texto em tempo real (live) sobre mensagens

## What Changes

### Backend (C# / .NET / SQLite)

- **Nova tabela** `ScanJobLog` no SQLite para persistir entradas de log (job_id, timestamp, level, message, suggestion)
- **Nova tabela** `ScanJob` para persistir metadados do job (job_id, status, processed, skipped, errors, error_message, created_at, updated_at)
- **Migração** adicionada ao `DatabaseInitializer` (versão 8)
- **Nova implementação** `SqliteScanJobStore : IScanJobStore` substituindo a versão em memória
- **Métodos adicionais** na interface `IScanJobStore`:
  - `Task ClearLogsAsync()` — limpa todos os logs do BD
  - `Task CleanupOldLogsAsync(int retentionDays = 30)` — remove registros > N dias
  - `Task<List<ScanJob>> GetHistoryAsync(int limit = 100)` — histórico paginado
  - `Task<List<ScanLogEntry>> GetLogsAsync(string? jobId, DateTime? from, DateTime? to, string? level, string? textFilter)` — query flexível
- **BackgroundService** `LogCleanupBackgroundService` executando `CleanupOldLogsAsync(30)` uma vez por dia
- **Novos endpoints** em `SyncEndpoints`:
  - `GET /api/sync/history` — lista jobs passados
  - `GET /api/sync/logs` — query de logs com filtros (jobId, from, to, level, text)
  - `POST /api/sync/logs/clear` — limpa todos os logs
  - `POST /api/sync/logs/cleanup` — executa limpeza manual (retentionDays opcional)

### Frontend (Vue 3 / Tailwind / Pinia)

- **Nova view** `LogsView.vue` em `/logs` (rota nova) — tela dedicada de histórico de logs
- **Componentes**:
  - `LogFilterBar.vue` — filtros: busca texto (live/debounced), seletor de level, seletor de data (range), agrupamento (data/status/tipo)
  - `LogTable.vue` / `LogGroupedView.vue` — renderização agrupada com virtualização
  - `LogStatsCards.vue` — cards de resumo (total, erros, warnings, hoje)
- **Store** `logs.js` — estado reativo, debounce de busca, paginação
- **Integração no menu** — novo item "Logs" no Sidebar / breadcrumb
- **Tema** — respeita light/dark toggle existente (`useThemeStore`)

### Testes

- **E2E Playwright** (`tests/e2e/tests/logs.spec.js`):
  - Navega para `/logs`
  - Verifica tema aplicado (light/dark toggle)
  - Verifica menus funcionando (abertura/navegação/fechamento/estado ativo)
  - Testa filtro de texto live
  - Testa agrupamento por data/status/tipo
  - Testa botão "Limpar Logs" e confirmação
- **Unitários** xUnit para `SqliteScanJobStore` e `LogCleanupBackgroundService`

## Capabilities

### New Capabilities

- `log-persistence`: persistência de jobs e logs de varredura no SQLite
- `log-cleanup`: limpeza agendada diária e manual de logs antigos (>30 dias)
- `log-ui`: tela de logs com agrupamento, filtros live e ações de limpeza

### Modified Capabilities

- `plex-sync`: endpoints de log/histórico expandidos; `IScanJobStore` agora persiste

## Impact

**Backend:**
- `src/PlexAutoplay.Domain/Ports/IScanJobStore.cs` — interface expandida
- `src/PlexAutoplay.Infrastructure.Persistence/DatabaseInitializer.cs` — migração v8
- `src/PlexAutoplay.Infrastructure.Persistence/Repositories/SqliteScanJobStore.cs` — nova implementação
- `src/PlexAutoplay.Infrastructure.Sync/LogCleanupBackgroundService.cs` — novo serviço
- `src/PlexAutoplay.Web/Endpoints/SyncEndpoints.cs` — novos endpoints
- `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs` — registra novo store e background service

**Frontend:**
- `vue/plex-frontend/src/views/Logs/LogsView.vue` — nova view
- `vue/plex-frontend/src/components/Logs/` — componentes de UI
- `vue/plex-frontend/src/stores/logs.js` — store Pinia
- `vue/plex-frontend/src/router/index.js` — nova rota `/logs`
- `vue/plex-frontend/src/components/Sidebar/SidebarArea.vue` — item de menu

**Testes:**
- `tests/e2e/tests/logs.spec.js`
- `tests/unit/Infrastructure/Persistence/SqliteScanJobStoreTests.cs`

**Infra:**
- `scripts/build.sh` — já cobre build completo
- `infra/docker-compose.yml` — sem mudanças

## Goals / Non-Goals

**Goals**
- Persistência total de histórico de scan jobs e logs
- Limpeza automática diária (retention 30 dias) + manual
- UI de logs com agrupamento (data/status/tipo) + filtro texto live
- Tema light/dark e menus funcionando validados via Playwright

**Non-Goals**
- Exportação de logs (CSV/JSON) — futuro
- Alertas baseados em log — futuro
- Dashboard de métricas avançado — coberto por `telemetry` spec

## Rollback Plan

Reverter migração v8 (drop tables `ScanJob`, `ScanJobLog`), restaurar `ScanJobStore` em memória, remover background service e endpoints, remover view/frontend components.