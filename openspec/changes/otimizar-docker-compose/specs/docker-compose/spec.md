# docker-compose Specification (Delta)

## ADDED Requirements
### Requirement: Apenas variáveis utilizadas
O compose e o arquivo `.env` **SHALL** conter exclusivamente as variáveis de ambiente necessárias e consumidas pela aplicação ou pelos serviços Docker.

#### Scenario: Validação de variáveis órfãs
**Given** O arquivo `.env` e o `docker-compose.yml` do projeto.
**When** Um operador inspeciona as variáveis declaradas.
**Then** Não existem variáveis que não sejam lidas pela aplicação nem interpoladas pelo Docker Compose.

## MODIFIED Requirements
### Requirement: Validação de start simplificada
O script `scripts/start.sh` **SHALL** validar apenas as variáveis obrigatórias para o funcionamento inicial da aplicação (`PLEX_URL`, `PLEX_TOKEN`, `MEDIA_ROOTS`, `JWT_SECRET`, `ADMIN_PASSWORD`).

#### Scenario: Validação simplificada de start
**Given** O script `scripts/start.sh` é executado com um `.env` completo.
**When** O script valida o ambiente antes de iniciar.
**Then** Apenas as variáveis obrigatórias são validadas; variáveis opcionais ou legadas não causam falha de inicialização.
