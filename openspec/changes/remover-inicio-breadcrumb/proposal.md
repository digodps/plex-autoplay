# Proposal

## Why

O cabeçalho já tem o logo `Plex Autoplay` como link para a home
(`/`, redirecionada para `dashboard`). O primeiro nível `Início` da trilha
aponta para a mesma rota, duplicando a ação e gastando espaço horizontal em
telas estreitas.

## What Changes

- Remover o item `Início` de `buildBreadcrumb`; a trilha passa a começar em
  `Bibliotecas`.
- Remover o divisor vertical entre o logo e a trilha; cada item passa a ser
  precedido pelo ícone `chevron-right`, mantendo a leitura
  `Plex Autoplay > Bibliotecas > Animes`.
- Em `dashboard` a trilha fica vazia e o landmark `<nav>` não é renderizado.
- Manter o logo clicável para a home, com texto acessível.
- O contrato de acessibilidade de `series-full-page-view` deixa de citar o
  link "Voltar às bibliotecas": vale o link "Bibliotecas" com
  `aria-current="page"` já usado no cabeçalho.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `web-ui`: a trilha deixa de renderizar `Início`; o logo cumpre o papel de
  acesso à home no cabeçalho.
- `series-full-page-view`: o breadcrumb da página de séries passa a ser
  `Bibliotecas > <biblioteca>`.

## Impact

- `vue/plex-frontend/src/lib/breadcrumb.js`
- `vue/plex-frontend/src/components/AppHeader.vue`
- `vue/plex-frontend/tests/breadcrumb.test.mjs`
- `openspec/changes/remover-inicio-breadcrumb/**`

Sem mudanças de backend, rotas ou APIs.

## Acceptance Criteria

- `/dashboard` não exibe trilha nem seta; o logo continua levando à home.
- `/libraries` mostra `Bibliotecas`.
- `/libraries/1/series` com biblioteca `Animes` mostra `Bibliotecas > Animes`.
- `/series/42/markers?...` mostra `Bibliotecas > Animes > Aosh`.
- Leitura visual do cabeçalho: `Plex Autoplay > Bibliotecas > Animes`.
- `Bibliotecas` continua clicável nas rotas filhas; item atual com
  `aria-current="page"`.
