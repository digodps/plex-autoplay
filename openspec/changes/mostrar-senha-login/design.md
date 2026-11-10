# Design — mostrar-senha-login

## Context

Ainda não há mecanismo de mostrar/ocultar senha. Cada input de senha é um
`<input type="password">` isolado com `id` e `data-testid`. O `AppIcon.vue` renderiza
SVG inline por nome com `stroke="currentColor"`, portanto herda automaticamente o tema
light/dark.

## Goals / Non-Goals

**Goals:**

- Todo campo `type="password"` da UI ganha um botão de alternância de visibilidade com
  ícone de olho.
- O ícone alterna entre `eye` (senha oculta) e `eye-off` (senha visível) conforme o
  estado do campo.
- Funciona em tema light e dark sem alterar classes de cor.
- Não altera o valor da senha nem o fluxo de submissão.

**Non-Goals:**

- Mostrar senha por padrão.
- Persistir preferência de visibilidade.
- CRUD de senha ou alteração de backend/API/banco.

## Decisions

- [ ] 1.1 Adicionar ícones `eye` e `eye-off` ao `AppIcon.vue` (SVG inline,
  `stroke="currentColor"`, sem sprite externo).
- [ ] 1.2 Em cada campo `type="password"`, adicionar um `ref` de visibilidade separado e
  um botão ícone que alterna `type="password"`/`type="text"` no clique.
- [ ] 1.3 Manter `data-testid`, labels e classes existentes; apenas acrescentar o botão
  de toggle ao lado do input.

## 2. Campos de senha

- [ ] 2.1 `SigninView.vue`: toggle no campo de senha do login (`id="password"`).
- [ ] 2.2 `SetupView.vue`: toggle em nova senha, confirmação e token do Plex.
- [ ] 2.3 `ChangePasswordView.vue`: toggle em senha atual, nova senha e confirmação.
- [ ] 2.4 `PasswordModal.vue`: toggle em senha atual, nova senha e confirmação.
- [ ] 2.5 `SettingsView.vue`: toggle no token do Plex.
