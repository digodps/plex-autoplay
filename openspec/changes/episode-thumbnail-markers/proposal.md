# Proposal

## Why

A tela de edição de marcadores lista apenas temporada/episódio/título, deixando o usuário sem referência visual do episódio. O Plex mostra a miniatura do episódio ao lado dos dados; replicar isso reduz erros de edição no episódio errado.

## Changes

- Endpoint `GET /api/series/{sectionKey}/{seriesKey}/episodes` passa a retornar `thumb` (URL do proxy `/api/images/...`) por episódio, usando `Metadata.Thumb` do Plex.
- `PlexEpisodeDto` recebe `Thumb`; `PlexResponseParser` popula o campo.
- Cada episódio na tela de marcadores ganha uma `div` à esquerda com a miniatura (proporção 2:3 ou 16:9, `object-cover`, lazy) e fallback de ícone quando ausente.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `plex-libraries`: o requisito "Map episodes to local media files" passa a incluir a miniatura (`thumb`) de cada episódio no contrato de resposta.
- `plex-markers`: o requisito "Manual marker creation and editing" passa a exigir miniatura do episódio ao lado dos marcadores na UI.

## Impact

- `src/PlexAutoplay.Application/IPlexApiClient.cs` (`PlexEpisodeDto`)
- `src/PlexAutoplay.Infrastructure/PlexProxy/PlexResponseParser.cs`
- `src/PlexAutoplay.Web/Endpoints/SeriesEndpoints.cs`
- `vue/plex-frontend/src/stores/markers.js`
- `vue/plex-frontend/src/views/Markers/MarkerEditorView.vue`
- Sem migração de banco, sem mudança de autenticação, sem novas dependências.
