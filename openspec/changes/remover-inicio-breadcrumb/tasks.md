# Tasks

## 1. Trilha de navegação

- [x] 1.1 Remover a constante `HOME` de `vue/plex-frontend/src/lib/breadcrumb.js`, retornar `[]` explicitamente para `dashboard` e deixar a trilha começar em `Bibliotecas`.
- [x] 1.2 Atualizar `vue/plex-frontend/tests/breadcrumb.test.mjs`: remover fixture `home`, esperar `dashboard` vazio, cobrir marcadores sem metadados com bibliotecas ausentes e manter o invariante só para trilhas não vazias.

## 2. Cabeçalho

- [x] 2.1 Em `vue/plex-frontend/src/components/AppHeader.vue`, remover o divisor vertical, renderizar `chevron-right` antes de cada item, aplicar `v-if="items.length"` no `<nav>` e chavear o `v-for` por índice.

## 3. Verificação

- [x] 3.1 Executar `npm --prefix vue/plex-frontend run test:breadcrumb`.
- [x] 3.2 Executar `npm --prefix vue/plex-frontend run lint` e `npm --prefix vue/plex-frontend run build`.
- [x] 3.3 Executar `openspec validate remover-inicio-breadcrumb --strict`.
- [x] 3.4 QA Playwright em `http://localhost:8092`: breadcrumb (`Plex Autoplay > Bibliotecas > Animes`, `aria-current="page"`, dashboard sem seta), tema light/dark e menus (abrir, navegar, fechar, item ativo).
