# Spec Delta — config-env

## REMOVED Requirements

### Requirement: Configuração exclusivamente variáveis de ambiente

## ADDED Requirements

### Requirement: Configuração operacional persistida no banco

O sistema SHALL obter as configurações operacionais (`PLEX_URL`,
`PLEX_TOKEN`, `MEDIA_ROOT`, `SYNC_INTERVAL_MINUTES`,
`SYNC_CATALOG_INTERVAL_MINUTES`) da tabela `AppSetting` do SQLite, e SHALL
usar variáveis de ambiente apenas como valor inicial de seed ou para
configuração de bootstrap imutável.

#### Scenario: Valor lido do banco

- **WHEN** existe `PLEX_URL` na tabela `AppSetting`
- **THEN** as chamadas ao Plex usam esse valor
- **AND** nenhuma leitura de `.env` é feita para esse fim

#### Scenario: Primeiro start com banco vazio

- **WHEN** a tabela `AppSetting` está vazia e a variável de ambiente
  correspondente está definida
- **THEN** o sistema grava o valor da variável no banco como seed
- **AND** passa a usar o valor persistido nas leituras seguintes

#### Scenario: Seed não ressuscita valor removido

- **WHEN** o banco já foi semeado e o operador remove/alterar uma chave
- **THEN** o próximo start não recria nem sobrescreve a chave com o valor do
  ambiente

#### Scenario: Alteração pela interface sem reinício

- **WHEN** o operador altera uma configuração em `/settings`
- **THEN** a próxima leitura da configuração usa o novo valor
- **AND** não é necessário reiniciar o container

#### Scenario: Configuração de bootstrap permanece no ambiente

- **WHEN** o sistema inicia
- **THEN** `JWT_SECRET`, portas HTTP/HTTPS/WebSocket, certificado,
  `ASPNETCORE_ENVIRONMENT` e `SQLITE_CONNECTION_STRING` continuam sendo lidos
  do ambiente
- **AND** não são expostos nem editáveis por `/api/settings`

#### Scenario: Sem variável de ambiente e sem registro

- **WHEN** uma configuração operacional não existe no banco nem no ambiente
- **THEN** o sistema usa o valor padrão documentado
- **AND** não falha ao iniciar
