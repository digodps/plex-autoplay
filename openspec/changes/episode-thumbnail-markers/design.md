# Design

## Context

O frontend renderiza marcadores por episódio em `MarkerEditorView.vue`, iterando sobre `seasons` e chamando `MarkerRow.vue` para `intro` e `credits`. Atualmente `PlexEpisodeDto` e `SeriesEndpoints.cs` não retornam a propriedade `thumb` do episódio retornado pelo Plex. Veja `proposal.md` para motivação.

## Goals / Non-Goals

**Goals:**
- Estender `PlexEpisodeDto` com campo `Thumb` (string opcional).
- Extrair `Thumb` no `PlexResponseParser` ao ler episódios da API do Plex.
- Mapear `thumb` no endpoint `GET /api/series/{sectionKey}/{seriesKey}/episodes` usando `/api/images/{Thumb}` (idêntico ao catálogo de séries).
- Na tela de marcadores (`MarkerEditorView.vue`), exibir miniatura (aspect 16:9 ou ~2:3 similar ao Plex) em `div` à esquerda, com título e linhas de marcador alinhados à direita.
- Fallback para ícone quando `thumb` for nulo ou falhar.

**Non-Goals:**
- Não alterar persistência de banco de dados (SQLite Markers).
- Não alterar lógica de detecção de áudio/ffprobe.
- Não refatorar componentes alheios a marcadores.

## Decisions

1. **Reaproveitar proxy existente de imagens (`/api/images/...`)**
   - *Decisão*: usar a rota já existente que anexa o token de autenticação do Plex server-side.
   - *Alternativa*: carregar direto do Plex via URL absoluta com token no frontend (rejeitada por vazar token e quebrar CORS).

2. **Layout com flexbox horizontal por card de episódio**
   - *Decisão*: organizar cada `<article>` de episódio com flex horizontal: miniatura fixa à esquerda (`w-64`, `aspect-video`), bloco de detalhes (título, marcadores intro/credits) à direita.
   - *Alternativa*: colocar a imagem dentro do `MarkerRow` (rejeitada: duplicaria a miniatura para intro e créditos no mesmo episódio).

3. **Fallback seguro para imagens quebradas**
   - *Decisão*: exibir container com fundo neutro e `<AppIcon name="tv" />` se `thumb` for nulo ou se o `<img>` disparar erro `@error`.

## Risks / Trade-offs

- [Plex não retorna `thumb` para todos os episódios] → UI usa fallback visual padronizado com ícone, sem quebrar o layout.
- [Atraso no carregamento da imagem] → Atributo `loading="lazy"` e dimensionamento reservado evitam salto de layout (layout shift).
