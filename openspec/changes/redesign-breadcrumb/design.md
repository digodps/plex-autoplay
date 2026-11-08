# Design

## Context

`buildBreadcrumb(route, libraries, { loading })` é pura e alimenta
`AppHeader.vue`. Hoje ela omite `Bibliotecas`, e o header usa `/` como separador.

## Decision

Manter a função pura e corrigir a composição da trilha. O header continua sendo o
único renderizador. A especificação em `specs/web-ui/spec.md` é a fonte dos
cenários.

| Rota | Trilha |
|---|---|
| `/dashboard` | `Início` |
| `/libraries` | `Início > Bibliotecas` |
| `/libraries/:key/series` | `Início > Bibliotecas > <biblioteca>` |
| `/series/:seriesKey/markers` | `Início > Bibliotecas > <biblioteca> > <série>` |
| demais | `Início > <meta.title>` |

- `Início` aponta para `dashboard`.
- `Bibliotecas` aponta para `libraries` e não é atual nas rotas filhas.
- Biblioteca aponta para `series` da própria chave quando existe destino.
- Último item recebe `isCurrent: true` e `aria-current="page"`.
- Todos os itens têm o mesmo shape `{ label, to?, isCurrent }`.
- Itens sem `label` são filtrados; se sobrar apenas `Início`, ele vira o item
  atual.
- Enquanto a biblioteca carrega, a trilha fica `Início > Bibliotecas`.
- Biblioteca desconhecida após o carregamento usa `Biblioteca indisponível`.
- Deep link de marcadores sem `libraryKey`/`libraryTitle` omite o nível da
  biblioteca e usa `Série` como fallback do título atual.

## Separador

Em `AppHeader.vue`, trocar o `<span>/</span>` por
`<AppIcon name="chevron-right" size="sm" />`, decorativo, renderizado somente
entre itens. A lista usa `overflow-x-auto` e `whitespace-nowrap` para preservar
todos os níveis em telas estreitas.

## Testes

Atualizar `tests/breadcrumb.test.mjs` para exigir o shape completo
(`label`, `to`, `isCurrent`) em todas as rotas, incluindo fallbacks de loading,
biblioteca desconhecida e deep link de marcadores.

## Non-Goals

- Novo componente de breadcrumb; `AppHeader.vue` mantém a trilha inline.
- Mapa genérico de rotas; a cadeia atual cobre as cinco rotas do app.
- Mudança de rotas, backend ou layout do header.
