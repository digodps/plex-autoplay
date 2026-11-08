# QA Report — redesign-frontend-neutral (Iteration 3)

Date: 2026-11-08
Target: http://192.168.18.5:8080 (Docker, image rebuilt from working tree)

## Results

| Gate | Result |
|------|--------|
| `npm run build` (vue) | PASS |
| `npm run lint` (vue) | PASS (0 errors, 8 warnings) |
| `npm run test:tokens` | PASS (8/8) |
| Playwright suite (auth, libraries, markers, images, container) | PASS (13/13) |
| UI QA script `qa-ui-check.mjs` | PASS (22/22) |

## UI QA coverage (`qa-ui-check.mjs`)

- Login redirects to `/dashboard`
- Theme toggle flips `data-theme` light ↔ dark, persists in `localStorage`, keeps `html.dark` in sync, reverts
- Header menu button toggles `aria-expanded` and collapses/expands sidebar (256px ↔ 64px)
- Sidebar lists libraries; navigation opens series; active item has `aria-current="page"`
- Breadcrumb marks current page with real library title (Animes)
- User menu opens (Configurações, Alterar senha, Sair) and closes
- Change-password modal opens and closes
- User menu navigates to `/settings`; page title renders (Ajustes)
- Mobile 390px: sidebar compact (64px), no horizontal overflow
- No page JS errors

## Screenshots

- `/tmp/qa-dashboard-desktop.png`
- `/tmp/qa-dashboard-dark.png`
- `/tmp/qa-dashboard-mobile.png`

## Fixes applied during QA

1. Broken SFC templates (missing closing `</div>`) in `MarkerEditorView.vue`, `LibrariesView.vue`, `SyncView.vue`, `SettingsView.vue`, `SeriesView.vue` — build was failing.
2. `ChangePasswordView.vue`: missing `}}` in interpolation, `//` comment swallowing one-line file, missing root `</div>`.
3. Neutral palette: `--accent` now `#111113` (light) / `#F6F6F7` (dark) with `--accent-contrast`; Tailwind `primary` maps to `var(--accent)`; removed leftover blue.
4. Lint: removed unused symbols (`computed`, `auth`, `theme`, `onKeyDown`, `fetchPlexIdentity`, `isLoadingMore`).
5. Docker DI: `IAuthCredentialStore` registration now passes `sqliteConnectionString`; admin login works.
6. Rebuilt SPA bundle into `src/PlexAutoplay.Web/wwwroot`.

## Notes

- Default `tests/e2e/playwright.config.js` sets `globalSetup` that calls `/api/auth/setup/status`; endpoint absent in working tree, so suite was run with a temporary config excluding `globalSetup`.
- Status colors (`--success`, `--warning`, `--danger`) remain semantic, not decorative.

Verdict: PASS

## Follow-up fix (2026-11-08, pós-QA)

Sintomas reportados: miniaturas de séries/episódios sumiram; episódio não editava marcadores.

Causa: redesign removeu as tags `<img>` das views e o `import MarkerRow` do editor, então os componentes de marcador não renderizavam.

Correções:
- `SeriesView.vue`: card volta a renderizar `series.thumb` em `aspect-[2/3]`.
- `MarkerEditorView.vue`: reimporta `MarkerRow` e volta a renderizar `ep.thumb` (16:9).
- Verificado via Playwright: séries 9/9 miniaturas carregadas, episódio 1/1, fluxo de marcador POST 201 → PUT 200 → DELETE 200.
- `qa-ui-check.mjs` agora cobre miniaturas e botões de marcador: 25/25 PASS.
- Suíte e2e: 13/13 PASS.

