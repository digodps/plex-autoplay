# Spec Delta

## MODIFIED Requirements

### Requirement: Discover Plex libraries
O sistema SHALL consultar a API do Plex Media Server para listar todas as seções de biblioteca disponíveis, filtrar as do tipo "show" e persistir cada biblioteca de séries no banco local com rating key, título, tipo e agente.

#### Scenario: Successful library discovery
- **WHEN** o sistema chama GET /library/sections com um token válido do Plex
- **THEN** o sistema retorna uma lista de bibliotecas com key, title, type e agent

#### Scenario: Plex server unreachable
- **WHEN** o servidor Plex em PLEX_URL não está acessível
- **THEN** o sistema retorna um erro indicando que o servidor Plex está indisponível

#### Scenario: Series library persisted to local database
- **WHEN** o sistema descobre uma biblioteca do tipo "show" via API do Plex
- **THEN** o sistema insere ou atualiza o registro da biblioteca na tabela de bibliotecas do banco local com rating key, title, type e agent

#### Scenario: Series library included
- **WHEN** uma seção de biblioteca do Plex tem type="show"
- **THEN** o sistema a inclui na lista de bibliotecas de séries

#### Scenario: Movie library excluded
- **WHEN** uma seção de biblioteca do Plex tem type="movie"
- **THEN** o sistema não a inclui nas bibliotecas sincronizadas
