# Design: Mover tema e logout para a sidebar

## Visão geral

A sidebar (`SidebarArea.vue`) passa a ser o único local de controles de navegação,
tema e logout. O header (`HeaderArea.vue`) fica contendo somente o título da
página.

### Mudanças de layout

- **Header**: removido do shell (`DefaultLayout.vue` deixa de renderizar
  `HeaderArea`); o arquivo `HeaderArea.vue` é deletado.
- **Sidebar topo**: os controles ficam empilhados em coluna: (1) toggle de modo
  compacto (ícone hambúrguer), (2) alternador de tema logo abaixo.
- **Sidebar base**: botão de logout.
- **SidebarItem**: quando `isCollapsed=true`, esconde o rótulo e usa
  `aria-label`/`title` no `<router-link>`. Quando a rota é a do item, aplica
  `bg-primary text-white`; caso contrário, apenas hover.

### SidebarStore

Adicionar estado `isCollapsed` em `stores/sidebar.js`, persistido via
`localStorage.getItem('sidebar.collapsed')` e `localStorage.setItem`.

### Comportamento de modo compacto

- Toggle é um botão circular no topo da sidebar, usando o ícone <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-6">
  <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 5.25h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5" />
</svg>.
- Quando ativado: sidebar width `w-16` (instead of `w-64`), labels hidden via
  `hidden` CSS class, icons receive `aria-label` from label text for accessibility.
- Quando desativado: width volta `w-64`, labels visíveis.

### CSS adjustments (Tailwind)

- Add `sidebar-collapsed` class variant or utility:
  `w-16`, `pr-2` (icone somente), labels `.hidden`.
- Botões de tema e logout no topo/rodapé da sidebar usam `flex-1` ou apropriado.

### Acessibilidade

- Controles icon-only recebem `aria-label`/`title` correspondente.
- O toggle de tema mantém `title` dinâmico: `Tema claro`/`Tema escuro`.
- O botão logout mantém `title` "Sair".
- Focus visível em links e botões:
  `focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary
  focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900`.