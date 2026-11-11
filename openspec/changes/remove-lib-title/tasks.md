# Tasks

## 1. Remover título duplicado da página de séries

- [x] 1.1 Remover o bloco `<div class="border-b ...">` contendo `<h1>Biblioteca → ...</h1>` e `<p>Biblioteca selecionada.</p>` de `src/views/Series/SeriesView.vue`.
- [x] 1.2 Testes de breadcrumb (`node --test tests/breadcrumb.test.mjs`) 12/12 PASS — breadcrumb.js não alterado, header migalha mantida.

## 2. Build, QA e validação

- [x] 2.1 `npm run lint` em `vue/plex-frontend`: ESLint dep issue (missing @eslint/js) — issue pré-existente no worktree, não bloqueante; código sem erros sintáticos.
- [x] 2.2 `npm run test:breadcrumb` 12/12 PASS.
- [x] 2.3 `openspec validate remove-lib-title --strict` passa.
- [x] 2.4 Build do frontend `npm run build` passa.
- [x] 2.5 QA UI Playwright: tema + menus 3/3 PASS; página de séries não exibe "Biblioteca →" nem "Biblioteca selecionada." (novo teste qa-remove-lib-title.spec.js 1/1 PASS).
