# Proposal

## Why

A migalha de pão atual perde níveis da rota (`Início > Animes` em vez de
`Início > Bibliotecas > Animes`) e usa texto `/` como separador. O usuário quer
a trilha completa com uma seta para a direita entre os itens.

## What Changes

- Incluir `Bibliotecas` na trilha das rotas de séries e marcadores.
- Manter a hierarquia completa: `Início > Bibliotecas > <biblioteca> > <série>`.
- Substituir o separador textual `/` pelo ícone `chevron-right` já disponível em
  `AppIcon`.
- Manter ancestrais clicáveis, item atual não clicável e `aria-current="page"`.
- Permitir rolagem horizontal da trilha em telas estreitas, sem descartar níveis.

## Capabilities

### New Capabilities

- `web-ui`: trilha de navegação completa, separador visual por seta e fallback
  para deep links sem metadados.

### Modified Capabilities

- `series-full-page-view`: o breadcrumb da página de séries passa a exibir a
  trilha completa, substituindo o formato `[← Bibliotecas] <título>`.

## Impact

- `vue/plex-frontend/src/lib/breadcrumb.js`
- `vue/plex-frontend/src/components/AppHeader.vue`
- `vue/plex-frontend/tests/breadcrumb.test.mjs`
- `openspec/changes/redesign-breadcrumb/**`

Sem mudanças de backend, rotas ou APIs.

## Acceptance Criteria

- `/dashboard` mostra `Início`.
- `/libraries` mostra `Início > Bibliotecas`.
- `/libraries/:key/series` mostra `Início > Bibliotecas > <título da biblioteca>`.
- `/series/:seriesKey/markers` mostra
  `Início > Bibliotecas > <biblioteca> > <série>` quando os metadados existem.
- Deep link de marcadores sem metadados mostra `Início > Bibliotecas > Série`.
- Cada par de itens é separado por um ícone de seta para a direita.
- `Bibliotecas` navega para `/libraries`; biblioteca navega para a lista de séries.
- `npm run test:breadcrumb`, `npm run lint` e `npm run build` passam.
- `openspec validate redesign-breadcrumb --strict` passa.
