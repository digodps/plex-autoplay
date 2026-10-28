# plex-sync Specification

## Purpose
Defines the behavior for a periodic BackgroundService that scans Plex series libraries, detects missing markers, and persists them to custom.json and SQLite.

## Requirements

### Requirement: Periodic synchronization
O sistema SHALL executar um serviço de segundo plano (BackgroundService) que varre todas as bibliotecas de séries configuradas em um intervalo configurável.

#### Scenario: Default sync interval
- **WHEN** o serviço inicia sem intervalo customizado configurado via variável de ambiente `SYNC_INTERVAL_MINUTES`
- **THEN** a primeira varredura começa 5 minutos após a inicialização

#### Scenario: Configurable sync interval
- **WHEN** o usuário define o intervalo de sincronização para 30 minutos via variável de ambiente `SYNC_INTERVAL_MINUTES`
- **THEN** as varreduras subsequentes ocorrem a cada 30 minutos

#### Scenario: Sync interval not read from JSON file
- **WHEN** o arquivo `app-config.json` contém um valor de `syncIntervalMinutes` diferente da variável de ambiente `SYNC_INTERVAL_MINUTES`
- **THEN** o sistema ignora o arquivo JSON e usa apenas o valor da variável de ambiente `SYNC_INTERVAL_MINUTES`

#### Scenario: Sync interval persisted
- **WHEN** o intervalo de sincronização é alterado através da variável de ambiente `SYNC_INTERVAL_MINUTES`
- **THEN** o novo valor é usado imediatamente na próxima varredura; não é gravado em `app-config.json` (esse arquivo não é mais lido para configuração)

### Requirement: Scan only libraries without markers
The system SHALL skip episodes that already have markers stored in custom.json or SQLite.

#### Scenario: Episode already has markers
- **WHEN** an episode's rating key exists in custom.json with intro and credits markers
- **THEN** the system skips ffprobe analysis for that episode

#### Scenario: Episode missing markers
- **WHEN** an episode has no markers in custom.json
- **THEN** the system runs ffprobe silence detection and stores any detected markers

### Requirement: Scan progress reporting
The system SHALL expose scan status and progress through an API endpoint and SSE stream.

#### Scenario: Scan status query
- **WHEN** a user requests `GET /api/sync/status`
- **THEN** the system returns the current scan state (queued, running, completed, failed) and processed/skipped/error counts

#### Scenario: SSE progress stream
- **WHEN** a user connects to `GET /api/sync/log/stream`
- **THEN** the system sends server-sent events with scan progress updates

### Requirement: Scan cancellation
The system SHALL allow a running scan to be cancelled.

#### Scenario: Cancel running scan
- **WHEN** a user calls `POST /api/sync/cancel` during a running scan
- **THEN** the system interrupts the scan worker and updates status to cancelled
