# Spec Delta

## MODIFIED Requirements

### Requirement: Manual marker creation and editing
O sistema SHALL prover UI e API para usuários criar, atualizar e remover marcadores de intro e créditos por episódio, incluindo um endpoint de atualização (PUT) que altera tempos de um marcador existente sem recriá-lo.

#### Scenario: User creates a manual marker
- **WHEN** a user submits `{ rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 }` via the API
- **THEN** the system stores the marker in SQLite (with origin `manual`) and writes it to `custom.json`

#### Scenario: Usuário cria um marcador manual
- **WHEN** um usuário envia { rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 } via a API
- **THEN** o sistema armazena o marcador em SQLite (com origem `manual`) e escreve em custom.json

#### Scenario: User updates an existing marker
- **WHEN** a user sends an update request with new `start_ms`/`end_ms` for an existing marker identified by rating key and kind
- **THEN** the system updates the marker in SQLite via its update operation and rewrites `custom.json` with the new values, without deleting and recreating the marker

#### Scenario: User updates a non-existent marker
- **WHEN** an update request targets a marker that does not exist for that rating key and kind
- **THEN** the API responds with a "not found" status and nothing is written to SQLite or `custom.json`

#### Scenario: User removes a marker
- **WHEN** a user deletes a marker by rating key and kind
- **THEN** the system removes it from SQLite and rewrites `custom.json` without that entry

#### Scenario: Usuário remove um marcador
- **WHEN** um usuário remove um marcador por rating key e kind
- **THEN** o sistema remove do SQLite e reescreve custom.json sem aquela entrada

#### Scenario: Usuário atualiza um marcador existente
- **WHEN** um usuário envia uma requisição de atualização para um marcador existente com novos start_ms e end_ms
- **THEN** o sistema atualiza o marcador em SQLite (reusando a operação de atualização) e reescreve custom.json com os novos valores, sem apagar e recriar o marcador

#### Scenario: Edição inline por episódio
- **WHEN** o usuário abre a página de edição de uma série
- **THEN** o sistema apresenta os episódios da série agrupados por temporada, cada episódio com seus marcadores de intro e créditos, e permite editar os tempos inline com salvamento otimista (a UI não bloqueia durante o salvamento e reflete o estado salvo após a confirmação).

## ADDED Requirements

### Requirement: Registrar origem do marcador

O sistema SHALL registrar a origem de cada marcador (`manual` criado via UI/API, `detected` criado pelo scanner), persistir essa origem no SQLite, expor origem nas respostas da API e aplicar migração aditiva (coluna com default `manual`) em bancos de dados existentes.

#### Scenario: Marcador detectado pelo scanner

- **WHEN** o scanner detecta e grava um marcador
- **THEN** o marcador é persistido com origem `detected` em SQLite e custom.json continua em formato PlexAutoSkip inalterado.

#### Scenario: Marcador manual via UI ou API

- **WHEN** o usuário cria um marcador na UI ou via API
- **THEN** o marcador é persistido com origem `manual`, editável pelo endpoint de atualização.

#### Scenario: Banco de dados preexistente

- **WHEN** a aplicação inicia com um banco SQLite criado antes da coluna origem existir
- **THEN** a migração adiciona a coluna origem de forma aditiva marcando marcadores preexistentes como `manual`, sem perder dados.

#### Scenario: Listagem expõe a origem

- **WHEN** a API retorna marcadores armazenados
- **THEN** cada marker inclui o campo de origem para que a UI possa rotular marcadores manuais e detectados.
