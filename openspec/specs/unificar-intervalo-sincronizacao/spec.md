# Spec Delta — unificar-intervalo-sincronizacao

## MODIFIED Requirements

### Requirement: Processo único de sincronização em duas etapas

O sistema SHALL oferecer um processo único de sincronização que executa, em
ordem, etapa de catálogo (busca bibliotecas no Plex) e etapa de marcadores
(busca marcadores nos diretórios de mídia). O processo SHALL ser reutilizado
tanto pelo agendamento periódico quanto pelo comando de sincronização forçada.
O ciclo SHALL ser governado por um único intervalo `sync_interval_minutes`.

#### Scenario: Ciclo periódico

- **WHEN** o intervalo `sync_interval_minutes` é alcançado
- **THEN** o processo executa etapa 1 (catálogo) e, em seguida, etapa 2 (marcadores)
- **AND** o próximo ciclo aguarda `sync_interval_minutes` novamente

#### Scenario: Sincronização forçada

- **WHEN** o botão **Iniciar Escaneamento** em `/sync` é acionado
- **THEN** `POST /api/sync/run` cria um job e executa o mesmo processo completo (etapa 1 + etapa 2)
- **AND** retorna `{ job_id }` para acompanhamento em `GET /api/sync/status/{jobId}` e `GET /api/sync/log/stream/{jobId}`

#### Scenario: Progresso por etapa

- **WHEN** o job está em execução
- **THEN** os logs/status informam claramente etapa corrente (catálogo ou marcadores) e as contagens cumulativas

#### Scenario: Intervalo único

- **WHEN** o operador abre a tela de Ajustes
- **THEN** vê apenas o campo **Intervalo de Sincronização (minutos)**
- **AND** não existe campo separado para o intervalo de catálogo
