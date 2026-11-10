# Tasks — mostrar-senha-login

## Phase 1 — Artefatos e validação do spec

- [ ] EGM A/S/C sobre proposta/spec/design (achados em `deficiencias_resolvidas.md`).
- [ ] `openspec validate mostrar-senha-login --strict` passa.
- [ ] Commit somente dos artefatos da change na worktree.

## Phase 2 — Ícones

- [ ] Adicionar ícones `eye` e `eye-off` ao `AppIcon.vue`.

## Phase 3 — Campos de senha

- [ ] `SigninView.vue`: ícone de mostrar/ocultar na senha do login.
- [ ] `SetupView.vue`: ícone em nova senha, confirmação e token do Plex.
- [ ] `ChangePasswordView.vue`: ícone em senha atual, nova senha e confirmação.
- [ ] `PasswordModal.vue`: ícone em senha atual, nova senha e confirmação.
- [ ] `SettingsView.vue`: ícone no token do Plex.

## Phase 4 — Build

- [ ] `npm run lint` passa em `vue/plex-frontend`.
- [ ] `npm run build` passa em `vue/plex-frontend`.

## Phase 5 — QA

- [ ] `openspec validate mostrar-senha-login --strict` passa.
- [ ] Playwright em `http://localhost:<port>`: ícone presente e funcional (oculta/exibe
  senha) nos campos; tema light/dark aplicado; menus abrindo/navegando/fechando com
  estado ativo.
- [ ] `qa-report-1.md` com PASS e achados numerados + URL acessada.
