# Design

## Context

Shell autenticado atual: `DefaultLayout` monta `AppHeader` + `SidebarArea` + `<main>`. `AppHeader` expõe botão de recolher ligado a `stores/sidebar.js`. O breadcrumb é calculado em `lib/breadcrumb.js` a partir da rota e da lista de bibliotecas.

## Decisions

### 1. Shell sem coluna lateral

`DefaultLayout` renderiza apenas:

```
<div flex h-screen flex-col>
  <AppHeader :items="breadcrumbs" />
  <main flex-1 overflow-y-auto>...</main>
</div>
```

Remove `SidebarArea`, `useSidebarStore` e `isCollapsed`. Mantém `fetchLibraries` no layout, pois o breadcrumb precisa dos títulos reais.

### 2. Header sem controle de menu

`AppHeader` mantém marca, breadcrumb, toggle de tema e menu do usuário (configurações, alterar senha, sair). Remove o botão `Expandir/Recolher menu` e o separador associado.

### 3. Breadcrumb é a navegação

Trilhas:

- `/dashboard`: `Início`
- `/libraries`: `Início → Bibliotecas`
- `/libraries/:key/series`: `Início → Bibliotecas → <biblioteca>`
- `/series/:seriesKey/markers`: `Início → Bibliotecas → <biblioteca> → <série>`
- demais rotas: `Início → <meta.title>`

`Bibliotecas` é sempre um ancestral clicável (`{ name: "libraries" }`) em rotas de séries e marcadores, inclusive durante o loading. Em `/libraries/:key/series`, a biblioteca é o item atual (sem link). Em `/series/:seriesKey/markers`, a biblioteca linka para `{ name: "series", params: { key } }` e a série é o item atual. Sem `key`, a trilha mantém `Bibliotecas` clicável e usa rótulos genéricos (`Biblioteca`, `Série`) — nunca remove o ancestral. Somente o último item tem `isCurrent`; `AppHeader` continua renderizando link apenas quando `to` existe e o item não é atual.

### 4. Remoção de código morto

Apagar `components/Sidebar/SidebarArea.vue`, `components/Sidebar/SidebarItem.vue`, `stores/sidebar.js`, `lib/navigation.js` e os testes que os exercitam. Remover o script `test:sidebar`.

## Risks / Trade-offs

- Sem sidebar, o acesso às bibliotecas passa pelo card do Dashboard ou breadcrumb. O card de bibliotecas já aponta para `/libraries`.
- Breadcrumb depende do carregamento de bibliotecas; fallback mantém `Bibliotecas` clicável e rótulos genéricos até carregar. Falha de `fetchLibraries` no layout é silenciosa nesse ponto; `LibrariesView` continua exibindo o erro da sua própria chamada.

## Migration Plan

1. EGM A/S/C nos artefatos.
2. Commit dos artefatos e worktree `feature/spec-remover-menu-lateral`.
3. Remover componentes/store/helper e ajustar layout/header/breadcrumb.
4. Atualizar testes e scripts.
5. Build gate; QA independente com Playwright (tema e menus).
