# plex-markers Specification Delta

## MODIFIED Requirements

### Requirement: Detect intro markers from audio silence
O sistema SHALL analisar um arquivo de mídia usando detecção de silêncio do ffprobe e identificar segmentos contíguos de silêncio no início do episódio como candidatos a marcadores de intro.

#### Scenario: Intro detectado no início do episódio
- **WHEN** ffprobe encontra silêncio de 0ms a aproximadamente 90000ms em um episódio de série
- **THEN** o sistema cria um marcador do tipo intro com start_ms=0 e end_ms igual ao fim do silêncio

#### Scenario: Sem silêncio no início do episódio
- **WHEN** ffprobe não encontra silêncio significativo nos primeiros 120 segundos de áudio
- **THEN** o sistema não cria um marcador de intro para aquele episódio

### Requirement: Detect credits markers from audio silence
O sistema SHALL analisar um arquivo de mídia usando detecção de silêncio do ffprobe e identificar segmentos contíguos de silêncio próximo do final do episódio como candidatos a marcadores de créditos.

#### Scenario: Créditos detectados no final do episódio
- **WHEN** ffprobe encontra silêncio de aproximadamente 85% da duração até o final do episódio
- **THEN** o sistema cria um marcador do tipo credits com start_ms no início do silêncio e end_ms na duração do episódio

#### Scenario: Episódio menor que o limite mínimo de créditos
- **WHEN** a duração do episódio é menor que 300000ms
- **THEN** o sistema não tenta detecção de créditos para aquele episódio

### Requirement: Persistir marcadores em custom.json e SQLite
O sistema SHALL escrever marcadores detectados ou criados manualmente em custom.json no formato PlexAutoSkip, keyed por rating key, e também persistir em SQLite, sem duplicar marcadores existentes salvo re-scan manual explícito.

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

### Requirement: Criação e edição manual de marcadores
O sistema SHALL prover UI e API para usuários criar, atualizar e remover marcadores de intro e créditos por episódio.

#### Scenario: Usuário cria um marcador manual
- **WHEN** um usuário envia { rating_key, kind: "intro", start_ms: 1000, end_ms: 90000 } via a API
- **THEN** o sistema armazena o marcador em SQLite e escreve em custom.json

#### Scenario: Usuário remove um marcador
- **WHEN** um usuário remove um marcador por rating key e kind
- **THEN** o sistema remove do SQLite e reescreve custom.json sem aquela entrada

#### Scenario: Usuário atualiza um marcador existente
- **WHEN** um usuário envia uma requisição de atualização para um marcador existente com novos start_ms e end_ms
- **THEN** o sistema atualiza o marcador em SQLite e reescreve custom.json com os novos valores

### Requirement: Consultar marcadores armazenados
O sistema SHALL expor endpoints para listar todos os marcadores armazenados e recuperar marcadores de um episódio ou biblioteca específico.

#### Scenario: Listar todos os marcadores
- **WHEN** um usuário requisita GET /api/markers
- **THEN** o sistema retorna todos os marcadores armazenados em SQLite com rating key, kind, start_ms e end_ms

#### Scenario: Marcadores de um episódio específico
- **WHEN** um usuário requisita GET /api/markers?rating_key=12345
- **THEN** o sistema retorna apenas os marcadores associados ao rating key 12345
