# Tasks — logs-processamento

## Phase 1 — Requisitos e artefatos

- [x] EGM A/S/C — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [x] `openspec validate logs-processamento --strict` passa.
- [x] Commit somente dos artefatos da change.
- [x] Worktree `feature/spec-logs-processamento` criada e sessão movida.

## Phase 2 — Backend persistência (Domain + Repositories)

- [ ] Adicionar `CreatedAt` em `ScanJob` (Domain/Ports).
- [ ] Criar `IScanJobRepository` (Domain/Ports): `AddAsync`, `GetAsync`, `GetLatestAsync`, `GetHistoryAsync`, `UpdateAsync`, `ClearJobsAsync`.
- [ ] Criar `IScanJobLogStore` (Domain/Ports): `AddLogAsync`, `GetLogsAsync` (com `LogQueryParams`), `ClearLogsAsync`, `CleanupOldLogsAsync`.
- [ ] Remover `IScanJobStore` antiga.
- [ ] Migração v8 em `DatabaseInitializer`: tabelas `ScanJob` e `ScanJobLog` + índices + `COLLATE NOCASE`.
- [ ] `SqliteScanJobRepository` (Infrastructure/Persistence/Repositories) com Dapper.
- [ ] `SqliteScanJobLogStore` (Infrastructure/Persistence/Repositories) com Dapper — paginação, filtros, ordenação.
- [ ] Registrar ambos em `ServiceCollectionExtensions` (singleton).

## Phase 3 — Backend limpeza e endpoints

- [ ] `LogCleanupBackgroundService` (PeriodicTimer + CancellationToken + ILogger, retenção `LOG_RETENTION_DAYS` default 30, execução startup + cada 24h, remove logs antigos + jobs órfãos).
- [ ] Registrar background service.
- [ ] `GET /api/sync/history?page=1&pageSize=100` → usa `IScanJobRepository.GetHistoryAsync`.
- [ ] `GET /api/sync/logs?jobId&from&to&level&text&page=1&pageSize=100` → usa `IScanJobLogStore.GetLogsAsync(LogQueryParams)`.
- [ ] `POST /api/sync/logs/clear` → `IScanJobLogStore.ClearLogsAsync`.
- [ ] `POST /api/sync/clear` → `IScanJobRepository.ClearJobsAsync` + `IScanJobLogStore.ClearLogsAsync`.
- [ ] `POST /api/sync/logs/cleanup?retentionDays=30` → `IScanJobLogStore.CleanupOldLogsAsync`.

## Phase 4 — Frontend tela de logs

- [ ] Store `logs.js` (fetch, debounce 200ms, filtros reativos, agrupamento computado).
- [ ] `LogsView.vue` em `/logs` — layout com header, filtros, view agrupada.
- [ ] `LogFilterBar.vue` — busca live, select level, date range picker, select groupBy.
- [ ] `LogGroupedView.vue` — prop `groupBy`, renderiza grupos com `data-testid="log-group-{key}"`, linhas com `data-testid="log-entry"`.
- [ ] Rota `/logs` em `router/index.js` (meta title: "Logs").
- [ ] Item "Logs" no `SidebarArea.vue` (icon "terminal" ou "list").

## Phase 5 — Testes e build

- [ ] Testes unitários `SqliteScanJobRepositoryTests.cs` (CRUD, history paginado).
- [ ] Testes unitários `SqliteScanJobLogStoreTests.cs` (filtros, paginação, clear, cleanup).
- [ ] Testes unitários `LogCleanupBackgroundServiceTests.cs` (tempo mockado, idempotência).
- [ ] E2E `tests/e2e/tests/logs.spec.js` (Playwright):
  - Navega `/logs`, verifica tema light/dark toggle.
  - Verifica menu lateral abre/fecha, item "Logs" ativo.
  - Testa filtro texto live (debounce).
  - Testa agrupamento por data/status/tipo.
  - Testa botão "Limpar Logs" + confirmação.
  - Testa paginação history/logs.
- [ ] `./scripts/build.sh` passa.
- [ ] `openspec validate logs-processamento --strict` passa.

## Phase 6 — QA

- [ ] QA independente contra spec; relatório `qa-report-1.md`.
- [ ] Playwright acessa `http://localhost:<port>/logs`; valida tema e menus.
- [ ] PASS ou loop de correção (máx. 5 iterações).