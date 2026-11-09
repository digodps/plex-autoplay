# Spec Delta — plex-sync

## ADDED Requirements

### Requirement: Sincronização unificada com catálogo precedendo marcadores

O sistema SHALL executar uma única rotina de sincronização que, por ciclo,
executa **duas etapas em ordem**: (1) sincronização do catálogo de séries
(bibliotecas e séries do Plex para o banco), e em seguida (2) varredura de
marcadores dos episódios. Nenhum cycle de sincronização SHALL rodar marcadores
antes que o catálogo esteja atualizado para aquele ciclo.

#### Scenario: Ordem das etapas no cycle

- **WHEN** um ciclo de sincronização começa
- **THEN** a etapa 1 (catálogo) executa antes da etapa 2 (marcadores)
- **AND** o endpoint usado pelo botão **Sincronizar** roda ambas as etapas
- **AND** logs e contagens refletem as duas etapas, com totais cumulativos
  visíveis em status SSE

#### Scenario: Forçar sincronização completa

- **WHEN** o botão **Iniciar Escaneamento** é acionado
- **THEN** o sistema executa etapa 1 (catálogo) e etapa 2 (marcadores) em ordem
- **AND** retorna um `job_id` cujo status eventualmente passa para `completed`

#### Scenario: Falha na etapa 1 impede etapa 2

- **WHEN** a etapa 1 falha ao consultar o Plex
- **THEN** o job termina como `failed` com a mensagem de erro
- **AND** a etapa 2 não inicia

#### Scenario: Cancelamento entre etapas

- **WHEN** `POST /api/sync/cancel/{jobId}` é chamado durante o processo
- **THEN** as etapas em andamento são interrompidas no próximo ponto seguro
- **AND** o status do job passa para `cancelled`