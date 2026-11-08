# Proposal

## Why

O header hoje concentra ações que pertencem à navegação (tema e sair), gastando
largura em toda rota e deixando o menu lateral sem identidade própria. Mover
essas ações para a sidebar e remover a barra de título libera espaço vertical e
horizontal, permite oferecer um modo compacto (somente ícones) e deixa claro
qual item do menu está ativo.

## What Changes

- Remover o botão de tema e o botão "Sair" do header.
- Adicionar o alternador de tema no topo da sidebar (mesma lógica atual:
  claro/escuro, persistida no store de tema), empilhado logo abaixo do controle
  de menu.
- Adicionar o botão "Sair" na base da sidebar (mesma ação atual: logout +
  redirecionar para `login`).
- Adicionar controle de modo compacto na sidebar, usando o ícone de três linhas
  (hambúrguer) informado; quando ativo, a sidebar mostra somente ícones e
  reduz a largura; quando inativo, volta ao layout com rótulos.
- Persistir a preferência de modo compacto em `localStorage` e restaurá-la no
  carregamento.
- Remover a barra de título (header) do shell; o conteúdo passa a ocupar o topo.
- Destacar o item de menu da rota atual com fundo e cor de texto contrastantes,
  visíveis nos temas claro e escuro.
- Manter acessibilidade: controles icon-only com `aria-label`/`title`, item de
  navegação com `aria-current="page"` já existente preservado.

## Capabilities

### New Capabilities

- Nenhuma.

### Modified Capabilities

- `navegacao-lateral`: a sidebar passa a ser dona do tema, do logout e do modo
  compacto; o header deixa de existir no shell; o item ativo ganha destaque
  visual.

## Impact

- `vue/plex-frontend/src/components/Header/HeaderArea.vue` — removido do shell
  e deletado.
- `vue/plex-frontend/src/components/layouts/DefaultLayout.vue` — sem header.
- `vue/plex-frontend/src/components/Sidebar/SidebarArea.vue` — tema, logout e
  controle de modo compacto empilhados.
- `vue/plex-frontend/src/components/Sidebar/SidebarItem.vue` — esconde rótulo
  no modo compacto, expõe `aria-label` e destaca o item ativo.
- `vue/plex-frontend/src/stores/sidebar.js` — estado `isCollapsed` persistido.
- `vue/plex-frontend/src/components/AppIcon.vue` — ícone `menu`.
- Sem mudanças de backend.
