# Design — logs-processamento

## Context

`ScanJobStore` (Infrastructure/Sync) mantém jobs e logs em `ConcurrentDictionary`; `IScanJobStore` (Domain/Ports) define `Add/Get/GetLatest/AddLog`. O SQLite é inicializado por `DatabaseInitializer` com migrações versionadas (1–7 hoje). `SyncEndpoints` expõe status, SSE e cancelamento. `SyncView.vue` consome SSE e mostra o job atual.

O frontend é Vue 3 + Vite + Pinia + Vue Router + Tailwind com tema light/dark persistido por `GET/PUT /api/theme`. O build canônico é `scripts/build.sh` (npm ci + build, wwwroot, dotnet, docker).

## Decisões

### 1. Persistência em SQLite via Dapper (não EF Core)
Reusar o padrão existente dos repositórios (Dapper + `SqliteConnection`). Migração v8 no `DatabaseInitializer` cria `ScanJob` e `ScanJobLog`. Nenhuma dependência nova.

### 2. Segregar interfaces: `IScanJobRepository` + `IScanJobLogStore`
Substituir `IScanJobStore` por duas interfaces no Domain/Ports:
- `IScanJobRepository` — CRUD assíncrono de jobs (`AddAsync`, `GetAsync`, `GetLatestAsync`, `GetHistoryAsync`, `UpdateAsync`, `ClearJobsAsync`)
- `IScanJobLogStore` — logs assíncronos (`AddLogAsync`, `GetLogsAsync`, `ClearLogsAsync`, `CleanupOldLogsAsync`)

Ambas full-async (`Task`). `ScanJob` (Domain) ganha `CreatedAt`.

### 3. `SqliteScanJobRepository` + `SqliteScanJobLogStore` em `Infrastructure/Persistence/Repositories/`
Implementações Dapper separadas, uma por responsabilidade. `SqliteScanJobLogStore` implementa paginação, filtros (jobId, from, to, level, text) e ordenação. Sem cache em memória para SSE — o SSE consulta o banco diretamente (simples, evita race).

### 4. `LogCleanupBackgroundService` diário
`BackgroundService` usando `PeriodicTimer` + `CancellationToken` + `ILogger<T>`. Retenção via `LOG_RETENTION_DAYS` (default 30). Executa no startup e a cada 24h. Remove `ScanJobLog` por `timestamp` e `ScanJob` órfãos (sem logs). Idempotente.

### 5. Endpoints REST
- `GET /api/sync/history?page=1&pageSize=100`
- `GET /api/sync/logs?jobId&from&to&level&text&page=1&pageSize=100`
- `POST /api/sync/logs/clear` (apaga apenas logs)
- `POST /api/sync/clear` (apaga jobs + logs — admin)
- `POST /api/sync/logs/cleanup?retentionDays=30` (executa limpeza manual)

Validação de parâmetros com `Results.BadRequest`.

### 6. UI de logs
Nova view `LogsView.vue` em `/logs` + store `logs.js` + `LogFilterBar.vue` + `LogGroupedView.vue` (único componente, prop `groupBy: 'date' | 'status' | 'level'`). Agrupamento client-side, filtro live debounce 200ms, limite 500 linhas recentes. `data-testid` em grupos, linhas, botões. Rota `/logs` no menu. Tema via `useThemeStore` existente.

## Riscos

- `ScanJobLog` pode crescer: mitigado por retenção 30 dias + limpeza manual.
- SQLite concorrência: usar `PRAGMA busy_timeout` existente; conexão por operação.
- Migração v8 não idempotente: usar `CREATE TABLE IF NOT EXISTS`.

## Estrutura de arquivos

**Backend**
- `src/PlexAutoplay.Domain/Ports/IScanJobRepository.cs` — nova interface
- `src/PlexAutoplay.Domain/Ports/IScanJobLogStore.cs` — nova interface
- `src/PlexAutoplay.Domain/Ports/IScanJobStore.cs` — remover (substituído)
- `src/PlexAutoplay.Domain/Ports/ScanJob.cs` — adicionar `CreatedAt`
- `src/PlexAutoplay.Infrastructure/Persistence/DatabaseInitializer.cs` — migração v8
- `src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteScanJobRepository.cs` — nova
- `src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteScanJobLogStore.cs` — nova
- `src/PlexAutoplay.Infrastructure/Sync/LogCleanupBackgroundService.cs` — nova
- `src/PlexAutoplay.Web/Endpoints/SyncEndpoints.cs` — novos endpoints
- `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs` — registros

**Frontend**
- `vue/plex-frontend/src/views/Logs/LogsView.vue`
- `vue/plex-frontend/src/components/Logs/LogFilterBar.vue`
- `vue/plex-frontend/src/components/Logs/LogGroupedView.vue`
- `vue/plex-frontend/src/stores/logs.js`
- `vue/plex-frontend/src/router/index.js` — rota `/logs`
- `vue/plex-frontend/src/components/Sidebar/SidebarArea.vue` — item "Logs"

**Testes**
- `tests/e2e/tests/logs.spec.js`
- `tests/unit/SqliteScanJobRepositoryTests.cs`
- `tests/unit/SqliteScanJobLogStoreTests.cs`

## Schema

```sql
CREATE TABLE IF NOT EXISTS ScanJob (
  JobId TEXT PRIMARY KEY,
  Status TEXT NOT NULL,
  Processed INTEGER NOT NULL DEFAULT 0,
  Skipped INTEGER NOT NULL DEFAULT 0,
  Errors INTEGER NOT NULL DEFAULT 0,
  ErrorMessage TEXT,
  CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
  UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS ScanJobLog (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  JobId TEXT NOT NULL,
  Timestamp TEXT NOT NULL DEFAULT (datetime('now')),
  Level TEXT NOT NULL COLLATE NOCASE,
  Message TEXT NOT NULL COLLATE NOCASE,
  Suggestion TEXT,
  FOREIGN KEY (JobId) REFERENCES ScanJob(JobId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_ScanJobLog_Timestamp ON ScanJobLog(Timestamp);
CREATE INDEX IF NOT EXISTS IX_ScanJobLog_JobId ON ScanJobLog(JobId);
CREATE INDEX IF NOT EXISTS IX_ScanJobLog_Level ON ScanJobLog(Level);
```