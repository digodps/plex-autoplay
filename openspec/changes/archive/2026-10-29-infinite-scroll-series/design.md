# Design

## Context

Current state (see proposal.md for motivation):
- `LibrariesView.vue` exibe séries em expansão inline com paginação explícita (botões Anterior/Próxima + indicador de página)
- Composable `useLibrarySeries.js` gerencia estado de paginação (`seriesPage`, `goToSeriesPage`, etc.)
- Endpoint `/api/series/{sectionKey}` já suporta `page` e `pageSize` query params
- Grid de séries usa CSS Grid com `grid-template-columns: repeat(auto-fill, minmax(180px, 1fr))`

## Goals / Non-Goals

**Goals:**
- Substituir paginação explícita por infinite scroll contínuo (estilo Netflix)
- Ocultar barra de rolagem nativa do container de séries
- Implementar lazy loading de imagens de capa via IntersectionObserver
- Manter busca e filtro funcionando (reset ao buscar = primeira página)
- Manter deep-linking via `route.params.key` (auto-expande biblioteca)

**Non-Goals:**
- Mudar API backend (já suporta paginação)
- Mudar estrutura de dados das séries
- Virtualização de DOM (lista não é tão grande para justificar)
- Scroll restoration ao voltar do MarkerEditorView (fora do escopo)

## Decisions

### 1. Composable dedicado `useInfiniteScroll.js`
**Decisão**: Extrair lógica de infinite scroll para composable reutilizável (`src/composables/useInfiniteScroll.js`)

**Racional**: 
- Separa responsabilidades (SRP)
- Reutilizável em outras views futuras
- Encapsula IntersectionObserver, throttling, cleanup

**Alternativas consideradas**:
- Manter lógica inline em `useLibrarySeries.js` → Rejeitado: mistura responsabilidades
- Usar biblioteca externa (vueuse) → Rejeitado: dependência extra desnecessária

### 2. IntersectionObserver vs scroll event
**Decisão**: Usar `IntersectionObserver` com sentinela (elemento no final do grid)

**Racional**:
- Performance: nativo, assíncrono, não bloqueia main thread
- Precisão: dispara exatamente quando sentinela entra na viewport
- Simplicidade: API moderna, bem suportada

**Alternativas consideradas**:
- `window.onscroll` + throttle → Rejeitado: problemas de performance, jank
- Biblioteca (vue-infinite-loading) → Rejeitado: dependência extra

### 3. Sentinela como último filho do grid
**Decisão**: Inserir elemento `<div ref="sentinel">` como último filho do `.series-grid`

**Racional**:
- Simples: quando sentinela entra na viewport, carrega próxima página
- Sem cálculos de posição manual
- Funciona com CSS Grid nativo

### 4. CSS para ocultar scrollbar
**Decisão**: CSS global no container `.series-grid-wrapper` (ou novo wrapper)

```css
.series-grid-wrapper {
  overflow-y: auto;
  scrollbar-width: none;           /* Firefox */
  -ms-overflow-style: none;        /* IE/Edge Legacy */
}
.series-grid-wrapper::-webkit-scrollbar {
  display: none;                   /* Chrome, Safari, Edge */
}
```

**Racional**: 
- Padrão Netflix: scroll funciona mas barra invisível
- Cross-browser: cobre Firefox, Chrome/Safari/Edge, IE/Edge Legacy

### 5. Lazy loading de imagens
**Decisão**: `loading="lazy"` nativo + IntersectionObserver fallback para placeholders

**Racional**:
- `loading="lazy"` é nativo, zero JS, funciona em 95%+ browsers
- IntersectionObserver para detectar quando imagem entra na viewport e trocar placeholder

### 5. Estado de busca reseta infinite scroll
**Decisão**: Ao digitar busca (`seriesQuery`), reseta `page=1`, limpa lista, recarrega

**Racional**: UX esperada - busca nova = nova lista do início

### 6. Loading state com skeletons
**Decisão**: Manter skeletons atuais durante carregamento inicial; adicionar skeleton discreto no final durante fetch de próxima página

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| IntersectionObserver não suportado em browsers muito antigos | Polyfill mínimo (apenas 0.5KB gzipped) ou fallback para scroll event |
| Usuário rola rápido e dispara múltiplos fetches | Debounce/throttle no observer (guard `isLoading`) |
| Perda de posição ao voltar do MarkerEditorView | Fora do escopo; pode ser melhorado depois com `scrollRestoration` |
| Busca + infinite scroll: conflito de estado | Reset completo ao buscar (`page=1`, limpa lista) |
| Memória cresce indefinidamente com muitos itens | Lista de séries típica < 500 itens; não justifica virtualização ainda |

## Migration Plan

1. **Criar `useInfiniteScroll.js`** - composable genérico
2. **Atualizar `useLibrarySeries.js`** - remover paginação, integrar `useInfiniteScroll`
3. **Atualizar `LibrariesView.vue`** - remover paginação UI, adicionar sentinela, CSS scrollbar
4. **Adicionar CSS scrollbar** - global ou scoped no componente
5. **Testar**: busca, deep-link, rolagem rápida, fim da lista, reload
6. **Build + deploy** - `npm run build` → `docker compose build` → `docker compose up -d`

**Rollback**: `git revert` dos commits; API inalterada

## Open Questions

- Qual `pageSize` ideal? (atual 24; Netflix usa ~15-20 por "linha" mas depende do breakpoint)
- Threshold do IntersectionObserver: 80% (atual) ou elemento sentinela visível?
- Manter `totalSeriesPages` computado para indicar fim? (sim, para saber quando parar)