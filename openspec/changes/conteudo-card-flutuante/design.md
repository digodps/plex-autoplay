# Design

## Context

`DefaultLayout.vue` é o shell de todas as views autenticadas. Hoje ele usa:

```html
<div class="flex h-screen flex-col overflow-hidden">
  <AppHeader :items="breadcrumbs" />
  <main class="relative min-h-0 flex-1 overflow-x-hidden overflow-y-auto">
    <div class="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
      <slot />
    </div>
  </main>
</div>
```

O `AppHeader` é `sticky top-0` com `border-b`. A rolagem acontece no `<main>`, sem card e com barra visível. Cinco views também pintam `bg-gray-50 dark:bg-gray-900 min-h-screen` e têm header interno `sticky` com borda.

## Decision

Trocar a estrutura por três camadas:

1. **Shell**: `flex h-dvh flex-col overflow-hidden bg-gray-50 dark:bg-gray-900`.
2. **Header**: `relative z-30 shrink-0`, transparente, sem `sticky` e sem `border-b`. O fundo vem do shell.
3. **Card scroller**: `<main class="min-h-0 flex-1 p-2 md:p-3">` contendo um único elemento `h-full overflow-x-hidden overflow-y-auto rounded-2xl bg-white shadow-default no-scrollbar dark:bg-gray-800`.

| Tema | Body / header | Card |
|---|---|---|
| Claro | `gray-50` (`#F8F9FA`) | `white` (`#FFFFFF`) |
| Escuro | `gray-900` (`#18181B`) | `gray-800` (`#21262D`) |

O card é o próprio scroller; não há camada interna com `h-full`. Isso recorta os cantos (`rounded-2xl` + `overflow-y-auto`) e mantém a barra invisível com o utilitário `no-scrollbar` já existente em `src/styles/style.css`.

Como a rolagem deixa de ser da janela, o `scrollBehavior` do router não reposiciona a view. `DefaultLayout` guarda uma `ref` no scroller e observa `route.fullPath` para zerar `scrollTop`.

Views com fundo próprio são ajustadas: removem `bg-gray-50 dark:bg-gray-900`, `min-h-screen`, `sticky top-0`, `border-b` e o fundo dos headers internos. O log da `SyncView` ganha `no-scrollbar overscroll-contain`.

## Trade-offs

- `body` de `src/styles/style.css` passa a `bg-gray-50 dark:bg-gray-900` para o overscroll não expor branco.
- `shadow-default` substitui `shadow-card` para o card destacar no escuro.
- Sem CSS novo e sem dependências: apenas classes utilitárias Tailwind existentes.
- `rounded-2xl` no card é intencional; `--radius` continua valendo para painéis internos.

## Testes

- `npm --prefix vue/plex-frontend run lint` e `run build`.
- `openspec validate conteudo-card-flutuante --strict`.
- QA Playwright em `http://localhost:<port>` com asserts de cor computada, header fora do scroller, barra invisível e menu.
