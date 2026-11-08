# Spec Delta — backend-architecture

## REMOVED Requirements

### Requirement: Configuration I/O outside Web

## ADDED Requirements

### Requirement: Configuration persistence outside Web

O sistema SHALL ler/gravar configurações operacionais através da porta
`IAppSettingsStore` (Application, implementação Infrastructure). Endpoints
Web SHALL delegar validação e persistência a um caso de uso, sem I/O direto
de arquivo ou SQL.

#### Scenario: Config change via port

- **WHEN** um endpoint persiste configurações
- **THEN** delega a `UpdateAppSettingsUseCase` e `IAppSettingsStore`
- **AND** não executa `System.IO.File.*` nem SQL na camada Web

#### Scenario: Web não implementa persistência

- **WHEN** o build roda o teste de arquitetura
- **THEN** nenhum tipo de Web implementa `IAppSettingsStore`
