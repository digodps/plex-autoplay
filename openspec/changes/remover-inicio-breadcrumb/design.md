# Design

## Context

`buildBreadcrumb(route, libraries, { loading })` é pura e alimenta
`AppHeader.vue`. Hoje ela semeia a trilha com `HOME` (`Início` →
`dashboard`), e o header usa um divisor vertical entre o logo e a trilha.

## Decision

Remover a constante `HOME` e deixar a trilha começar em `Bibliotecas`. O logo
(`RouterLink` para `/`, que redireciona para `dashboard`) é o único acesso à
home no cabeçalho, então a duplicação não é necessária.

| Rota | Trilha |
|---|---|
| `/dashboard` | *(vazia)* |
| `/libraries` | `Bibliotecas` |
| `/libraries/:key/series` | `Bibliotecas > <biblioteca>` |
| `/series/:seriesKey/markers` | `Bibliotecas > [<biblioteca> >] <série>` |
| demais | `<meta.title>` |

O nível `<biblioteca>` dos marcadores só aparece quando `libraryKey` ou
`libraryTitle` existe; sem metadados a trilha é `Bibliotecas > Série`.

- `Bibliotecas` aponta para `libraries` e não é atual nas rotas filhas.
- Biblioteca aponta para `series` da própria chave quando existe destino.
- Último item recebe `isCurrent: true` e `aria-current="page"`.
- Itens sem `label` continuam filtrados; lista vazia é válida.
- Enquanto bibliotecas carregam, trilha fica `Bibliotecas`.
- Biblioteca desconhecida após carregamento usa `Biblioteca indisponível`.
- `dashboard` retorna `[]` por caminho explícito, não por efeito dos filtros.
- Sem `HOME`, as demais rotas usam apenas `meta.title` (ou `Página`).

## Header

Em `AppHeader.vue`:

- O divisor vertical entre logo e trilha é removido.
- Cada item passa a renderizar o `chevron-right` (*antes* do rótulo), sem
  seta depois do último. Com isso há uma única regra de separador e o
  resultado visual é `Plex Autoplay > Bibliotecas > Animes`. O chevron é
  decorativo (`aria-hidden`); o leitor de tela lê o logo e a lista.
- `<nav aria-label="Breadcrumb">` só existe com `v-if="items.length"`, para
  não anunciar um landmark vazio em `dashboard`.
- A chave do `v-for` passa a ser o índice: o rótulo pode repetir (biblioteca
  chamada `Bibliotecas`).

O logo mantém `aria-label="Plex Autoplay — início"`.

## Contrato de acessibilidade

A delta de `series-full-page-view` troca o antigo link "Voltar às
bibliotecas" pela trilha com link "Bibliotecas" e `aria-current="page"`, como
já praticado pelo cabeçalho atual. A garantia de nome acessível permanece.

## Testes

`vue/plex-frontend/tests/breadcrumb.test.mjs` passa a esperar trilhas sem
`Início`, a cobrir `dashboard` vazio e a separar o invariante "se a trilha não
é vazia, exatamente um `isCurrent`, no fim". Tema e menu continuam validados
por Playwright no QA, que também confere o breadcrumb renderizado.
