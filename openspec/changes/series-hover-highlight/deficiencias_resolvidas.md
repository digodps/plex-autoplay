# Deficiências resolvidas — EGM A/S/C

Change: `series-hover-highlight`. Escopo: hover/focus destaca miniatura do card de série.

| # | Achado | Decisão | Ação |
|---|---|---|---|
| A1/S4/C3/S5 | Falta indicador de foco visível no `<button>` do card | Aceito | Adicionar `focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900`, padrão já usado no app. |
| A1/C5 | Overlay `absolute` exige contêiner `relative` | Aceito | Contêiner da miniatura recebe `relative overflow-hidden`; design/tasks com lista literal de classes. |
| A4 | Série sem `thumb` (placeholder) não recebe zoom/overlay | Aceito | Aplicar transição/overlay no wrapper interno, não só na `<img>`, cobrindo ambos os ramos. |
| A5 | `motion-reduce:transform-none` conflita com scale | Aceito | Usar `motion-reduce:scale-100` + `motion-reduce:transition-none`. |
| C2 | Overlay opcional deixaria reduced-motion sem feedback | Aceito | Overlay obrigatório; spec scenario 3 exige overlay/borda mesmo sem zoom. |
| C1 | Sem verificação executável do hover | Aceito | Adicionar asserts ao `qa-ui-check.mjs` (transform no hover, Tab, reduced-motion). |
| S6/C6 | `dark:hover:text-primary` sem par claro e sem efeito nos filhos | Aceito (parcial) | Remover classe morta; manter `hover:border-primary` e adicionar anel de foco. |
| A2/S2/C4 | `viewState` nunca retorna `seriesError`; bloco de erro morto; bloco duplicado com fundo ilegível no tema claro | Rejeitado | Pré-existente e fora do escopo do hover. Registrado para change futura. |
| S3 | Extrair `ErrorNotice.vue` | Rejeitado | Refactor não relacionado; aumenta diff. |
| S5 | `<h4>` dentro de `<button>` | Rejeitado | Pré-existente; fora do escopo. |
| A6 | Build .NET em tasks vs. backend intocado | Aceito (parcial) | Mantido como build gate do orquestrador; nenhum arquivo backend muda. |
