# Spec Delta

## Purpose

Define o comportamento da telemetria de ponta a ponta que permite rastrear requisições desde o frontend Vue até o banco SQLite e serviços externos, com traces distribuídos, métricas e logs estruturados correlacionados.

## ADDED Requirements

### Requirement: Propagar correlation ID em requisições HTTP
O sistema SHALL gerar e propagar um correlation ID único em cada requisição entrante, incluindo-o nos cabeçalhos de resposta e nos logs estruturados.

#### Scenario: Correlation ID gerado automaticamente
- **WHEN** uma requisição chega sem cabeçalho de correlation ID
- **THEN** o sistema gera um ID único e o inclui no cabeçalho de resposta `X-Correlation-ID`

#### Scenario: Correlation ID recebido do cliente é preservado
- **WHEN** uma requisição chega com cabeçalho `X-Correlation-ID` válido
- **THEN** o sistema usa esse ID em todos os logs e traces dessa requisição

### Requirement: Exportar traces distribuídos para chamadas externas
O sistema SHALL exportar traces distribuídos que incluam chamadas ao Plex Media Server, permitindo visualizar o tempo gasto em cada span.

#### Scenario: Trace inclui chamada ao Plex
- **WHEN** a API consulta bibliotecas no Plex Media Server
- **THEN** o trace distribuído inclui um span para a chamada HTTP ao Plex com duração e status

#### Scenario: Trace inclui scan de marcadores via ffprobe
- **WHEN** o serviço de sincronização executa ffprobe para detectar marcadores
- **THEN** o trace distribuído inclui um span para a execução do ffprobe com duração e resultado

### Requirement: Expor métricas de tempo de resposta e taxa de erro
O sistema SHALL expor métricas de Prometheus com contadores de requisições, duração e códigos de status HTTP por endpoint.

#### Scenario: Métrica de duração de requisição HTTP
- **WHEN** uma requisição HTTP é processada
- **THEN** o sistema registra a duração em uma métrica de histograma agrupada por método, caminho e status

#### Scenario: Métrica de erro de chamada externa
- **WHEN** uma chamada ao Plex Media Server falha
- **THEN** o sistema incrementa um contador de erros agrupado por serviço externo e tipo de erro

### Requirement: Exibir dashboard de métricas na aplicação
O sistema SHALL fornecer uma página ou endpoint que exiba as principais métricas de observabilidade (tempo médio de resposta, taxa de erro, duração média de scans).

#### Scenario: Dashboard de métricas acessível
- **WHEN** o usuário acessa o endpoint de métricas
- **THEN** o sistema retorna dados de tempo médio de resposta, taxa de erro e contagem de scans realizados

### Requirement: Correlacionar logs estruturados por correlation ID
O sistema SHALL emitir logs em formato JSON contendo correlation ID, timestamp, nível, mensagem e contexto da requisição.

#### Scenario: Log inclui correlation ID
- **WHEN** o sistema processa uma requisição e emite um log
- **THEN** o log JSON inclui o campo `correlation_id` com o valor da requisição

#### Scenario: Logs de chamada externa incluem contexto
- **WHEN** o sistema chama o Plex Media Server e emite um log
- **THEN** o log JSON inclui o campo `external_service` com o valor `plex` e o campo `duration_ms`
