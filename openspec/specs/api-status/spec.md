# api-status Specification

## Purpose
Define o comportamento do endpoint de saúde do serviço `GET /api/status`, que expõe informações do serviço, o último job de sincronização e métricas de processo para o dashboard e clientes de monitoramento.

## Requirements

### Requirement: O endpoint de status retorna HTTP 200 com dados do serviço
O sistema SHALL retornar HTTP 200 em `GET /api/status` com o nome e a versão do serviço.

#### Scenario: Resposta de status bem-sucedida
- **WHEN** o cliente faz `GET /api/status`
- **THEN** o sistema retorna HTTP 200 com `service` igual a "plex-autoplay" e `version` preenchido

### Requirement: O endpoint expõe o último job de sincronização
O sistema SHALL incluir, na resposta de `GET /api/status`, os dados do último job de sincronização executado ou null quando nenhum job foi executado.

#### Scenario: Último job disponível
- **WHEN** existe pelo menos um job de sincronização registrado
- **THEN** a resposta inclui `lastJob` com `job_id`, `status`, `processed`, `skipped`, `errors`, `error` e `updated_at`

#### Scenario: Nenhum job executado ainda
- **WHEN** nenhum job de sincronização foi registrado
- **THEN** a resposta inclui `lastJob` igual a null

### Requirement: O endpoint expõe métricas de processo
O sistema SHALL incluir na resposta de `GET /api/status` métricas do processo: tempo total de CPU, memória privada, working set e tempo de atividade.

#### Scenario: Métricas de processo presentes
- **WHEN** o cliente faz `GET /api/status`
- **THEN** a resposta inclui `process` com `cpu_total_ms`, `memory_private_bytes`, `memory_working_set_bytes` e `uptime_ms` maiores ou iguais a zero

### Requirement: O endpoint nunca retorna 500 por falha interna
O sistema SHALL tratar exceções inesperadas ao montar a resposta de `GET /api/status` e nunca retornar HTTP 500 ao cliente.

#### Scenario: Falha ao ler métricas de processo
- **WHEN** a leitura de métricas de processo lança uma exceção
- **THEN** o sistema retorna HTTP 200 com os campos que puder preencher, sem expor 500 ao cliente
