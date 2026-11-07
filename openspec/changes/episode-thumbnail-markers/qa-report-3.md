# QA Report — Iteração 3

**Status:** PASS

## Mudança aplicada

- `MarkerEditorView.vue`: o botão "Voltar" foi substituído por `BreadcrumbDefault` no mesmo local (topo do conteúdo), com itens "Bibliotecas" → biblioteca (quando vier na query) → série.
- Navegação do breadcrumb usa `router.push` no evento `select`; a série atual é marcada como página.

## Evidências

1. **Frontend**
   - `npm run build`: sucesso; assets copiados para `src/PlexAutoplay.Web/wwwroot/`.
   - Miniatura preservada em `aspect-video w-64`.

2. **Backend**
   - `dotnet test`: 3/3 passed.

3. **Spec / validação**
   - `openspec validate episode-thumbnail-markers --strict`: válido.
