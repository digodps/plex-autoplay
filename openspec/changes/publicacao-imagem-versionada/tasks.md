# Tasks — publicacao-imagem-versionada

## Phase 1 — Requisitos e artefatos

- [x] **EGM A/S/C** — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [x] **Reconciliar artefatos** — proposal/design/specs/tasks coerentes com os achados aceitos.

## Phase 2 — Implementação

- [x] **Script** `scripts/publish-image.sh` — cálculo de versão, validações, tag anotada, push de branch + tag, `--dry-run`, `--help`.
- [x] **Workflow** `.github/workflows/publish-container.yml` — trigger em tags `v*.*.*`, guarda semver estrita, tags semver (`{{version}}`, `{{major}}.{{minor}}`, `latest` só em tag), imagem via `${{ github.repository }}`, `concurrency`.
- [x] **Teste** `scripts/publish-image.test.sh` — asserts de bump/dry-run/flags inválidas/tree sujo/sem remote em repo temporário.

## Phase 3 — Verificação

- [x] **Sintaxe** `bash -n scripts/publish-image.sh` passa.
- [x] **Teste** `bash scripts/publish-image.test.sh` passa (bump patch/minor/major, sem tags, dry-run, flags inválidas, tree sujo, sem remote).
- [x] **Build gate** `dotnet build PlexAutoplay.slnx -c Release` passa.
- [x] **OpenSpec** `openspec validate publicacao-imagem-versionada --strict` sem erros.
- [x] **QA independente** `qa-report-1.md` (FAIL) e `qa-report-2.md` (PASS) após correções.

## Phase 4 — Aceitação

- [x] **Docker acceptance** `docker build --no-cache -t plex-autoplay:publicacao-imagem-versionada .` + `docker run -p 8095:8080 -v /tmp/qa-data:/data --env-file .env ...` responde em `/login`.
- [x] **UI Playwright** tema claro/escuro e menus validados em `http://localhost:<port>`.
