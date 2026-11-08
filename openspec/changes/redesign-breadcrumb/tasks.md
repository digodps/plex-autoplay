# Tasks

## 1. Trilha completa

- [ ] 1.1 Atualizar `vue/plex-frontend/src/lib/breadcrumb.js` para incluir
  `Bibliotecas` em `/libraries/:key/series` e
  `/series/:seriesKey/markers`, manter shape `{ label, to, isCurrent }` e os
  fallbacks de loading, biblioteca desconhecida e deep link de marcadores.
- [ ] 1.2 Atualizar `vue/plex-frontend/tests/breadcrumb.test.mjs` com as trilhas
  completas e asserções de `label`, `to` e `isCurrent` em todas as rotas.

## 2. Separador por seta

- [ ] 2.1 Em `vue/plex-frontend/src/components/AppHeader.vue`, trocar o
  separador `/` por `AppIcon` `chevron-right` decorativo, exibido apenas entre
  itens, e permitir rolagem horizontal da trilha.

## 3. Verificação

- [ ] 3.1 Executar `npm --prefix vue/plex-frontend run test:breadcrumb`.
- [ ] 3.2 Executar `npm --prefix vue/plex-frontend run lint` e
  `npm --prefix vue/plex-frontend run build`.
- [ ] 3.3 Executar `openspec validate redesign-breadcrumb --strict`.
