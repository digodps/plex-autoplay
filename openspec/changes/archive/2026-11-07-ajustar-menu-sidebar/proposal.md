# Proposal: ajustar-menu-sidebar

## Why

O menu lateral exibe o caminho da rota (ex.: `/dashboard`) em vez de um rótulo legível e os ícones não têm espaçamento consistente em relação ao texto.

## What Changes

- Adicionar rótulos em português aos itens do menu lateral.
- Permitir que `SidebarItem` receba um `label` opcional, mantendo fallback para `to`.
- Aplicar espaçamento ao ícone (`m-1`) para separar ícone e texto.

## Escopo

- `vue/plex-frontend/src/components/Sidebar/SidebarArea.vue`
- `vue/plex-frontend/src/components/Sidebar/SidebarItem.vue`

## Fora de escopo

- Alterar rotas, layout geral, tema ou comportamento de navegação.
- Redesign do frontend.
