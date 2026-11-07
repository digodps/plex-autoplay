# plex-libraries Specification

## Purpose
Defines the behavior for discovering Plex libraries, filtering to series-only libraries, and mapping library content to local media file paths via MEDIA_ROOT.

## Requirements

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

### Requirement: Filter to series libraries only
O sistema SHALL incluir apenas bibliotecas do tipo "show" (séries) no conjunto sincronizado.

#### Scenario: Series library included
- **WHEN** uma seção de biblioteca do Plex tem type="show"
- **THEN** o sistema a inclui na lista de bibliotecas de séries

#### Scenario: Movie library excluded
- **WHEN** uma seção de biblioteca do Plex tem type="movie"
- **THEN** o sistema não a inclui nas bibliotecas sincronizadas

### Requirement: Map episodes to local media files

O sistema SHALL listar os episódios de cada biblioteca de séries consultando a API do Plex em `/library/sections/{sectionKey}/all?type=4`, resolver o caminho do arquivo de mídia de cada episódio via `MediaPart.file` + `MEDIA_ROOT` e retornar a miniatura do episódio como URL do proxy `/api/images/{thumb}`.

#### Scenario: Episode list retrieved for a series library

- **WHEN** o sistema consulta os episódios de uma biblioteca de tipo `show`
- **THEN** o sistema chama `/library/sections/{sectionKey}/all?type=4`
- **AND** retorna uma lista de episódios com rating key, título, temporada, número do episódio, caminho do arquivo de mídia e `thumb` no formato `"/api/images/{Thumb}"`

#### Scenario: Episode without thumbnail

- **WHEN** o Plex não fornece `Thumb` para um episódio
- **THEN** o campo `thumb` é `null` e o episódio permanece na lista

#### Scenario: File path resolved via MEDIA_ROOT

- **WHEN** o `MediaPart.file` de um episódio é `/Volumes/Media/Series/Show/Season 1/Episode 1.mkv` e `MEDIA_ROOT=/media`
- **THEN** o sistema resolve o caminho local como `/media/Volumes/Media/Series/Show/Season 1/Episode 1.mkv`
- **AND** o campo `thumb` não é afetado pela resolução de caminho

#### Scenario: Media file not accessible

- **WHEN** o caminho do arquivo de mídia resolvido não existe em disco
- **THEN** o sistema loga o arquivo ausente e ignora a detecção de marcadores para aquele episódio

#### Scenario: Non-episode items are ignored

- **WHEN** a resposta do Plex contém itens que não são do tipo `episode`
- **THEN** o sistema ignora esses itens e não os inclui na lista de episódios

#### Scenario: Plex rejects the episode query

- **WHEN** o Plex retorna status diferente de `2xx` para a consulta de episódios
- **THEN** o sistema retorna erro explícito e o scan contabiliza a biblioteca como falha
