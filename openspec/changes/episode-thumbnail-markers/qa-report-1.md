# QA Report — Iteração 1

**Status:** PASS

## Evidências

1. **Backend — campo `thumb` no contrato de episódios**
   - `PlexEpisodeDto` inclui `Thumb` opcional: `src/PlexAutoplay.Application/IPlexApiClient.cs`.
   - `PlexResponseParser` popula campo a partir de `PlexMetadataItem.Thumb`.
   - `SeriesEndpoints` mapeia `thumb` para `/api/images/{Thumb}`.
   - `dotnet test`: 3/3 passed (ArchitectureTests).

2. **Frontend — miniatura do episódio à esquerda dos marcadores**
   - `MarkerEditorView.vue` renderiza `<div class="episode-thumb">` com `<img loading="lazy">` e fallback `<AppIcon name="tv">`.
   - Marcadores (`MarkerRow` para `intro` e `credits`) ficam alinhados à direita no bloco `flex-1`.
   - `npm run build`: concluído com sucesso, sem erros.

3. **Contrato do spec**
   - `GET /api/series/{sectionKey}/{seriesKey}/episodes` retorna `thumb` quando disponível.
   - Tela de marcadores exibe miniatura com fallback para ícone.

4. **OpenSpec validate**
   - `openspec validate episode-thumbnail-markers --strict`: válido.
