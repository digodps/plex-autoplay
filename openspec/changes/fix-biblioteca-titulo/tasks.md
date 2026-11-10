# Tasks — fix-biblioteca-titulo

## Phase 1 — EGM e Artefatos
- [x] Proposta e Design criados.
- [ ] Goldfish Review (A/S/C) executado, achados em `deficiencias_resolvidas.md`.
- [ ] Commit dos artefatos da change na worktree.

## Phase 2 — Implementação
- [ ] `SeriesView.vue`: renomear "Séries" para "Biblioteca" no título e cabeçalho.
- [ ] `SidebarArea.vue`: renomear "Séries" para "Biblioteca" no menu.
- [ ] `router/index.js`: atualizar `meta.title` de "Séries" para "Biblioteca".
- [ ] `breadcrumb.js`: atualizar fallback de "Série" para "Biblioteca".
- [ ] `breadcrumb.test.mjs`: atualizar asserções de teste para "Biblioteca".
- [ ] `AppHeader.vue`: ajustar padding horizontal para `px-4 md:px-6` alinhando com conteúdo principal.

## Phase 3 — Build & Test
- [ ] Executar testes de breadcrumb (`npm test` / vitest/node test).
- [ ] Compilar frontend (`npm run build`).
- [ ] Compilar backend e copiar wwwroot.

## Phase 4 — QA
- [ ] Validação UI (tema light/dark, menus).
- [ ] `openspec validate fix-biblioteca-titulo --strict`.
