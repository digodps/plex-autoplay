# Spec Delta

## MODIFIED Requirements

### Requirement: Map episodes to local media files

O sistema SHALL listar os episódios de cada biblioteca de séries consultando a API do Plex em `/library/sections/{sectionKey}/all?type=4` e resolver o caminho do arquivo de mídia de cada episódio usando o valor `MediaPart.file` do Plex e a `MEDIA_ROOT` configurada.

#### Scenario: Episode list retrieved for a series library

- **WHEN** o sistema consulta os episódios de uma biblioteca de tipo `show`
- **THEN** o sistema chama `/library/sections/{sectionKey}/all?type=4`
- **AND** retorna uma lista de episódios com rating key, título, temporada, número do episódio e caminho do arquivo de mídia

#### Scenario: File path resolved via MEDIA_ROOT

- **WHEN** o `MediaPart.file` de um episódio é `/Volumes/Media/Series/Show/Season 1/Episode 1.mkv` e `MEDIA_ROOT=/media`
- **THEN** o sistema resolve o caminho local como `/media/Volumes/Media/Series/Show/Season 1/Episode 1.mkv`

#### Scenario: Media file not accessible

- **WHEN** o caminho do arquivo de mídia resolvido não existe em disco
- **THEN** o sistema loga o arquivo ausente e ignora a detecção de marcadores para aquele episódio

#### Scenario: Non-episode items are ignored

- **WHEN** a resposta do Plex contém itens que não são do tipo `episode`
- **THEN** o sistema ignora esses itens e não os inclui na lista de episódios

#### Scenario: Plex rejects the episode query

- **WHEN** o Plex retorna status diferente de `2xx` para a consulta de episódios
- **THEN** o sistema retorna erro explícito e o scan contabiliza a biblioteca como falha
