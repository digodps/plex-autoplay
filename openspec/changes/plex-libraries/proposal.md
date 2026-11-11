# Proposal: Plex Libraries Discovery and Episode Mapping

## Why

As bibliotecas de séries do Plex não estão carregando no sistema, impedindo a descoberta e sincronização de conteúdo (séries, temporadas e episódios) necessárias para o funcionamento do autoplay e do mapeamento de arquivos locais via `MEDIA_ROOT`.

## What Changes

- Implementar descoberta de bibliotecas do Plex (`GET /library/sections`), filtrando por tipo `show` e persistindo no banco local.
- Implementar listagem de episódios de bibliotecas de séries (`/library/sections/{sectionKey}/all?type=4`) com resolução de caminho de arquivo local via `MEDIA_ROOTS`.
- Tratar cenários de erro (servidor Plex indisponível, falhas na API, arquivos de mídia inacessíveis).

## Capabilities

### New Capabilities
- `plex-libraries`: Descoberta de bibliotecas de séries do Plex, persistência local e mapeamento de caminhos de arquivos de mídia dos episódios.

### Modified Capabilities
- None

## Impact

- Banco de dados local (tabelas de bibliotecas e episódios).
- Endpoints de API do backend (serviço Plex client/sync).
- Interface web (exibição de bibliotecas de séries).
