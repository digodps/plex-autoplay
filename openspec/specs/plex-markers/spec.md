# plex-markers Specification

## Purpose
Defines the behavior for detecting, storing, and retrieving intro and credits time markers for Plex series episodes, persisting them in PlexAutoSkip-compatible `custom.json` format.

## Requirements

### Requirement: Detect intro markers from audio silence
O sistema SHALL analisar um arquivo de mídia usando detecção de silêncio do ffprobe e identificar segmentos contíguos de silêncio no início do episódio como candidatos a marcadores de intro.

#### Scenario: Intro detected at episode start
- **WHEN** ffprobe finds silence from 0ms to approximately 90000ms in a series episode
- **THEN** the system creates a marker of kind `intro` with `start_ms=0` and `end_ms` equal to the silence end

#### Scenario: No silence at episode start
- **WHEN** ffprobe finds no significant silence in the first 120 seconds of audio
- **THEN** the system does not create an intro marker for that episode

#### Scenario: Intro detectado no início do episódio
- **WHEN** ffprobe encontra silêncio de 0ms a aproximadamente 90000ms em um episódio de série
- **THEN** o sistema cria um marcador do tipo intro com start_ms=0 e end_ms igual ao fim do silêncio

### Requirement: Detect credits markers from audio silence
O sistema SHALL analisar um arquivo de mídia usando detecção de silêncio do ffprobe e identificar segmentos contíguos de silêncio próximo do final do episódio como candidatos a marcadores de créditos.

#### Scenario: Credits detected at episode end
- **WHEN** ffprobe finds silence from approximately 85% of duration to the end of the episode
- **THEN** the system creates a marker of kind `credits` with `start_ms` at the silence start and `end_ms` at the episode duration

#### Scenario: Episode shorter than minimum credits threshold
- **WHEN** the episode duration is less than 300000ms
- **THEN** the system does not attempt credits detection for that episode

#### Scenario: Créditos detectados no final do episódio
- **WHEN** ffprobe encontra silêncio de aproximadamente 85% da duração até o final do episódio
- **THEN** o sistema cria um marcador do tipo credits com start_ms no início do silêncio e end_ms na duração do episódio

#### Scenario: Episódio menor que o limite mínimo de créditos
- **WHEN** a duração do episódio é menor que 300000ms
- **THEN** o sistema não tenta detecção de créditos para aquele episódio

### Requirement: Persist markers in custom.json format
O sistema SHALL escrever marcadores detectados ou criados manualmente em custom.json no formato PlexAutoSkip, keyed por rating key, e também persistir em SQLite, sem duplicar marcadores existentes salvo re-scan manual explícito.

#### Scenario: New marker written to custom.json
- **WHEN** a marker is detected for an episode with rating key `12345`
- **THEN** `custom.json` contains an entry for rating key `12345` with the marker's start_ms, end_ms, and kind

#### Scenario: Existing marker not duplicated
- **WHEN** `custom.json` already contains a marker for rating key `12345`
- **THEN** the system does not overwrite it unless explicitly triggered by a manual re-scan

#### Scenario: Novo marcador gravado em custom.json e SQLite
- **WHEN** um marcador é detectado para um episódio com rating key 12345
- **THEN** custom.json contém uma entrada para rating key 12345 com start_ms, end_ms e kind, e o SQLite também persiste o marcador

#### Scenario: Marcador existente não duplicado
- **WHEN** custom.json já contém um marcador para rating key 12345
- **THEN** o sistema não sobrescreve salvo que um re-scan manual seja disparado

#### Scenario: Marcador criado via API gravado em ambos
- **WHEN** um usuário envia { rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 } via API
- **THEN** o sistema armazena o marcador em SQLite e escreve em custom.json

#### Scenario: Marcador removido via API removido de ambos
- **WHEN** um usuário remove um marcador por rating key e kind
- **THEN** o sistema remove do SQLite e reescreve custom.json sem aquela entrada

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

### Requirement: Query stored markers
O sistema SHALL expor endpoints para listar todos os marcadores armazenados e recuperar marcadores de um episódio ou biblioteca específico.

#### Scenario: List all markers
- **WHEN** a user requests `GET /api/markers`
- **THEN** the system returns all markers stored in SQLite with their rating key, kind, start_ms, and end_ms

#### Scenario: Markers for a specific episode
- **WHEN** a user requests `GET /api/markers?rating_key=12345`
- **THEN** the system returns only markers associated with rating key `12345`

#### Scenario: Listar todos os marcadores
- **WHEN** um usuário requisita GET /api/markers
- **THEN** o sistema retorna todos os marcadores armazenados em SQLite com rating key, kind, start_ms e end_ms

#### Scenario: Marcadores de um episódio específico
- **WHEN** um usuário requisita GET /api/markers?rating_key=12345
- **THEN** o sistema retorna apenas os marcadores associados ao rating key 12345

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
