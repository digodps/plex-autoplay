# Spec Delta — synchronization

## ADDED Requirements

### Requirement: Processo único de sincronização em duas etapas

O sistema SHALL oferecer um processo único de sincronização que executa, em
ordem, a etapa de catálogo e a etapa de marcadores. O processo SHALL ser
reutilizado tanto pelo agendamento periódico quanto pelo comando de
sincronização forçada do botão **Sincronizar**.

#### Scenario: Ciclo periódico

- **WHEN** o intervalo `sync_interval_minutes` é alcançado
- **THEN** o processo executa etapa 1 (catálogo) e, em seguida, etapa 2
  (marcadores)
- **AND** o próximo ciclo aguarda `sync_interval_minutes` novamente

#### Scenario: Sincronização forçada

- **WHEN** o botão **Iniciar Escaneamento** em `/sync` é acionado
- **THEN** `POST /api/sync/run` cria um job e executa o mesmo processo completo
  (etapa 1 + etapa 2)
- **AND** retorna `{ job_id }` para acompanhamento em `GET /api/sync/status/{jobId}`
  e `GET /api/sync/log/stream/{jobId}`

#### Scenario: Progresso por etapa

- **WHEN** o job está em execução
- **THEN** os logs/status informam claramente a etapa corrente (catálogo ou
  marcadores) e as contagens cumulativas de processados/pulados/erros
- **AND** o status final é `completed`, `failed` ou `cancelled`

#### Scenario: Falha na etapa 1 não executa etapa 2

- **WHEN** a etapa 1 falha ao consultar o Plex
- **THEN** o job termina como `failed` com a mensagem de erro
- **AND** a etapa 2 não inicia

#### Scenario: Cancelamento

- **WHEN** `POST /api/sync/cancel/{jobId}` é chamado durante o processo
- **THEN** as etapas em andamento são interrompidas no próximo ponto seguro
- **AND** o status do job passa para `cancelled`

#### Scenario: Disponibilidade de um único intervalo

- **WHEN** o operador abre `/settings`
- **THEN** vê apenas o campo **Intervalo de Sincronização (minutos)**
- **AND** o mesmo intervalo governa as duas etapas do ciclo