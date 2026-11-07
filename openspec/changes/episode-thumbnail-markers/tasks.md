# Tasks

## 1. Backend: Suporte a miniatura de episódio

- [x] 1.1 Adicionar propriedade `Thumb` ao `PlexEpisodeDto` e preenchê-la no `PlexResponseParser.ParseEpisodesInternal` - verificar com testes unitários em `tests/PlexAutoplay.Tests/`
- [x] 1.2 Mapear `thumb` no endpoint `GET /api/series/{sectionKey}/{seriesKey}/episodes` para `/api/images/{Thumb}` caso preenchido - verificar com teste de integração/unitário do endpoint

## 2. Frontend: Exibir miniatura do episódio ao lado dos marcadores

- [x] 2.1 Atualizar `MarkerEditorView.vue` para incluir container flex com `div` da miniatura à esquerda e dados/marcadores à direita, com fallback para ícone `tv` e loading lazy - verificar renderização visual e classes CSS
- [x] 2.2 Recompilar frontend (`npm run build` em `vue/plex-frontend`) e verificar saída sem erros

## 3. Verificação e Validação

- [x] 3.1 Executar testes backend (`dotnet test`) e lint/build frontend (`npm run build`)
- [x] 3.2 Executar `openspec validate episode-thumbnail-markers --strict`
