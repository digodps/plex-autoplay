# Change Proposal — mostrar-senha-login

## Why

Os campos de senha (login, setup, troca de senha e token do Plex nos ajustes) usam
`type="password"` sem que o usuário possa conferir o que digitou. Sem um ícone de
exibir/ocultar, erros de digitação forçam redefinição de senha.

## What Changes

- Adicionar ícone de olho (`eye`/`eye-off`) ao lado de cada campo `type="password"` para
  alternar a visibilidade da senha.
- Campos abrangidos: login (`/login`), setup (nova senha, confirmação, token do Plex),
  troca de senha (atual, nova, confirmação) e ajustes (token do Plex).
- Adicionar ícones `eye` e `eye-off` ao `AppIcon.vue`.
- Não altera backend, API, banco nem escopo de CRUD.

## Capabilities

### New Capabilities

- `password-visibility`: toggle de exibição de senha em todos os campos
  `type="password"` da UI.

### Modified Capabilities

- (nenhum)

## Impact

- Frontend: `vue/plex-frontend/src/components/AppIcon.vue`,
  `views/Authentication/SigninView.vue`, `views/Authentication/SetupView.vue`,
  `views/Authentication/ChangePasswordView.vue`,
  `components/PasswordModal.vue`, `views/Pages/SettingsView.vue`.
- Sem novas dependências.
