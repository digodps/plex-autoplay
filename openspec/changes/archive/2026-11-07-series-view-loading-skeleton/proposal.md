# Proposal

## Why

Ao abrir uma biblioteca, a trilha de navegação mostra primeiro um título
provisório (`Biblioteca 1`, derivado da key da rota) e depois troca pelo nome
real vindo da API. O usuário vê a tela montar em duas etapas, com texto
errado piscando. Além disso, rotas de séries/marcadores (`/series/:seriesKey/markers`)
não destacam nenhum item do menu lateral.

## What Changes

- Enquanto o título da biblioteca não chega, exibir um skeleton no lugar do
  título da trilha, em vez do fallback `Biblioteca <key>`.
- Só renderizar o conteúdo da biblioteca (trilha + grid) quando o título
  estiver resolvido; até lá, manter o skeleton de carregamento.
- Se a API de bibliotecas falhar, mostrar o nome real indisponível como estado
  de erro/indisponível — nunca a key crua como título.
- Destacar o item "Bibliotecas" do menu nas rotas filhas
  `/libraries/:key/series` e `/series/:seriesKey/markers`.

## Capabilities

### New Capabilities

- Nenhuma.

### Modified Capabilities

- `series-full-page-view`: carregamento da página de séries passa a usar
  skeleton para o título e só exibe conteúdo quando o título resolve.
- `navegacao-lateral`: item ativo considera rotas filhas mapeadas.

## Impact

- `vue/plex-frontend/src/views/Series/SeriesView.vue`
- `vue/plex-frontend/src/components/Breadcrumbs/BreadcrumbDefault.vue`
- `vue/plex-frontend/src/components/Sidebar/SidebarArea.vue`
- `vue/plex-frontend/src/components/Sidebar/SidebarItem.vue`
- Sem mudanças de backend.
