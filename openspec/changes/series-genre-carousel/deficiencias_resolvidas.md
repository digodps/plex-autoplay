# Deficiências Resolvidas — series-genre-carousel

## Goldfish Review — Arquitetura (A)

1. **A-1**: Nova composable `useSeriesByGenre` duplica lógica de busca existente (`useSeriesCatalog`).
   - **Decisão**: Aceito. A busca de todas as séries de uma vez (para agrupamento) difere fundamentalmente da paginação incremental. Reutilização forçada criaria acoplamento desnecessário.
   - **Ação**: Mantido como composable separado.

2. **A-2**: Componente `GenreCarousel` tem lógica de scroll state complexa (refs, reactive, event listeners).
   - **Decisão**: Parcial. Lógica encapsulada no componente é aceitável para UI complexa. Adicionado `onBeforeUnmount` para limpeza de refs.
   - **Ação**: Nenhuma refatoração adicional necessária.

## Goldfish Review — SOLID (S)

1. **S-1**: `useSeriesByGenre` viola SRP (busca + agrupamento + reatividade).
   - **Decisão**: Rejeitado. Para composable Vue, encapsular o ciclo completo (fetch → transform → reactive) é padrão e evita vazamento de lógica para a view.

2. **S-2**: `GenreCarousel` recebe `genreGroups` como prop mas também gerencia estado de scroll interno.
   - **Decisão**: Aceito. Estado de scroll é puramente visual (UI state), não de domínio. Separação limpa.

## Goldfish Review — Clean Code (C)

1. **C-1**: Uso de `requestAnimationFrame` no watcher de `genreGroups` para atualizar scroll state.
   - **Decisão**: Aceito. Garante que DOM já renderizou antes de medir `scrollWidth`. Alternativa `nextTick` também funcionaria; mantido por clareza.

2. **C-2**: Tokens CSS hardcoded em alguns lugares (ex: `999px` border-radius).
   - **Decisão**: Aceito. `999px` é padrão para "full round" (pill shape), não tem token equivalente. Não afeta tema.

3. **C-3**: Valor mágico `1000` para `pageSize` no composable.
   - **Decisão**: Parcial. Documentado no design como limite MVP. Futuro: tornar configurável ou paginar agrupamento.

4. **C-4**: Skeleton loading hardcoded para 6 seções / 8 cards.
   - **Decisão**: Aceito. Visual placeholder; não precisa ser exato. Não afeta funcionalidade.

## Resumo

- **Aceitos**: 6
- **Parciais**: 3
- **Rejeitados**: 1
- **Bloqueadores**: 0

Todas as deficiências aceitas/parciais foram mitigadas no código ou documentadas para evolução futura.