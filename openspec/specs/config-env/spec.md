# config-env Specification

## Purpose

Define que toda configuração configurável do sistema deve ser obtida exclusivamente de variáveis de ambiente, removendo a dependência de arquivos JSON internos (`app-config.json`, `appsettings.json`) para valores configuráveis pelo operador.

## Requirements

### Requirement: Configuração exclusivamente via variáveis de ambiente
O sistema SHALL obter todos os valores configuráveis (intervalo de sincronização, nível de log, allowed hosts, porta HTTPS, porta WebSocket, caminho do custom.json, caminho do app-config, exportador de telemetria, endpoint OTLP, nome do serviço, etc.) unicamente de variáveis de ambiente, não lendo arquivos JSON internos para esses fins.

#### Scenario: Valor lido de variável de ambiente
- **WHEN** uma variável de ambiente relevante está definida
- **THEN** o sistema usa esse valor para configurar o comportamento correspondente

#### Scenario: Valor padrão quando variável não definida
- **WHEN** nenhuma variável de ambiente está definida para um item configurável
- **THEN** o sistema usa um valor padrão sensato (hardcoded no código) em vez de ler um arquivo JSON

#### Scenario: LOG_LEVEL lido da variável de ambiente
- **WHEN** a variável LOG_LEVEL está definida como Debug
- **THEN** o sistema emite logs em nível Debug

#### Scenario: LOG_LEVEL não definido
- **WHEN** a variável LOG_LEVEL não está definida
- **THEN** o sistema usa o nível Information como padrão

#### Scenario: ALLOWED_HOSTS lido da variável de ambiente
- **WHEN** a variável ALLOWED_HOSTS está definida
- **THEN** o sistema aceita requisições apenas para os hosts listados

#### Scenario: ALLOWED_HOSTS não definido
- **WHEN** a variável ALLOWED_HOSTS não está definida
- **THEN** o sistema aceita requisições de qualquer host (*)

#### Scenario: WS_PORT lido da variável de ambiente
- **WHEN** a variável WS_PORT está definida
- **THEN** o sistema expõe o WebSocket na porta configurada

#### Scenario: OTEL_EXPORTER e OTEL_ENDPOINT lidos da variável de ambiente
- **WHEN** a variável OTEL_EXPORTER está definida como otlp e OTEL_ENDPOINT aponta para um coletor
- **THEN** o sistema exporta telemetria via OTLP para aquele endpoint

#### Scenario: APP_CONFIG_PATH não é lido para configuração
- **WHEN** o arquivo app-config.json existe no volume persistente
- **THEN** o sistema ignora seu conteúdo e usa apenas variáveis de ambiente

### Requirement: Nenhuma leitura de app-config.json para configuração
O sistema SHALL não ler o arquivo app-config.json para obter valores de configuração.

#### Scenario: app-config.json ausente
- **WHEN** o arquivo app-config.json não existe no volume persistente
- **THEN** o sistema inicia normalmente usando variáveis de ambiente ou valores padrão

#### Scenario: app-config.json presente mas ignorado
- **WHEN** o arquivo app-config.json existe com valores diferentes das variáveis de ambiente
- **THEN** o sistema ignora o conteúdo do JSON e usa as variáveis de ambiente

### Requirement: LOG_LEVEL configurável via variável de ambiente
O sistema SHALL permitir configurar o nível de log através da variável de ambiente LOG_LEVEL.

#### Scenario: LOG_LEVEL definido
- **WHEN** a variável LOG_LEVEL está definida como Debug
- **THEN** o sistema emite logs em nível Debug

#### Scenario: LOG_LEVEL não definido
- **WHEN** a variável LOG_LEVEL não está definida
- **THEN** o sistema usa o nível Information como padrão

### Requirement: ALLOWED_HOSTS configurável via variável de ambiente
O sistema SHALL permitir configurar hosts permitidos através da variável de ambiente ALLOWED_HOSTS.

#### Scenario: ALLOWED_HOSTS definido
- **WHEN** a variável ALLOWED_HOSTS está definida
- **THEN** o sistema aceita requisições apenas para os hosts listados

#### Scenario: ALLOWED_HOSTS não definido
- **WHEN** a variável ALLOWED_HOSTS não está definida
- **THEN** o sistema aceita requisições de qualquer host (*)

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
