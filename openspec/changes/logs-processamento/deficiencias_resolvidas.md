# Deficiências Resolvidas — logs-processamento

## Revisão A — Arquitetura

| # | Achado | Veredito | Ação |
|---|--------|----------|------|
| A1 | Interface mistura sync/async | PARCIAL | Tornar `IScanJobStore` full-async (`Task` em todos métodos) |
| A2 | `Add`/`AddLog` síncronos no SQLite | PARCIAL | Com full-async, `Task AddAsync`/`Task AddLogAsync` com Dapper |
| A3 | `ClearLogsAsync` apaga jobs também | **REJEITADO** | Separar: `ClearLogsAsync` (apenas `ScanJobLog`) + `ClearJobsAsync` (jobs + logs órfãos). Endpoint `POST /api/sync/logs/clear` → apenas logs; `POST /api/sync/clear` → tudo. |
| A4 | Caminho `SqliteScanJobStore` divergente | **REJEITADO** | Padronizar: `Infrastructure/Persistence/Repositories/SqliteScanJobStore.cs` |
| A5 | Caminho testes divergente | PARCIAL | Usar convenção flat: `tests/unit/SqliteScanJobStoreTests.cs` |
| A6 | `GetLogsAsync` sem paginação | PARCIAL | Adicionar `page`/`pageSize` (default 100) |
| A7 | `ScanJob` sem `CreatedAt` | PARCIAL | Adicionar `CreatedAt` à classe no Domain |
| A8 | Virtualização sem dependência | **REJEITADO** | Remover menção a virtualização; renderização simples com limite de 500 linhas |
| A9 | Rollback migration v8 manual | PARCIAL | Documentar como procedimento operacional |
| A10 | FK não habilitada | ACEITO | `LogCleanupBackgroundService` limpa órfãos explicitamente |
| A11 | Integração camadas | ACEITO | — |
| A12 | DI e BackgroundService | ACEITO | — |
| A13 | Frontend integração | ACEITO | — |
| A14 | Spec coerente | ACEITO | — |

## Revisão S — SOLID

| # | Achado | Veredito | Ação |
|---|--------|----------|------|
| S1 | SRP - `ScanJobStore` gerencia dicionário + log | REJEITADO | Separar responsabilidade: `SqliteScanJobStore` persiste; logs via método dedicado; não muta entidade em memória |
| S2 | OCP - extensão requer alterar interface | REJEITADO | Segregar interface (ver S4) |
| S3 | LSP - substituição problemática | PARCIAL | Com interface segregada + full-async, `SqliteScanJobStore` substitui sem quebrar contratos |
| S4 | ISP - interface unifica job + log | **REJEITADO** | Separar: `IScanJobRepository` (CRUD job) + `IScanJobLogStore` (logs) |
| S5 | DIP - falha na extensão | PARCIAL | Com segregação, implementações injetáveis por abstração |

## Revisão C — Clean Code / Escopo / Testabilidade

| # | Achado | Veredito | Ação |
|---|--------|----------|------|
| C1 | Escopo YAGNI | ACEITO | — |
| C2 | `text` filter case-insensitive Unicode | PARCIAL | Especificar `COLLATE NOCASE` no SQLite |
| C3 | `Timestamp` sem formato | **REJEITADO** | `DEFAULT (datetime('now'))` + comment `ISO8601 UTC`; `ScanLogEntry.Timestamp` é `DateTime` (UTC) |
| C4 | Cache curto vago | PARCIAL | Definir: ler direto do DB no SSE (sem cache) ou `ConcurrentDictionary` + `PeriodicTimer` 5s refresh |
| C5 | `LogCleanupBackgroundService` sem CT/structured logging | PARCIAL | Usar `PeriodicTimer` + `CancellationToken` + `ILogger<T>` |
| C6 | 5 parâmetros opcionais em `GetLogsAsync` | **REJEITADO** | Criar `LogQueryParams` record |
| C7 | Agrupamento por status | ACEITO | — |
| C8 | `data-testid` para E2E | PARCIAL | Adicionar em `LogGroupedView`, `LogFilterBar`, botões |
| C9 | Phase 2 item 15 (tasks) | PARCIAL | Revisar conforme alterações acima |
| C10 | `ClearLogsAsync` retorna contagem | ACEITO | — |
| C11 | Rota `/logs` + menu + QA | ACEITO | — |
| C12 | Tema via `useThemeStore` | ACEITO | — |
| C13 | `LogStatsCards` não no spec | **REJEITADO** | Remover do design |
| C14 | `LogTable` + `LogGroupedView` duplicação | **REJEITADO** | Unificar em `LogGroupedView` com prop `groupBy` |
| C15 | BackgroundService padrão | ACEITO | — |
| C16 | History paginação completa | PARCIAL | Adicionar `page`/`pageSize` no `GET /api/sync/history` |
| C17 | SQLite concorrência | ACEITO | — |
| C18 | `Suggestion` nullable | ACEITO | — |
| C19 | `openspec validate --strict` | ACEITO | — |
| C20 | Filtro combinado | ACEITO | — |

## Correções Aplicadas nos Artefatos

1. **`IScanJobStore`** → segregado em `IScanJobRepository` + `IScanJobLogStore`, full-async
2. **`ScanJob`** (Domain) → adicionado `CreatedAt`
3. **`SqliteScanJobStore`** → caminho `Infrastructure/Persistence/Repositories/`
4. **`GetLogsAsync`** → recebe `LogQueryParams` record, retorna paginado
5. **`ClearLogsAsync`** → apenas logs; novo `ClearJobsAsync` para tudo
5. **Endpoints** → `POST /api/sync/logs/clear` (logs), `POST /api/sync/clear` (tudo)
6. **Migration v8** → `Timestamp DEFAULT (datetime('now'))` + `COLLATE NOCASE` em `Message`, `Level`
7. **`LogCleanupBackgroundService`** → `PeriodicTimer` + `CancellationToken` + `ILogger`
8. **Design Frontend** → remover `LogStatsCards`, `LogTable`; `LogGroupedView` único com `groupBy`; `data-testid`
9. **Tasks** → atualizados conforme acima
10. **Spec** → adicionar paginação em `history` e `logs`, `COLLATE NOCASE`, formato timestamp