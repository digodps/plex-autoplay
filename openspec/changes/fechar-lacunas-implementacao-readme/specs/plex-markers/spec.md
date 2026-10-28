# plex-markers Specification Delta

## MODIFIED Requirements

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
O sistema SHALL prover UI e API para usuários criar, atualizar e remover marcadores de intro e créditos por episódio.

#### Scenario: User creates a manual marker
- **WHEN** a user submits `{ rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 }` via the API
- **THEN** the system stores the marker in SQLite and writes it to `custom.json`

#### Scenario: User removes a marker
- **WHEN** a user deletes a marker by rating key and kind
- **THEN** the system removes it from SQLite and rewrites `custom.json` without that entry

#### Scenario: Usuário cria um marcador manual
- **WHEN** um usuário envia { rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 } via a API
- **THEN** o sistema armazena o marcador em SQLite e escreve em custom.json

#### Scenario: Usuário remove um marcador
- **WHEN** um usuário remove um marcador por rating key e kind
- **THEN** o sistema remove do SQLite e reescreve custom.json sem aquela entrada

#### Scenario: Usuário atualiza um marcador existente
- **WHEN** um usuário envia uma requisição de atualização para um marcador existente com novos start_ms e end_ms
- **THEN** o sistema atualiza o marcador em SQLite e reescreve custom.json com os novos valores

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
