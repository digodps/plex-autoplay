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
