# Tasks — mostrar-senha-login

## Phase 1 — Artefatos e validação do spec

- [x] EGM A/S/C sobre proposta/spec/design (achados em `deficiencias_resolvidas.md`).
- [x] `openspec validate mostrar-senha-login --strict` passa.
- [x] Commit dos artefatos da change na worktree.

## Phase 2 — Ícones

- [x] Adicionar ícones `eye` e `eye-off` ao `AppIcon.vue` com `stroke="currentColor"`.

## Phase 3 — Campos de senha

- [x] `SigninView.vue`: ícone de mostrar/ocultar na senha do login (`data-testid="toggle-password-signin"`).
- [x] `SetupView.vue`: ícone em nova senha, confirmação e token do Plex.
- [x] `ChangePasswordView.vue`: ícone em senha atual, nova senha e confirmação.
- [x] `PasswordModal.vue`: ícone em senha atual, nova senha e confirmação.
- [x] `SettingsView.vue`: ícone no token do Plex (`id="plex-token"`, `data-testid="plex-token"`).

## Phase 4 — Build

- [x] `npm run lint` passa em `vue/plex-frontend`.
- [x] `npm run build` passa em `vue/plex-frontend`.
- [x] Build da imagem Docker com o `dist` atualizado.

## Phase 5 — QA

- [x] `openspec validate mostrar-senha-login --strict` passa.
- [x] Playwright em `http://localhost:8080`: ícone presente e funcional (oculta/exibe senha) nos campos; tema light/dark aplicado; menus abrindo/navegando/fechando com estado ativo.
- [x] `qa-evidence/test-ui.spec.js` PASS (1 test, URL final: http://localhost:8080/settings).
