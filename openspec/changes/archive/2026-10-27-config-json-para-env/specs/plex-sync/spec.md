# Spec Delta

## MODIFIED Requirements

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
