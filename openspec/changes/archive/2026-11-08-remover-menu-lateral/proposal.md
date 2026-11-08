# Proposal

## Why

O shell autenticado ainda renderiza um menu lateral (`SidebarArea`) com lista de bibliotecas, botão de recolher no header, store de preferência e testes dedicados. O usuário pediu a remoção do menu lateral: a navegação deve ficar na top bar, via breadcrumb, sem rail, dock ou lista lateral.

## What Changes

- Remover `SidebarArea`, `SidebarItem`, `stores/sidebar.js`, `lib/navigation.js` e o botão de recolher do header.
- Deixar `DefaultLayout` com top bar (`AppHeader`) + `<main>`, sem coluna lateral.
- Completar o breadcrumb para navegação: `Início → Bibliotecas → <biblioteca> → <série>`, com ancestrais clicáveis.
- Remover testes e scripts npm que fixam o comportamento do menu lateral.
- Preservar theme toggle, menu do usuário, logout e todos os fluxos existentes.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `navegacao-lateral`: remove todos os requisitos de menu/rail lateral.
- `web-ui`: remove requisitos de menu lateral e adiciona navegação por breadcrumb na top bar.

## Impact

**Frontend (`vue/plex-frontend`):**
- `src/components/layouts/DefaultLayout.vue`, `src/components/AppHeader.vue`, `src/lib/breadcrumb.js`.
- Remover `src/components/Sidebar/`, `src/stores/sidebar.js`, `src/lib/navigation.js`.
- `src/components/AppIcon.vue` permanece (ainda usado por outras telas).

**Tests:**
- Remover `tests/sidebar-collapsed.test.mjs` e `tests/navigation.test.mjs`.
- Atualizar `tests/breadcrumb.test.mjs`.
- Remover script `test:sidebar` de `package.json`.

**Backend:** no changes.
