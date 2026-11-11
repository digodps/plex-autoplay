# Spec Delta

## Purpose

Defines behavior discovering Plex libraries, filtering series-only libraries, mapping library content local media file paths via MEDIA_ROOT.

## ADDED Requirements

### Requirement: Discover Plex libraries
O sistema SHALL consultar API do Plex Media Server para listar todas seções de biblioteca disponíveis, filtrar do tipo "show" persistir cada biblioteca de séries no banco local com rating key, título, tipo e agente.

#### Scenario: Successful library discovery
- **WHEN** o sistema chama GET /library/sections com um token válido do Plex
- **THEN** o sistema retorna uma lista de bibliotecas com key, title, type e agent

#### Scenario: Plex server unreachable
- **WHEN** o servidor Plex em PLEX_URL não está acessível
- **THEN** o sistema retorna um erro indicando que o servidor Plex está indisponível

#### Scenario: Series library persisted local database
- **WHEN** o sistema descobre uma biblioteca do tipo "show" via API do Plex
- **THEN** o sistema insere ou atualiza o registro da biblioteca na tabela de bibliotecas do banco local com rating key, title, type e agent

#### Scenario: Series library included
- **WHEN** uma seção de biblioteca do Plex tem type="show"
- **THEN** o sistema inclui na lista de bibliotecas de séries

#### Scenario: Movie library excluded
- **WHEN** uma seção de biblioteca do Plex tem type="movie"
- **THEN** o sistema exclui da lista de bibliotecas de séries

### Requirement: Map episodes local media files
O sistema SHALL listar os episódios de cada biblioteca de séries consultando API do Plex em `/library/sections/{sectionKey}/all?type=4`, resolver o caminho do arquivo de mídia de cada episódio via `MediaPart.file` + lista `MEDIA_ROOTS` e retornar miniatura do episódio como URL do proxy `/api/images/{thumb}`.

#### Scenario: Episode list retrieved series library
- **WHEN** o sistema consulta os episódios de uma biblioteca de tipo `show`
- **THEN** o sistema chama `/library/sections/{sectionKey}/all?type=4`
- **AND** retorna uma lista de episódios com rating key, título, temporada, número do episódio, caminho do arquivo de mídia e `thumb` no formato `"/api/images/{Thumb}"`

#### Scenario: Episode without thumbnail
- **WHEN** o Plex não fornece `Thumb` para um episódio
- **THEN** o campo `thumb` é `null` e o episódio permanece na lista

#### Scenario: File path resolved via MEDIA_ROOT
- **WHEN** o `MediaPart.file` de um episódio é `/Volumes/Media/Series/Show/Season 1/Episode 1.mkv` e `MEDIA_ROOTS=["/media","/mnt/nas"]`
- **THEN** o sistema retorna `local_path` do primeiro candidato existente em disco entre raízes configuradas
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
