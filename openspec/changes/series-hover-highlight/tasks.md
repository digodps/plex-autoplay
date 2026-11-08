# Tasks — series-hover-highlight

## Phase 1 — Requisitos e artefatos

- [x] **EGM A/S/C** — achados triados em `deficiencias_resolvidas.md`.
- [x] `openspec validate series-hover-highlight --strict` passa.
- [x] Commit somente dos artefatos da change.
- [x] Worktree `feature/spec-series-hover-highlight` criada.

## Phase 2 — Implementação

- [x] `SeriesView.vue`: contêiner da miniatura `relative overflow-hidden` + wrapper interno com `transition-transform duration-300 motion-safe:group-hover:scale-105 motion-safe:group-focus-visible:scale-105 motion-reduce:transition-none` (cobre img e placeholder).
- [x] `SeriesView.vue`: overlay `pointer-events-none absolute inset-0 bg-black/0 group-hover:bg-black/25 group-focus-visible:bg-black/25 motion-reduce:transition-none` com `aria-hidden="true"`.
- [x] `SeriesView.vue`: botão do card com `focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2`; remover `dark:hover:text-primary`; manter borda e navegação.
- [x] `qa-ui-check.mjs`: asserts de transform no hover, foco por Tab e reduced-motion.

## Phase 3 — Build

- [x] `npm run lint` passa em `vue/plex-frontend`.
- [x] `npm run build` passa em `vue/plex-frontend`.
- [x] `dotnet build PlexAutoplay.slnx` passa.

## Phase 4 — QA

- [x] `openspec validate series-hover-highlight --strict` passa.
- [x] QA com Playwright em URL local: hover no card destaca miniatura; foco por Tab também; light/dark sem regressão; clique navega.
- [x] `qa-report-1.md` registra PASS/FAIL, URL acessada e evidências.
