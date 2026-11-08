# Design: skeleton do título e item ativo em rotas filhas

## Estado de carregamento do título

`SeriesView.vue` passa a ter `libraryTitleLoading` (true até a primeira resposta
de `/api/plex/libraries`, ou até falhar):

- `libraryTitle` = título real da biblioteca ou `""`.
- `libraryTitleLoading` = `true` enquanto a requisição está em voo.
- Fallback `Biblioteca ${key}` é removido. Em falha, o título vira
  `"Biblioteca indisponível"`.

A trilha recebe um item com `loading: true` e `BreadcrumbDefault` renderiza
`<span class="... animate-pulse ...">` com largura fixa em vez do texto.
Assim não existe transição de texto errado para texto certo.

## Conteúdo só depois do carregamento

O grid de séries já usa `seriesLoading` + skeleton. A condição de exibição
passa a exigir `!libraryTitleLoading`, de modo que o grid só aparece depois do
título resolvido. Erro de título não bloqueia o catálogo: após a falha,
`libraryTitleLoading` vira `false` e o grid segue o fluxo normal.

## Item ativo em rotas filhas

`SidebarArea.vue` declara `match` por item:

- `Bibliotecas` → `match: ["/libraries", "/series"]`
- demais itens → sem `match` (usa apenas `to`)

`SidebarItem.vue` recebe `match: Array` (default `[]`) e considera ativo quando
`route.path` é igual a `to` ou começa com `to/` ou com qualquer prefixo de
`match` (com fronteira `/`). Mantém `aria-current="page"` no link ativo.

## Acessibilidade

- Skeleton com `aria-hidden="true"`; o `<nav>` mantém `aria-label`.
- Nenhuma mudança nos nomes acessíveis dos controles.
