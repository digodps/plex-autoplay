# plex-libraries Specification Delta

## MODIFIED Requirements

### Requirement: Discover Plex libraries
O sistema SHALL consultar a API do Plex Media Server para listar todas as seções de biblioteca disponíveis.

#### Scenario: Successful library discovery
- **WHEN** o sistema chama GET /library/sections com um token válido do Plex
- **THEN** o sistema retorna uma lista de bibliotecas com key, title, type e agent

#### Scenario: Plex server unreachable
- **WHEN** o servidor Plex em PLEX_URL não está acessível
- **THEN** o sistema retorna um erro indicando que o servidor Plex está indisponível

### Requirement: Filter to series libraries only
O sistema SHALL incluir apenas bibliotecas do tipo "show" (séries) no conjunto sincronizado.

#### Scenario: Series library included
- **WHEN** uma seção de biblioteca do Plex tem type="show"
- **THEN** o sistema a inclui na lista de bibliotecas de séries

#### Scenario: Movie library excluded
- **WHEN** uma seção de biblioteca do Plex tem type="movie"
- **THEN** o sistema não a inclui nas bibliotecas sincronizadas

### Requirement: List episodes and map to local media files
O sistema SHALL listar os episódios de cada biblioteca de séries e resolver o caminho do arquivo de mídia de cada episódio usando o valor MediaPart.file do Plex e a MEDIA_ROOT configurada.

#### Scenario: Episode list retrieved for a series library
- **WHEN** o sistema consulta os episódios de uma biblioteca de tipo show
- **THEN** o sistema retorna uma lista de episódios com rating key, título, temporada, número do episódio e caminho do arquivo de mídia

#### Scenario: File path resolved via MEDIA_ROOT
- **WHEN** o MediaPart.file de um episódio é /Volumes/Media/Series/Show/Season 1/Episode 1.mkv e MEDIA_ROOT=/media
- **THEN** o sistema resolve o caminho local como /media/Volumes/Media/Series/Show/Season 1/Episode 1.mkv

#### Scenario: Media file not accessible
- **WHEN** o caminho do arquivo de mídia resolvido não existe em disco
- **THEN** o sistema loga o arquivo ausente e ignora a detecção de marcadores para aquele episódio
