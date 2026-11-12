# logs-processamento Specification

## Purpose
Define o comportamento de persistência, limpeza e visualização dos logs de processamento (varredura/scan) do Plex Autoplay, garantindo que o histórico sobreviva a reinícios, que registros antigos sejam removidos automaticamente e que o operador consiga consultar, filtrar e agrupar os logs pela interface.

## Requirements

### Requirement: Persistir jobs de varredura no SQLite

O sistema SHALL persistir os jobs de varredura no SQLite, substituindo o armazenamento exclusivamente em memória.

#### Scenario: Job persistido ao iniciar
- **WHEN** um job de varredura é iniciado
- **THEN** o sistema grava um registro em `ScanJob` com `job_id`, `status`, `created_at` (ISO8601 UTC), `updated_at` (ISO8601 UTC)

#### Scenario: Status persistido ao concluir
- **WHEN** o job muda para `completed`, `failed` ou `cancelled`
- **THEN** o sistema atualiza o `status`, `processed`, `skipped`, `errors`, `error_message` e `updated_at` no registro `ScanJob`

#### Scenario: Histórico sobrevive a reinício
- **WHEN** o container reinicia
- **THEN** o histórico de jobs permanece consultável no SQLite

### Requirement: Persistir linhas de log no SQLite

O sistema SHALL persistir cada linha de log emitida durante a execução dos jobs.

#### Scenario: Log persistido durante execução
- **WHEN** o backend emite uma linha de log via `AddLogAsync`
- **THEN** a linha é gravada em `ScanJobLog` com `job_id`, `timestamp` (ISO8601 UTC), `level`, `message` (COLLATE NOCASE), `suggestion` (nullable)

### Requirement: Listar histórico de jobs com paginação

O sistema SHALL expor endpoint de leitura que retorna o histórico de jobs paginado.

#### Scenario: Histórico paginado
- **WHEN** o cliente chama `GET /api/sync/history?page=1&pageSize=100`
- **THEN** o sistema retorna jobs ordenados por `updated_at` descendente, com paginação

### Requirement: Listar logs com filtros e paginação

O sistema SHALL expor endpoint de leitura que retorna as linhas de log com filtros e paginação.

#### Scenario: Filtro por job
- **WHEN** o cliente chama `GET /api/sync/logs?jobId=...&page=1&pageSize=100`
- **THEN** o sistema retorna apenas as linhas daquele job, paginado

#### Scenario: Filtro por período
- **WHEN** o cliente chama `GET /api/sync/logs?from=2026-11-01T00:00:00Z&to=2026-11-02T00:00:00Z&page=1&pageSize=100`
- **THEN** o sistema retorna apenas linhas cujo `timestamp` está no intervalo (UTC)

#### Scenario: Filtro por nível
- **WHEN** o cliente chama `GET /api/sync/logs?level=error&page=1&pageSize=100`
- **THEN** o sistema retorna apenas linhas com `level` igual ao informado

#### Scenario: Filtro por texto (case-insensitive)
- **WHEN** o cliente chama `GET /api/sync/logs?text=falha&page=1&pageSize=100`
- **THEN** o sistema retorna linhas cuja `message` contém o texto (SQLite `COLLATE NOCASE`)

#### Scenario: Filtros combinados
- **WHEN** o cliente combina múltiplos filtros
- **THEN** o sistema aplica interseção (AND) de todos os filtros

### Requirement: Limpar logs do banco

O sistema SHALL permitir limpeza manual das linhas de log persistidas, preservando os metadados dos jobs.

#### Scenario: Limpar apenas logs
- **WHEN** o usuário aciona "Limpar Logs" na UI
- **THEN** o cliente chama `POST /api/sync/logs/clear`
- **THEN** o sistema remove todas as linhas de `ScanJobLog` e retorna a contagem removida

#### Scenario: Limpeza com confirmação
- **WHEN** o usuário clica em "Limpar Logs"
- **THEN** a UI exibe confirmação antes de enviar a requisição

### Requirement: Limpar jobs e logs (reset total)

O sistema SHALL permitir limpeza total (jobs + logs) via endpoint separado.

#### Scenario: Reset total
- **WHEN** o usuário aciona "Resetar Histórico" (ação administrativa)
- **THEN** o cliente chama `POST /api/sync/clear`
- **THEN** o sistema remove todas as linhas de `ScanJobLog` e `ScanJob` e retorna contagens

### Requirement: Limpeza automática diária

O sistema SHALL agendar limpeza automática de logs com retenção padrão de 30 dias, executando uma vez por dia.

#### Scenario: Remoção de logs antigos
- **WHEN** o serviço `LogCleanupBackgroundService` executa
- **THEN** remove linhas de `ScanJobLog` com `timestamp` anterior a N dias (padrão 30) e jobs `ScanJob` sem logs remanescentes

#### Scenario: Limpeza diária
- **WHEN** o serviço inicia
- **THEN** executa a limpeza e depois agenda novas execuções a cada 24 horas

#### Scenario: Retenção configurável
- **WHEN** a variável de ambiente `LOG_RETENTION_DAYS` está definida
- **THEN** o sistema usa esse valor como retenção; caso contrário, 30 dias

### Requirement: Tela de logs com agrupamento e filtro live

A UI SHALL exibir uma tela de logs com todos os registros, agrupamento por data/status/tipo e filtro de texto em tempo real.

#### Scenario: Exibir todos os logs
- **WHEN** o usuário acessa `/logs`
- **THEN** a tela carrega logs de `GET /api/sync/logs` e exibe em lista agrupada (limite 500 linhas mais recentes)

#### Scenario: Agrupar por data
- **WHEN** o usuário seleciona "Agrupar por data"
- **THEN** os logs são agrupados por dia (ex.: "Hoje", "Ontem", "2026-11-01")

#### Scenario: Agrupar por status
- **WHEN** o usuário seleciona "Agrupar por status"
- **THEN** os logs são agrupados pelo status do job (`completed`, `failed`, `cancelled`, `running`, `queued`)

#### Scenario: Agrupar por tipo
- **WHEN** o usuário seleciona "Agrupar por tipo"
- **THEN** os logs são agrupados pelo nível (`info`, `success`, `warning`, `error`, `skip`)

#### Scenario: Filtro de texto live
- **WHEN** o usuário digita no campo de filtro
- **THEN** a lista é filtrada imediatamente (debounce 200 ms) sem recarregar a página

#### Scenario: Filtro combinado
- **WHEN** o usuário aplica texto + status + data
- **THEN** a lista mostra apenas registros que atendem todos os filtros

#### Scenario: Tema aplicado
- **WHEN** o usuário alterna o tema light/dark
- **THEN** a tela de logs respeita o tema persistido (`GET/PUT /api/theme`) sem recarregar

#### Scenario: Menu funcional
- **WHEN** o usuário abre/fecha o menu ou navega para "Logs"
- **THEN** o menu funciona, o item "Logs" fica com estado ativo e a navegação fecha em mobile
