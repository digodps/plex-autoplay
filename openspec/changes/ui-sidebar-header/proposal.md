# Change Proposal — ui-sidebar-header

## Why

O menu lateral atual exibe controles que não pertencem à navegação (botão
"Menu", toggle de tema) e um item estático "Bibliotecas". O usuário quer as
bibliotecas reais do Plex diretamente no menu, logo abaixo do header, com
espaçamento entre elas.

## What Changes

- Sidebar passa a listar as bibliotecas carregadas de `GET /api/plex/libraries`.
- Remove da sidebar o botão "Menu", o toggle de tema e o item estático
  "Bibliotecas".
- Sidebar renderiza abaixo do header (não ocupa a altura total ao lado dele).
- Espaçamento vertical entre itens da lista.
- Modo compacto (só ícones) mantido, controlado pelo hamburger do header e
  persistido em `localStorage`.
- Breadcrumb do header passa a compor o fluxo real da navegação, com apenas
  `Início` fixo e o restante dinâmico: `Início > <título da biblioteca>` e
  `Início > <biblioteca> > <série>`.
- Breadcrumb duplicado dentro das views de séries/marcadores é removido; o
  header é o único dono da trilha.
- Reconcilia a spec `navegacao-lateral` com o comportamento implementado.

## Scope

- `vue/plex-frontend/src/components/Sidebar/SidebarArea.vue`
- `vue/plex-frontend/src/components/AppHeader.vue`
- `vue/plex-frontend/src/components/layouts/DefaultLayout.vue`
- `vue/plex-frontend/src/lib/breadcrumb.js` (novo)
- `vue/plex-frontend/src/views/Series/SeriesView.vue` e
  `views/Markers/MarkerEditorView.vue` (remover trilha duplicada)
- `vue/plex-frontend/src/stores/sidebar.js` (somente se necessário)
- `vue/plex-frontend/tailwind.config.cjs` (restaurar `gray-2`)
- `vue/plex-frontend/tests/*.mjs`
- `openspec/changes/ui-sidebar-header/**`

## Out of Scope

- Backend e integração Plex (endpoint de bibliotecas já existe).
- Troca de senha no SettingsModal.
- Repaginação global do tema.

## Acceptance Criteria

- Sidebar não contém "Menu", "Light"/toggle de tema nem item estático
  "Bibliotecas".
- Sidebar lista todas as bibliotecas retornadas por `/api/plex/libraries`.
- Cada item navega para `/libraries/<key>/series`.
- Sidebar começa abaixo do header.
- Itens têm espaçamento vertical entre si.
- Breadcrumb no header mostra `Início > <título da biblioteca>` na rota de
  séries e `Início > <biblioteca> > <série>` nos marcadores, sem trilha
  duplicada dentro do conteúdo.
- `npm run build`, `npm run lint`, `npm run test:sidebar`,
  `npm run test:breadcrumb` e `npm run test:tokens` passam.
- `openspec validate ui-sidebar-header --strict` passa.
