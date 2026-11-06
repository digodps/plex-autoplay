# Tasks

## 1. Remoção de dependências e limpeza
- [ ] 1.1 Excluir `vue/plex-frontend/src/styles/components/` e `vue/plex-frontend/src/views/*.css` (SeriesView.css, marker-editor.css, sync-view.css e arquivos button.css, card.css, field.css, table.css, state.css).
- [ ] 1.2 Remover `vue/plex-frontend/package.json` a dependência `@material-tailwind/html`.
- [ ] 1.3 Excluir `vue/plex-frontend/public/icons.svg` sprite usage; excluir `vue/plex-frontend/src/components/AppIcon.vue`.
- [ ] 1.4 Instalar dependência `lucide-vue-next` e atualizar o import em todos os componentes.
- [ ] 1.5 Excluir `vue/plex-frontend/src/views/HelloWorld.vue` e referências em `vue/plex-frontend/src/router/index.js`.

## 2. Estilo global único
- [ ] 2.1 Criar/modificar `vue/plex-frontend/src/styles/tokens.css` com todos os tokens necessários (`--speed`, `--ease`, `--shadow-1`, `--shadow-2`, `--bg-sunken`, `--mono`, além dos já presentes) e pares de contraste WCAG AA para light/dark.
- [ ] 2.2 Criar/modificar `vue/plex-frontend/src/styles/app.css` com reset, layout global, primitivas, estados (loading/empty/error), transições fade/slide, responsividade breakpoint 768px, alvos ≥44px, e **nenhum** `<style>` ou CSS scoped em views/components. Verificar execução `npm run build` bem-sucedido.
- [ ] 2.3 Executar `npm run test:tokens` e confirmar que todos os blocos de contraste passam.
- [ ] 2.4 Executar `npm run lint` e confirmar ausência de erros de “scoped style” ou “per-view CSS”.

## 3. Shell e navegação (top bar)
- [ ] 3.1 Reescrever `vue/plex-frontend/src/App.vue`:
     - Substituir `AppSidebar` por barra fixa superior contendo: marca (botão para `/dashboard`), breadcrumb (`<Breadcrumb :items="..."/>`), toggle de tema e logout.
     - Envolver `<router-view>` com `<transition name="page" mode="out-in">`.
     - Aplicar tema antes do mount (`themeStore.applyTheme()`) usando script anti-FOUC existente em `index.html`.
- [ ] 3.2 Atualizar `vue/plex-frontend/src/composables/useAppShell.js` para computar a trilha de breadcrumb a partir da rota, com labels fixos e fallback genérico. Não importar `vue-router` internamente.

## 4. BreadCrumb componente
- [ ] 4.1 Manter/atualizar `vue/plex-frontend/src/components/Breadcrumb.vue` para receber `items` (`{ label, to?, current? }`), emitir evento `select(to)` para ancestrais, e renderizar o item corrente como não clicável com `aria-current="page"`. **Não** importar `vue-router` internamente.
- [ ] 4.2 Confirmar que ao visitar `/dashboard` a migalha mostra `Início`; ao visitar `/libraries/:key` mostra `Início → Bibliotecas → <título>`; ao visitar `/series/:seriesKey/markers` mostra `Início → Bibliotecas → <série> → Marcadores`.

## 5. Tema
- [ ] 5.1 Manter `vue/plex-frontend/src/stores/theme.js` com `mode` `light|dark|system`, persistência via `GET/PUT /api/theme`, aplicar `data-theme` antes do mount usando script existente.
- [ ] 5.2 Adicionar botão de toggle de tema na top bar (cicla `system → light → dark → system`).
- [ ] 5.3 Confirmar que o toggle persiste recarregando a página.

## 6. UI Store (títulos de breadcrumb)
- [ ] 6.1 Criar `vue/plex-frontend/src/stores/ui.js` com `libraryTitle`/`seriesTitle` getters e `setLibraryTitle(title)` / `setSeriesTitle(title)` actions.
- [ ] 6.2 Em `LibrariesView.vue`, após `store.fetchAll()`, chamar `ui.setLibraryTitle(lib.title)`.
- [ ] 6.3 Em `SeriesView.vue` (após `useSeriesCatalog` data carrega), chamar `ui.setSeriesTitle(series.title)`.
- [ ] 6.4 Confirmar que ao visitar `/libraries/:key` o breadcrumb mostra o título real da biblioteca.

## 7. Composáveis reutilizáveis
- [ ] 7.1 Manter/atualizar `vue/plex-frontend/src/composables/useInfiniteScroll.js` (já existente) para funcionar com o novo grid responsivo.
- [ ] 7.2 Manter/atualizar `vue/plex-frontend/src/composables/useSeriesCatalog.js` (já existente) com search + infinite scroll.
- [ ] 7.3 **Excluir** `vue/plex-frontend/src/composables/useLibrarySeries.js` (duplicado; manter apenas `useSeriesCatalog`).
- [ ] 7.4 Confirmar execução `npm run dev` sem erros nos composables restantes.

