# Tasks — publicacao-imagem-versionada

## Phase 1 — Requisitos e artefatos

- [ ] **EGM A/S/C** — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [ ] **Reconciliar artefatos** — proposal/design/specs/tasks coerentes com os achados aceitos.

## Phase 2 — Implementação

- [ ] **Script** `scripts/publish-image.sh` — cálculo de versão, validações, tag anotada, push de branch + tag, `--dry-run`, `--help`.
- [ ] **Workflow** `.github/workflows/publish-container.yml` — trigger em tags `v*.*.*`, guarda semver estrita, tags semver (`{{version}}`, `{{major}}.{{minor}}`, `latest` só em tag), imagem via `${{ github.repository }}`, `concurrency`.
- [ ] **Teste** `scripts/publish-image.test.sh` — asserts de bump/dry-run/tag existente em repo temporário.

## Phase 3 — Verificação

- [ ] **Sintaxe** `bash -n scripts/publish-image.sh` passa.
- [ ] **Teste** `bash scripts/publish-image.test.sh` passa (bump patch/minor/major, sem tags, tag existente, dry-run).
- [ ] **Build gate** `dotnet build PlexAutoplay.slnx -c Release` passa.
- [ ] **OpenSpec** `openspec validate publicacao-imagem-versionada --strict` sem erros.
- [ ] **QA independente** `qa-report-1.md` com PASS/FAIL.

## Phase 4 — Aceitação

- [ ] **Docker acceptance** `docker build --no-cache -t plex-autoplay:publicacao-imagem-versionada .` + `docker run` responde na porta publicada.
- [ ] **UI Playwright** tema claro/escuro e menus validados em `http://localhost:<port>`.
