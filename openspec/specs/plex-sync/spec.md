# plex-sync Specification

## Purpose
Defines the behavior for a periodic BackgroundService that scans Plex series libraries, detects missing markers, and persists them to custom.json and SQLite.

## Requirements

### Requirement: Periodic synchronization
O sistema SHALL executar um serviço de segundo plano (BackgroundService) que varre todas as bibliotecas de séries configuradas em um intervalo configurável, executando detecção real de marcadores via ffprobe e persistindo os resultados em SQLite e custom.json.

#### Scenario: Default sync interval
- **WHEN** o serviço inicia sem intervalo customizado configurado via variável de ambiente SYNC_INTERVAL_MINUTES
- **THEN** a primeira varredura começa 5 minutos após a inicialização

#### Scenario: Configurable sync interval
- **WHEN** o usuário define o intervalo de sincronização para 30 minutos via variável de ambiente SYNC_INTERVAL_MINUTES
- **THEN** as varreduras subsequentes ocorrem a cada 30 minutos

#### Scenario: Sync interval not read from JSON file
- **WHEN** o arquivo app-config.json contém um valor de syncIntervalMinutes diferente da variável de ambiente SYNC_INTERVAL_MINUTES
- **THEN** o sistema ignora o arquivo JSON e usa apenas o valor da variável de ambiente SYNC_INTERVAL_MINUTES

#### Scenario: Sync interval persisted
- **WHEN** o intervalo de sincronização é alterado através da variável de ambiente SYNC_INTERVAL_MINUTES
- **THEN** o novo valor é usado imediatamente na próxima varredura; não é gravado em app-config.json

#### Scenario: Real scan executed during sync
- **WHEN** o serviço de sincronização executa uma varredura agendada
- **THEN** o sistema lista episódios de cada biblioteca de séries, executa ffprobe para detecção de marcadores e persiste os resultados em SQLite e custom.json

#### Scenario: Sync skips episodes that already have markers
- **WHEN** um episódio já possui marcadores de intro e créditos em custom.json ou SQLite
- **THEN** o sistema pula a detecção via ffprobe para aquele episódio

### Requirement: Scan only libraries without markers
O sistema SHALL pular episódios que já tenham marcadores armazenados em custom.json ou SQLite.

#### Scenario: Episode already has markers
- **WHEN** an episode's rating key exists in custom.json with intro and credits markers
- **THEN** the system skips ffprobe analysis for that episode

#### Scenario: Episode missing markers
- **WHEN** an episode has no markers in custom.json
- **THEN** the system runs ffprobe silence detection and stores any detected markers

### Requirement: Scan progress reporting
O sistema SHALL expor o status e progresso da varredura através de um endpoint de API e um stream SSE.

#### Scenario: Scan status query
- **WHEN** a usuário requisita GET /api/sync/status
- **THEN** o sistema retorna o estado atual da varredura (queued, running, completed, failed) e contagens de processados/pulados/erros

#### Scenario: SSE progress stream
- **WHEN** a usuário conecta a GET /api/sync/log/stream
- **THEN** o sistema envia eventos server-sent com atualizações de progresso da varredura

### Requirement: Scan cancellation
O sistema SHALL permitir cancelar uma varredura em execução.

#### Scenario: Cancel running scan
- **WHEN** a usuário chama POST /api/sync/cancel durante uma varredura em execução
- **THEN** o sistema interrompe o worker da varredura e atualiza o status para cancelled