## 8. Stores de funcional
- [ ] 8.1 Manter `vue/plex-frontend/src/stores/auth.js` (já existente).
- [ ] 8.2 Manter `vue/plex-frontend/src/stores/dashboard.js` (já existente) — polling 30s, `fetchAll()`, agregações.
- [ ] 8.3 Manter `vue/plex-frontend/src/stores/markers.js` (já existente) — CRUD otimista.
- [ ] 8.4 Manter `vue/plex-frontend/src/stores/theme.js` (já existente) e confirmar persistência.
- [ ] 8.5 Confirmar execução `npm run dev` sem erros de auth.

## 8.5. Views
- [ ] 8.6 Reescrever `vue/plex-frontend/src/views/DashboardView.vue`:
     - Exibir 4 `StatusCard` (Saúde, Última Sync, Bibliotecas, Marcadores) no grid responsivo usando tokens apenas.
     - Barra de ações: botão “Iniciar Escaneamento” (POST /api/plex/scan → router.push('/sync')) e “Ver Log Completo” (`router.push('/sync')`).
     - BreadCrumb via composable.
- [ ] 8.7 Reescrever `vue/plex-frontend/src/views/LibrariesView.vue`:
     - Grid `library-grid` usando tokens apenas.
     - Título real no breadcrumb via `ui.setLibraryTitle`.
     - Navegação para `/libraries/:key/series` ao clicar em card.
- [ ] 8.8 Reescrever `vue/plex-frontend/src/views/SeriesView.vue`:
     - Grid `series-grid` usando tokens.
     - Barra de busca + infinite scroll via `useSeriesCatalog`.
     - BreadCrumb com título real da série via `ui.setSeriesTitle`.
- [ ] 8.9 Reescrever `vue/plex-frontend/src/views/MarkerEditorView.vue`:
     - Grid por temporada + episódio, CRUD intro/créditos (optimistic).
     - BreadCrumb `Início → Bibliotecas → <série> → Marcadores`.
- [ ] 8.10 Reescrever `vue/plex-frontend/src/views/SettingsView.vue`:
     - Toggle theme, interval sync, Plex URL/Token save via existing API.
- [ ] 8.11 Reescrever `vue/plex-frontend/src/views/SyncView.vue`:
     - Painel de logs SSE, botão iniciar/cancelar scan, status badge usando tokens.
- [ ] 8.12 Confirmar que cada view renderiza sem erros (`npm run build`).

## 9. Componentes compartilhados
- [ ] 9.1 Reescrever/modificar `vue/plex-frontend/src/components/StateBlock.vue` (loading skeleton / empty / error com retry) usando tokens.
- [ ] 9.2 Reescrever/modificar `vue/plex-frontend/src/components/PageHeader.vue` (eyebrow + title + subtitle + actions).
- [ ] 9.3 Confirmar que todos os views usam esses componentes.

## 10. MarkerRow (presentational)
- [ ] 10.1 Refatorar `vue/plex-frontend/src/components/MarkerRow.vue` para receber `marker` e `saving` via props; remover `useMarkerStore` import; emitir `save` e `remove` eventos; pai gerencia estado otimista.

## 10.5. Transições e responsividade
- [ ] 10.2 Em `app.css`, definir `.page-enter-active`, `.page-leave-active`, `.page-enter-from`, `.page-leave-to` usando `var(--transition-fast)` e `var(--ease)`.
- [ ] 10.3 Adicionar media query `@media (prefers-reduced-motion: reduce)` para desabilitar transições.
- [ ] 10.4 Confirmar responsividade: viewport ≤768px → grids 1 coluna, todos os botões/links ≥44px, breadcrumb quebra linha.

## 11. Seleções estáveis (data-testid)
- [ ] 11.1 Adicionar `data-testid` em elementos críticos:
     - Formulário de login (`data-testid="login-form"`, inputs e botão).
     - Migalha de pão (`data-testid="breadcrumb-item"` para cada ancestra, `data-testid="breadcrumb-current"` para atual).
     - Cards de status (`data-testid="status-card"`).
     - Botões de ação (`data-testid="scan-start"`, `data-testid="sync-log"`).
     - Linhas de marcador (`data-testid="marker-row"`).
     - Painel de log (`data-testid="log-panel"`).
     - Toggle de tema (`data-testid="theme-toggle"`).
- [ ] 11.2 Confirmar execução `npm run build` ainda passando.

## 11.5. QA e validação
- [ ] 11.3 Executar `openspec validate redesign-frontend-neutral --strict` e confirmar aprovação.
- [ ] 11.4 Executar `npm run lint` e `npm run build` sem erros.
- [ ] 11.5 Executar `npm run test:tokens` e confirmar todos os blocos de contraste passam.
- [ ] 11.6 Executar checklist visual manual (desktop + mobile) e registrar achados.
- [ ] 11.7 Preparar cobertura E2E: listar os 7 fluxos e marcar para escrita posterior usando `data-testid`.

## 12. Fechamento
- [ ] 12.1 Executar `openspec status --change redesign-frontend-neutral --json` e confirmar `isPlanningComplete: true`, `isComplete: true`, `applyRequires: []`.
- [ ] 12.2 Archive a mudança com `/opsx-archive` se os requisitos de revisão do projeto forem atendidos.
- [ ] 12.3 Reportar arquivos alterados, progresso de tarefas e próximos passos.