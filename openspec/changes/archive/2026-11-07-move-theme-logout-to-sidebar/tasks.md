# Tasks

- [x] 1. Remover botão de tema do HeaderArea.vue e imports/funções órfãs.
- [x] 2. Remover botão de logout do HeaderArea.vue.
- [x] 3. Adicionar alternador de tema na sidebar.
- [x] 4. Adicionar botão logout na base da SidebarArea.vue.
- [x] 5. Adicionar estado `isCollapsed` ao sidebar store (`stores/sidebar.js`),
      persistido em `localStorage`; remover membros mortos.
- [x] 6. Adicionar toggle de modo compacto (ícone `menu` no AppIcon).
- [x] 7. SidebarItem: esconder rótulo quando `collapsed=true` e expor `aria-label`
      no `<router-link>`.
- [x] 8. Sidebar `w-64` normal / `w-16` colapsada.
- [x] 9. Verificação executável da persistência: `npm run test:sidebar`.
- [x] 10. `npm run lint` e `npm run build` sem erros.
- [x] 11. `openspec validate move-theme-logout-to-sidebar --strict` passa.
- [x] 12. Empilhar tema abaixo do menu em SidebarArea.vue (coluna, rótulo visível).
- [x] 13. Remover `<HeaderArea />` de DefaultLayout.vue e deletar
      `components/Header/HeaderArea.vue`; remover estado morto
      `isSidebarOpen`/`toggleSidebar`/`closeSidebar`.
- [x] 14. Destaque do item ativo (`bg-primary` + `text-white`) no
      `<router-link>`, incluindo modo compacto e rotas aninhadas (prefixo).
- [x] 15. Foco visível nos links e botões da sidebar.
- [x] 16. Reexecutar `npm run lint`, `npm run build`, `npm run test:sidebar` e
      `openspec validate --strict`.
