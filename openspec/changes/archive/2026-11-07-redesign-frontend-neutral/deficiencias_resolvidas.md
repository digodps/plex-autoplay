# Deficiências Resolvidas — EGM (Goldfish Review A/S/C)

> Revisão realizada sobre `proposal.md`, `design.md`, `specs/web-ui/spec.md` e `tasks.md` antes da implementação.

## Revisor A — Arquitetura

| ID | Achado | Classificação | Resolução |
|----|--------|---------------|-----------|
| A1 | `tokens.css` sem `--speed`, `--ease`, `--shadow-1/2`, `--bg-sunken`, `--mono` | Aceito | Já previsto na tarefa 2.1; tokens obrigatórios. |
| A2 | `<style scoped>` por view viola o padrão único | Aceito | Tarefa 1.1 remove blocos scoped; QA verifica ausência. |
| A3 | CSS por view/componente existe | Aceito | Tarefa 1.1 exclui `src/views/*.css` e `src/styles/components/`. |
| A4 | `@material-tailwind/html` sem uso claro | Aceito | Remover dependência e integração; manter `preflight: false`. |
| A5 | `AppSidebar` contradiz breadcrumb-first | Aceito | Tarefa 3.1 remove sidebar e cria top bar. |
| A6 | `Breadcrumb.vue` tem scoped CSS e importa `vue-router` | Aceito | Componente presentacional, emite `select`, sem CSS próprio. |
| A7 | Falta `stores/ui.js` para títulos reais | Aceito | Tarefa 6 cria `ui` store e wiring. |
| A8 | Rota `/ajustes` extra | Rejeitado | Mantida como redirect de compatibilidade; não impacta as 7 telas. |
| A9 | Persistência de tema precisa ser confirmada | Aceito | Tarefa 5 cobre aplicação antes do mount e persistência. |
| A10 | E2E sem seletores estáveis | Aceito | Adicionar `data-testid` e atualizar specs Playwright. |
| A11 | Transições sem `prefers-reduced-motion` | Aceito | Tarefa 10 cobre media query. |
| A12 | Falta wiring de títulos no breadcrumb | Aceito | Tarefas 6.2/6.3 conectam views ao `ui` store. |

## Revisor S — SOLID / Coesão

| ID | Achado | Classificação | Resolução |
|----|--------|---------------|-----------|
| S1 | `Breadcrumb` importa `vue-router` | Aceito | Emitir evento; shell navega. |
| S2 | Views importam CSS próprio | Aceito | Excluir arquivos e imports. |
| S3 | Scoped `<style>` em componentes | Aceito | Remover; usar classes do `app.css`. |
| S4 | Falta `ui` store | Aceito | Criar store dedicado. |
| S5 | `AppSidebar` ainda no shell | Aceito | Remover do shell. |
| S6 | `MarkerRow` importa store diretamente | Aceito | Refatorar para props/emits; view/composable orquestra. |
| S7 | `SeriesView` usa fallback genérico de título | Aceito | Wiring com `ui` store. |
| S8 | `HelloWorld.vue` legado | Aceito | Excluir. |
| S9 | `useLibrarySeries` duplica `useSeriesCatalog` | Aceito | Manter apenas `useSeriesCatalog`; excluir `useLibrarySeries`. |
| S10 | Tokens incompletos | Aceito | Definir todos os tokens usados. |

## Revisor C — Clean Code / Escopo / Testabilidade

| ID | Achado | Classificação | Resolução |
|----|--------|---------------|-----------|
| C1 | Scoped styles em views | Aceito | Remover todos. |
| C2 | CSS per-view importado | Aceito | Excluir todos. |
| C3 | `Breadcrumb` importa router | Aceito | Componente presentacional. |
| C4 | Tokens faltantes | Aceito | Definir no `tokens.css`. |
| C5 | Falta `ui` store | Aceito | Criar e conectar. |
| C6 | `App.vue` não tem top bar nova | Aceito | Reescrever shell. |
| C7 | Sem `data-testid` para E2E | Aceito | Adicionar hooks estáveis. |
| C8 | Ícones via sprite caseiro | Aceito | Trocar por `lucide-vue-next`. |
| C9 | Toggle de tema ausente da top bar | Aceito | Mover para top bar. |
| C10 | Breakpoints responsivos inconsistentes | Aceito | Padronizar `max-width: 767.98px`. |
| C11 | Seletores E2E frágeis | Aceito | Migrar para `data-testid`. |
| C12 | Sem `<transition>` no router-view | Aceito | Adicionar transição `page`. |

## Achados adicionais incorporados aos artefatos

1. Remover `@material-tailwind/html` do `package.json` e da configuração, mantendo `preflight: false`.
2. Excluir `useLibrarySeries.js`; manter apenas `useSeriesCatalog.js`.
3. Refatorar `MarkerRow.vue` para receber `marker`, `saving` e emitir `save`/`remove`.
4. Adicionar `data-testid` em login, breadcrumb, cards, ações, linhas de marcador, log e toggle de tema.
5. Adicionar verificação de fonte no QA: nenhum `<style` em `src/**/*.vue`, nenhum import de `.css` em views/components, nenhum hex/rgba fora de `tokens.css`.
6. Manter rota `/ajustes` como redirect para `/settings` (compatibilidade).

## Status

- Artefatos atualizados com todos os aceitos: **sim**.
- Implementação iniciada: **não** (aguardando apply).
- QA: pendente.
