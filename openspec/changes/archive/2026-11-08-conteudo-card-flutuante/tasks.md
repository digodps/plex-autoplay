# Tasks

## 1. Shell flutuante

- [x] 1.1 Reescrever `vue/plex-frontend/src/components/layouts/DefaultLayout.vue`: shell `flex h-dvh flex-col overflow-hidden bg-gray-50 dark:bg-gray-900`, header fora do scroller e `<main class="min-h-0 flex-1 p-2 md:p-3">` com card `h-full overflow-x-hidden overflow-y-auto rounded-2xl bg-white shadow-default no-scrollbar dark:bg-gray-800`.
- [x] 1.2 Adicionar `ref` no card scroller e `watch(() => route.fullPath, () => scroller.scrollTop = 0)`.
- [x] 1.3 Em `vue/plex-frontend/src/components/AppHeader.vue`, trocar `sticky top-0 ... border-b ... bg-white dark:bg-gray-900` por `relative z-30 ... shrink-0` transparente, sem borda inferior.
- [x] 1.4 Remover superfície própria das views `LibrariesView`, `SeriesView`, `MarkerEditorView`, `SettingsView`, `SyncView`: excluir `bg-gray-50 dark:bg-gray-900`, `min-h-screen`, `sticky top-0`, `border-b` e fundo dos headers internos; adicionar `no-scrollbar overscroll-contain` ao log `max-h-550` da `SyncView`.
- [x] 1.5 Em `vue/plex-frontend/src/styles/style.css`, trocar `body` de `bg-white` para `bg-gray-50` no tema claro.

## 2. Verificação

- [x] 2.1 Executar `npm --prefix vue/plex-frontend run lint`.
- [x] 2.2 Executar `npm --prefix vue/plex-frontend run build`.
- [x] 2.3 Executar `openspec validate conteudo-card-flutuante --strict`.
- [x] 2.4 QA Playwright em `http://localhost:<port>`: (a) tema claro/escuro aplicado e cor computada do card diferente do body; (b) card com `border-radius > 0`, margem > 0 e `scrollWidth - clientWidth === 0`; (c) header visível com `scrollTop` do card > 0 e `window.scrollY === 0`; (d) menu do usuário abre, navega para `/settings`, fecha e mantém estado ativo; (e) troca de rota volta o scroll do card ao topo.
