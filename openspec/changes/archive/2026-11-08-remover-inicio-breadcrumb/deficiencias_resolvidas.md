# EGM — Achados e resolução

Goldfish Review A/S/C sobre `remover-inicio-breadcrumb` (artefatos e código
atual). Legenda: Aceito, Parcial, Rejeitado.

| # | Achado | Classificação | Resolução |
|---|---|---|---|
| A1 | `redesign-breadcrumb` concluído mas não arquivado; specs principais ainda com `Início` | Aceito | `openspec archive redesign-breadcrumb` executado antes deste change; deltas rebaseadas em `MODIFIED` |
| A2 | Spec dizia logo → `/dashboard`, código usa `/` (redirect) | Aceito | Spec e design falam "home (`/`)" |
| A3 | "único acesso à home" era absoluto | Aceito | Escopado para "no cabeçalho" |
| A4 | `<nav aria-label="Breadcrumb">` vazio em dashboard | Aceito | `v-if="items.length"` no `<nav>` (spec + task 2.1) |
| A5 | Cenário de séries duplicado em duas capabilities | Aceito | `web-ui` é dono do contrato; `series-full-page-view` só cita o caso específico |
| A6 | Design contradizia proposal sobre separador logo↔trilha | Aceito | Design define chevron antes de cada item e a leitura com `>` |
| A7 | Invariante "último sempre atual" quebra com dashboard vazio | Aceito | Task 1.2 separa dashboard vazio do invariante de trilha não vazia |
| A8 | Rotas fora de bibliotecas não especificadas | Aceito | Cenário "Rotas fora de bibliotecas" adicionado |
| A9 | Delta de séries muda contrato de a11y de carona | Aceito | Registrado em proposal e design; nome acessível "Bibliotecas" preservado |
| A10 | `:key="item.label"` pode duplicar | Aceito | Chave por índice na task 2.1 |
| A11 | Header sem teste automatizado | Parcial | Cobertura fica no Playwright (task 3.4); teste de fonte seria frágil |
| S1 | Invariante de teste incompatível com trilha vazia | Aceito | Mesma resolução de A7 |
| S2 | Dois mecanismos de separador no template | Aceito | Chevron único antes de cada item; divisor removido |
| S3 | Invariante "logo = home" não codificado | Parcial | Comentário `ponytail:` em `breadcrumb.js`; teste de componente exigiria infra nova |
| S4 | `isCurrent` é apresentação dentro da lib | Rejeitado | Contrato já consumido pelo header e pelos testes; refatorar não agrega a este escopo |
| S5 | Chave por rótulo | Aceito | Mesma resolução de A10 |
| S6 | Requisito de breadcrumb em dois donos | Aceito | Mesma resolução de A5 |
| S7 | Layout carrega dados do header | Rejeitado | Fora de escopo; registrar dívida quando houver segundo consumidor |
| S8 | `if/else` de rotas sem caminho explícito para dashboard | Aceito | `dashboard` retorna `[]` explicitamente (task 1.1) |
| C1 | Teste de invariante quebra com `HOME` removido | Aceito | Mesma resolução de A7 |
| C2 | Condicional do chevron sem teste automático | Parcial | Asserts explícitos no Playwright da task 3.4 |
| C3 | Landmark vazio em dashboard | Aceito | Mesma resolução de A4 |
| C4 | Remoção do link "Voltar às bibliotecas" sem decisão | Aceito | Mesma resolução de A9 |
| C5 | Requisito duplicado entre deltas | Aceito | Mesma resolução de A5 |
| C6 | Vocabulário "trilha"/"migalha"/"breadcrumb" misturado | Parcial | Artefatos usam "trilha" de forma consistente; títulos mantêm o termo da spec |
| C7 | Tabela do design omitia condição de deep link | Aceito | Nível `<biblioteca>` marcado como opcional na tabela |
| C8 | Spec fixava o ícone `chevron-right` | Aceito | Spec fala de seta; ícone fica no design |
| C9 | Deep link não cobria bibliotecas ausentes | Aceito | Caso `[]` + `loading` adicionado à task 1.2 |
| C10 | QA agrupava breadcrumb, tema e menu | Aceito | Task 3.4 lista asserts explícitos por área |
| C11 | Constante `home` órfã no teste | Aceito | Removida na task 1.2 |

## Decisões registradas

- Ordem de arquivamento importa: `redesign-breadcrumb` arquivado antes para
  evitar duas deltas conflitantes em `web-ui`/`series-full-page-view`.
- Nenhuma dependência nova; `chevron-right` e `icon-sm` já existem.
