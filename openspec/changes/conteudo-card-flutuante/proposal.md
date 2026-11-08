# Proposal

## Why

O shell autenticado hoje é uma página plana: o `AppHeader` e o `<main>` compartilham o mesmo fundo e o conteúdo rola junto com a página. O pedido é que o conteúdo pareça um painel flutuante sobre o body, com cabeçalho fixo e rolagem interna, mantendo a leitura clara nos temas claro e escuro.

## What Changes

- Envolver a área de conteúdo do `DefaultLayout` em um card flutuante com cantos arredondados, margem pequena em relação à viewport e sombra sutil.
- Separar visualmente body e card por tom de superfície: no tema claro o body fica levemente acinzentado e o card branco; no tema escuro o body fica mais escuro e o card um tom acima.
- Manter `AppHeader` fixo no topo, sem borda inferior, usando a cor de superfície do body.
- Mover a rolagem para dentro do card (`overflow-y-auto`), com a barra de rolagem invisível usando o utilitário `no-scrollbar` já existente.
- Preservar tema claro/escuro, menu do usuário, breadcrumb e navegação existentes.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `web-ui`: o shell autenticado passa a renderizar o conteúdo em card flutuante com header fixo e rolagem interna sem barra visível.

## Impact

- `vue/plex-frontend/src/components/layouts/DefaultLayout.vue`
- `vue/plex-frontend/src/components/AppHeader.vue`
- `vue/plex-frontend/src/styles/style.css` (fundo do `body`)
- `vue/plex-frontend/src/views/{Libraries,Series,Markers,Pages,Sync}/*View.vue` (remover superfície própria)
- QA Playwright em `http://localhost:<port>`
- Sem mudanças de backend, API ou dependências.
