# Tasks

## 1. Remoção do menu lateral

- [ ] 1.1 Remover `SidebarArea`, `useSidebarStore`, `route`/`error` não usados de `vue/plex-frontend/src/components/layouts/DefaultLayout.vue`; manter `AppHeader` + `<main>` e `fetchLibraries` para os títulos do breadcrumb.
- [ ] 1.2 Remover botão de expandir/recolher menu e imports de sidebar de `vue/plex-frontend/src/components/AppHeader.vue`.
- [ ] 1.3 Excluir `vue/plex-frontend/src/components/Sidebar/`, `vue/plex-frontend/src/stores/sidebar.js` e `vue/plex-frontend/src/lib/navigation.js`; remover entradas de ícone `library` e `menu` de `AppIcon.vue` se ficarem sem uso.
- [ ] 1.4 Ajustar `vue/plex-frontend/src/lib/breadcrumb.js`: `Bibliotecas` sempre como ancestral clicável em séries/marcadores; `/libraries/:key/series` termina na biblioteca atual; marcadores fazem `Início → Bibliotecas → <biblioteca> → <série>`, com biblioteca linkável e fallback genérico quando faltar chave/título.
- [ ] 1.5 Remover `tests/sidebar-collapsed.test.mjs`, `tests/navigation.test.mjs`, script `test:sidebar`; atualizar `tests/breadcrumb.test.mjs` para as novas trilhas (incluindo rota `libraries`, fallback sem chave e loading); adicionar `tests/no-sidebar.test.mjs` que falha se `DefaultLayout.vue`/`AppHeader.vue` voltarem a referenciar `SidebarArea`, `useSidebarStore` ou `<aside`.

## 2. Verificação

- [ ] 2.1 Executar `npm run lint`, `npm run build`, `npm run test:breadcrumb`, `npm run test:ui` e `npm run test:tokens` sem erros.
- [ ] 2.2 Executar `openspec validate remover-menu-lateral --strict`.
- [ ] 2.3 QA independente com Playwright: validar tema light/dark e menus (abrir, navegar, fechar, estado ativo) em `http://localhost:<port>`, além de confirmar ausência do menu lateral.
