# UI — Sidebar & Header Design

## Layout

```
+--------------------------------------------------------------+
| Header: hamburger | breadcrumb        tema | perfil | ajustes |
+-------------------+------------------------------------------+
| Sidebar           |                                          |
| (bibliotecas)     |  Main content                            |
| • Biblioteca A    |                                          |
| • Biblioteca B    |                                          |
+-------------------+------------------------------------------+
```

- Header ocupa a largura total e fica no topo (`sticky top-0`).
- Sidebar fica abaixo do header, com altura `calc(100vh - header)` e scroll
  próprio.
- Conteúdo principal rola de forma independente.

## Componentes reais

| Componente | Arquivo | Responsabilidade |
|---|---|---|
| Header | `components/AppHeader.vue` | Hamburger (colapsa sidebar), breadcrumb, toggle de tema, perfil/logout, abrir ajustes |
| Sidebar | `components/Sidebar/SidebarArea.vue` | Lista de bibliotecas (somente apresentação) |
| Layout | `components/layouts/DefaultLayout.vue` | Carrega bibliotecas, deriva o breadcrumb, compõe header + sidebar + main |
| Breadcrumb | `lib/breadcrumb.js` | Função pura `buildBreadcrumb(route, libraries)` |
| Store | `stores/sidebar.js` | `isCollapsed` + persistência |

O componente `Sidebar/SidebarItem.vue` existente pode ser usado se já atender ao
item; caso contrário, o item é renderizado inline.

## Sidebar

- Props: `items` (array `{ key, title }`), `collapsed` (boolean).
- Sem imports de `vue-router`, stores, composables ou `api.js`.
- Larguras: `w-64` expandida, `w-16` compacta.
- Lista: `<ul>` com `space-y-1` (margem entre bibliotecas) e `p-2`.
- Item: `RouterLink` para `/libraries/<key>/series`, ícone `library`, rótulo com
  o título da biblioteca, alvo mínimo de 44px, `aria-current` quando ativo.
- Compacta: rótulo oculto, ícone permanece, `title`/`aria-label` com o título.
- Rota ativa: `/libraries/<key>/series` ou `/series/...` da mesma biblioteca
  quando o `key` estiver disponível na rota; caso contrário, comparação por
  prefixo `/libraries/<key>`.
- Carregamento: `fetchLibraries()` em `api.js` (já existe).
- Estado de erro: mensagem curta em `role="alert"` dentro da sidebar.

## Breadcrumb

- `buildBreadcrumb(route, libraries)` é pura, sem fetch nem DOM.
- O layout deriva os itens da rota + bibliotecas já carregadas e passa
  `breadcrumbs` ao header; header apenas renderiza.
- Tabela de trilhas:
  - `/dashboard` → `Início`
  - `/libraries` → `Início > Bibliotecas`
  - `/libraries/:key/series` → `Início > <título da biblioteca>`
  - `/series/:seriesKey/markers` → `Início > <biblioteca> > <série>`
  - demais rotas → `Início > <meta.title>`
- Apenas `Início` é fixo. Os demais níveis vêm do estado/rota (biblioteca e
  série) e nunca de rótulo estático de seção.
- `Início` sempre link para `/dashboard`; níveis intermediários dinâmicos são
  links quando há destino; último item é o atual (`aria-current="page"`).
- Título da biblioteca: `libraries` do layout → `route.query.libraryTitle` →
  `Biblioteca indisponível`.
- Separação visual por `/` apenas entre itens; item vazio é omitido.
- Trilha duplicada é removida de `SeriesView` e `MarkerEditorView`.

## Paleta

Mantidas as variáveis de `styles/style.css`:

| Token | Light | Dark |
|---|---|---|
| `--surface` | `#FFFFFF` | `#0D1117` |
| `--border` | `#D0D7DE` | `#30363D` |
| `--text` | `#24292F` | `#E6EDF3` |
| `--text-secondary` | `#57606A` | `#8B949E` |
| `--accent` | `#0969DA` | `#58A6FF` |

Header e sidebar usam `bg-white dark:bg-gray-900`; `gray-900` deve ser o
`#18181B` atual do config. O valor `#353B45` não é usado como superfície nesta
change.

## Tailwind

- Restaurar `gray-2` em `theme.extend.colors` para os skeletons existentes.
- Nenhuma dependência nova.

## Acessibilidade

- Hamburger com `aria-expanded` e `aria-label` refletindo o estado.
- Items with target ≥44px, visible focus (`focus-visible:ring-2`), `aria-current`.
- `motion-reduce` desativa transições.
