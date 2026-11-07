# Tasks — ui-sidebar-header

## Phase 1 — Requisitos e artefatos

- [x] **EGM A/S/C** — 3 revisores paralelos, achados triados em
  `deficiencias_resolvidas.md`.
- [x] **Reconciliar specs** — delta `web-ui` reescrito e delta
  `navegacao-lateral` com requisitos modificados.
- [x] **Atualizar proposal/design/tasks** para caminhos reais do código.

## Phase 2 — Implementação

- [x] **Sidebar presentacional** — `components/Sidebar/SidebarArea.vue`
  - Receber props `items` e `collapsed`; não importar stores, router ou `api.js`.
  - Renderizar `<ul>` com as bibliotecas, `space-y-1` entre itens.
  - Remover botão "Menu", toggle de tema e item estático "Bibliotecas".
  - Item: `RouterLink` para `/libraries/<key>/series`, ícone + título,
    `aria-current` na biblioteca ativa, alvo ≥44px, foco visível.
  - Compacto: rótulo oculto, `title`/`aria-label` com o título.
  - Estado vazio e estado de erro (`role="alert"`).
- [x] **Layout** — `components/layouts/DefaultLayout.vue`
  - Buscar bibliotecas com `fetchLibraries()` de `api.js`.
  - Passar `items`/`collapsed` para a sidebar.
  - Header na largura total; sidebar abaixo do header com altura
    `calc(100vh - 3.5rem)` e scroll próprio; main com scroll.
- [x] **Breadcrumb do header** — `components/AppHeader.vue` +
  `components/layouts/DefaultLayout.vue` + `lib/breadcrumb.js`
  - `Início` fixo; demais níveis dinâmicos (`/libraries/:key/series` →
    `<biblioteca>`; `/series/:seriesKey/markers` → `<biblioteca> > <série>`).
  - Título da biblioteca vem das bibliotecas carregadas pelo layout; fallback
    `route.query.libraryTitle` e `Biblioteca indisponível`.
  - Header é o único dono da trilha: remover `BreadcrumbDefault` de
    `views/Series/SeriesView.vue` e `views/Markers/MarkerEditorView.vue`.
  - Separador só entre itens; último item com `aria-current="page"`.
- [x] **Testes** — `tests/breadcrumb.test.mjs` + script `test:breadcrumb`.
- [x] **Tailwind** — restaurar `gray-2` em `tailwind.config.cjs`.
- [x] **Testes** — alinhar `tests/sidebar-collapsed.test.mjs` (chave
  `sidebar.collapsed.v2`) e `tests/tokens.contrast.test.mjs`
  (`src/styles/style.css`, seletor `.dark`).

## Phase 3 — Verificação

- [x] **Build gate** — `npm run build` em `vue/plex-frontend` passa.
- [x] **Lint** — `npm run lint` sem erros.
- [x] **Testes** — `npm run test:sidebar`, `npm run test:breadcrumb` e
  `npm run test:tokens` passam.
- [x] **OpenSpec** — `openspec validate ui-sidebar-header --strict` sem erros.
- [x] **QA independente** — agente separado gera `qa-report-2.md` com PASS/FAIL.
  - Playwright em `http://localhost:<port>`:
    - sidebar abaixo do header;
    - bibliotecas vindas da API listadas com espaçamento;
    - ausência de "Menu", toggle de tema e "Bibliotecas" estático;
    - navegação para `/libraries/<key>/series` e `aria-current`;
    - colapso pelo hamburger;
    - tema claro/escuro aplicado;
    - breadcrumb `Início > <biblioteca>` em `/libraries/<key>/series` e
      `Início > <biblioteca> > <série>` em marcadores;
    - apenas um `<nav aria-label="Breadcrumb">` na página.
- [x] **Docker acceptance** — build do frontend, `docker build --no-cache -t
  plex-autoplay:ui-sidebar-header .`, `docker run -p 8080:80`, validação da UI.

## Pendências (fora do escopo)

- Troca de senha no `SettingsModal`.
- Precedência `localStorage` vs `/api/theme` e modo `system`.
- Logout invalidando sessão no servidor.
