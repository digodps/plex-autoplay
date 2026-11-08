# Deficiências Resolvidas — EGM

Revisão Goldfish (A/S/C) em 2026-11-08 sobre `proposal.md`, `design.md`,
`specs/**` e `tasks.md` de `redesign-breadcrumb`.

| # | Revisor | Achado | Classificação | Resolução |
|---|---|---|---|---|
| A1 | A | Marcadores dependem de `query` e perdem `Bibliotecas` em deep link | Aceito | Cenário de deep link sem metadados e fallback `Série` adicionados; `Bibliotecas` sempre presente |
| A2 | A | Conflito com spec `series-full-page-view` | Aceito | Delta `MODIFIED` em `specs/series-full-page-view/spec.md` |
| A3 | A | Estado de loading indefinido | Aceito | Cenário `Biblioteca carregando` → `Início > Bibliotecas` |
| A4 | A | Quatro níveis em tela estreita | Aceito | Rolagem horizontal da trilha especificada e task 2.1 |
| A5 | A | Sem fallback de biblioteca em marcadores | Parcial | Deep link sem biblioteca omite o nível e usa `Série`; com chave/título inválidos usa `Biblioteca indisponível` |
| A6 | A | Fallback `query.libraryKey` sem emissor na rota `series` | Rejeitado | Mantido como suporte a deep link manual; sem custo e coberto pela função pura |
| A7 | A | `AppIcon size="xs"` igual a `sm` | Aceito | Design passa a prescrever `size="sm"` |
| A8 | A | Separador sem teste executável | Parcial | Função pura coberta por teste; ícone validado por Playwright no QA |
| S1 | S | `AppHeader` acumula responsabilidades | Rejeitado | Extração adiciona arquivo e abstração sem segundo consumidor; registrado como non-goal |
| S2 | S | `buildBreadcrumb` if/else dificulta extensão | Rejeitado | Cinco rotas estáveis; mapa genérico seria overengineering agora |
| S3 | S | Shape de item inconsistente | Aceito | Design exige `{ label, to?, isCurrent }` em todos os itens |
| S4 | S | Dependência do objeto `route` do vue-router | Rejeitado | Contrato estável do router; função permanece pura |
| S5 | S | Trilha definida em vários artefatos | Parcial | Spec vira fonte dos cenários; design só resume a tabela |
| S6 | S | Loading remove item atual | Aceito | Cenário define `Início > Bibliotecas` e regra de último item atual |
| S7 | S | Resolução de dados e strings de UI juntas | Rejeitado | Escopo pequeno; mover para view adicionaria camada sem ganho |
| S8 | S | Duplicação do item `Bibliotecas` | Aceito | Constante `LIBRARIES` ao lado de `HOME` |
| C1 | C | `series-markers` sem `Bibliotecas`/fallback | Aceito | Mesma resolução de A1/A5 |
| C2 | C | Task do separador sem verificação automatizada | Parcial | QA Playwright valida `data-icon="chevron-right"` e ausência de `/` |
| C3 | C | Testes validam só rótulos | Aceito | Task 1.2 exige asserções de `label`, `to` e `isCurrent` |
| C4 | C | Rota desconhecida sem item atual | Aceito | Design define `Início` como atual quando a trilha tiver um item |
| C5 | C | Proposal declarava MODIFIED, delta usa ADDED | Aceito | Proposal corrigido para New Capabilities + modified `series-full-page-view` |
| C6 | C | Literais de rota repetidos | Aceito | Constante `LIBRARIES`; nomes de rota permanecem explícitos |
| C7 | C | `aria-hidden` redundante e `xs` inexistente | Aceito | Design usa `size="sm"` e não exige `aria-hidden` extra |
| C8 | C | `v-for` com chave por índice | Aceito | Task 2.1 usa chave por `item.label` |
