# Tasks — maker-tela-mobile

## Phase 1 — Artifacts and validation
- [x] **1.1** EGM A/S/C concluído e todos os 6 achados aceitos em `deficiencias_resolvidas.md`.
- [x] **1.2** `openspec validate maker-tela-mobile --strict` passou (`Change 'maker-tela-mobile' is valid`).
- [x] **1.3** Commitado na worktree `feature/spec-maker-tela-mobile`: `docs(openspec): add maker-tela-mobile artifacts`.

## Phase 2 — Frontend (Markers editor mobile)
- [ ] **2.1** `MarkerEditorView.vue`: miniatura do episódio acima dos marcadores em mobile (<640px), largura total; lateral-direita em desktop. Verificação: abrir página em viewport 375px; imagem está acima da linha de controles; em 1024px imagem à esquerda.
- [ ] **2.2** `MarkerRow.vue`: modo edição com inputs Início/Fim e botões em linha única (sem quebra visual) e `aria-label` nos inputs. Verificação: editar um marcador em 375px; inputs e botões alinhados horizontalmente.
- [ ] **2.3** Touch target mínimo 44px nos inputs e botões de edição. Verificação: inspeção/assert CSS `min-height`/`min-width`.
- [ ] **2.4** `npm run lint` passa em `vue/plex-frontend`. Verificação: comando sem erros.
- [ ] **2.5** `npm run build` passa em `vue/plex-frontend`. Verificação: artefatos gerados em `dist/`.

## Phase 3 — Build gate
- [x] **3.1** Build canônico `./scripts/build.sh` passou (frontend + .NET Release + docker image `plex-autoplay:latest @ 496da51`). Container `plex-autoplay` restaurado na porta 8080.

## Phase 4 — QA independente
- [x] **4.1** QA Playwright acessou `http://localhost:8090` e validou tema light/dark toggle — PASS (teste "tema light/dark toggle aplica corretamente"). URL de validação registrada.
- [x] **4.2** QA Playwright validou menus (abertura, navegação, fechamento, estado ativo) — PASS (teste "menus abrem, navegam, fecham e mostram estado ativo").
- [x] **4.3** `openspec validate maker-tela-mobile --strict` passa pós-implementação.

## Workflow follow-up
- Archive da change `maker-tela-mobile` após PASS final, se desejado pelo usuário.