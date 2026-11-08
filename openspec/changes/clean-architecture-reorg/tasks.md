# Tasks

## 1. Preparação de pastas

- [ ] 1.1 Criar pastas `infra/`, `tools/` (e confirmar `scripts/`, `docs/`, `config/`)
- [ ] 1.2 Verificar se `scripts/` já existe e contém `_common.sh` (já existe)

## 2. Movimentação de arquivos (git mv para preservar histórico)

- [ ] 2.1 `git mv Dockerfile infra/`
- [ ] 2.2 `git mv Dockerfile.test-executor infra/`
- [ ] 2.3 `git mv docker-compose.yml infra/`
- [ ] 2.4 `git mv docker-compose.test.yml infra/`
- [ ] 2.5 `git mv qa-ui-check.mjs tools/`
- [ ] 2.6 `git mv test-sync-fix.mjs tools/`
- [ ] 2.7 `git mv DEVELOPMENT_GUIDE.md docs/`
- [ ] 2.8 `git mv pc-build-recommendation.md docs/`
- [ ] 2.9 ~~mover `opencode.json`~~ **NÃO mover**: descoberto automaticamente pelo OpenCode (EGM A3)
- [ ] 2.10 `git mv skills-lock.json docs/`
- [ ] 2.11 `git mv build.sh scripts/`
- [ ] 2.12 `git mv start.sh scripts/`
- [ ] 2.13 `git mv stop.sh scripts/`
- [ ] 2.14 `git mv publish-image.sh scripts/`
- [ ] 2.15 `git mv run-e2e-tests.sh scripts/`
- [ ] 2.16 `git mv acceptance-test.sh scripts/`
- [ ] 2.17 `git mv logs.sh scripts/`
- [ ] 2.18 `git mv sync-agent-configs.py scripts/`
- [ ] 2.19 `git mv code-review-skill.py scripts/`
- [ ] 2.20 `git mv commit-skill.py scripts/`
- [ ] 2.21 `git mv create-skill.py scripts/`
- [ ] 2.22 `git mv _common.sh scripts/`
- [ ] 2.23 ~~mover `PlexAutoplay.slnx`~~ **NÃO mover**: descoberta automática por IDEs (EGM A1)
- [ ] 2.24 **NÃO mover** `eslint.config.js`, `.prettierrc.json`, `.prettierignore`, `.pre-commit-config.yaml`, `package.json`, `package-lock.json` (descoberta automática de ferramentas; EGM A2/C4)

## 3. Atualização de referências — Scripts

- [ ] 3.1 `scripts/build.sh`: ajustar caminhos
  - `dotnet build config/PlexAutoplay.slnx -c Release`
  - `docker compose -f infra/docker-compose.yml build`
  - `docker compose -f infra/docker-compose.yml down/up`
  - chamadas internas a `scripts/*.sh` relativas a `$ROOT`
- [ ] 3.2 `scripts/start.sh`: usar `scripts/build.sh`, `infra/docker-compose.yml`
- [ ] 3.3 `scripts/stop.sh`: usar `infra/docker-compose.yml`
- [ ] 3.4 `scripts/publish-image.sh`: usar `infra/docker-compose.yml`, `infra/Dockerfile`
- [ ] 3.5 `scripts/run-e2e-tests.sh`: usar `infra/docker-compose.test.yml`
- [ ] 3.5 `scripts/acceptance-test.sh`: usar `infra/docker-compose.yml`
- [ ] 3.6 Verificar `scripts/_common.sh` se referencia arquivos movidos

## 4. Atualização de referências — Docker / Compose

- [ ] 4.1 `infra/docker-compose.yml`:
  - `context: ..`
  - `dockerfile: infra/Dockerfile` (ou `Dockerfile` se context=..)
  - `env_file: ../.env`
  - `volumes: ../data:/data`
  - `args: GIT_COMMIT`
- [ ] 4.2 `infra/docker-compose.test.yml`:
  - `context: ..`
  - `dockerfile: infra/Dockerfile.test-executor` (ou caminho correto)
  - `env_file: ../.env`
  - volumes `../data:/data`, `../tests/e2e:/app`, etc.
- [ ] 4.3 `Dockerfile` (agora `infra/Dockerfile`): COPY paths relativos ao context `..` → `COPY config/PlexAutoplay.slnx ./`, `COPY src/...` (inalterado)
- [ ] 4.4 `Dockerfile.test-executor`: COPY paths ajustados

## 5. Atualização de referências — CI/CD

- [ ] 5.1 `.github/workflows/ci.yml`:
  - `dotnet build config/PlexAutoplay.slnx`
  - `docker compose -f infra/docker-compose.test.yml`
  - `scripts/run-e2e-tests.sh`
- [ ] 5.2 `.github/workflows/publish-container.yml`:
  - `docker build -f infra/Dockerfile .`
  - `docker compose -f infra/docker-compose.yml`
- [ ] 5.3 Rodar `actionlint .github/workflows/ci.yml .github/workflows/publish-container.yml`

## 6. Atualização de referências — Lint / Package

- [ ] 6.1 Se `eslint.config.js` movido para `config/`:
  - `vue/plex-frontend/package.json` script `lint`: `eslint src --config ../../config/eslint.config.js`
  - `pre-commit` hook args: `--config config/eslint.config.js`
- [ ] 6.2 Se `.prettierrc.json` movido:
  - `prettier --config config/.prettierrc.json`
- [ ] 6.3 `package.json` raiz: se mantido, sem mudança; se movido, ajustar todos `npm run <script> --prefix config`

## 7. Atualização de referências — Testes e2e

- [ ] 7.1 `tests/e2e/playwright.config.js` (se existir): ajustar `baseURL` ou `use.baseURL` se mudar porta
- [ ] 7.2 Verificar `tests/e2e/package.json` scripts usam `docker-compose -f infra/docker-compose.test.yml`
- [ ] 7.3 Verificar `qa-ui-check.mjs` e `test-sync-fix.mjs` usam `BASE` env (não hardcoded)

## 8. Atualização README

- [ ] 8.1 Substituir comandos de deploy:
  - `./scripts/start.sh --detach`
  - `docker compose -f infra/docker-compose.yml up -d`
- [ ] 8.2 Atualizar seção de desenvolvimento referenciando `scripts/`, `infra/`, `config/`
- [ ] 8.3 Documentar exceções: `package.json`, `eslint.config.js`, `.prettierrc.json`, `LICENSE` permanecem na raiz por convenção de ferramenta

## 9. Validação (Build Gate)

- [ ] 9.1 `bash -n scripts/*.sh` (syntax check)
- [ ] 9.2 `dotnet build config/PlexAutoplay.slnx -c Release`
- [ ] 9.3 `npm run lint` (ou `npx eslint . --config config/eslint.config.js` se movido)
- [ ] 9.4 `docker compose -f infra/docker-compose.yml config`
- [ ] 9.5 `docker build -f infra/Dockerfile . -t plex-autoplay:test-reorg`
- [ ] 9.6 `docker compose -f infra/docker-compose.test.yml config`
- [ ] 9.7 `actionlint .github/workflows/*.yml`
- [ ] 9.8 `openspec validate clean-architecture-reorg --strict`
- [ ] 9.9 `scripts/build.sh` (dry-run ou completo se ambiente permitir) — verificar health 200

## 10. QA UI (Playwright)

- [ ] 10.1 Subir aplicação via `scripts/start.sh --detach` (ou `docker compose -f infra/docker-compose.yml up -d`)
- [ ] 10.2 Aguardar health check `/settings` 200
- [ ] 10.3 Executar `node tools/qa-ui-check.mjs` (com `PLAYWRIGHT_BASE_URL` e `ADMIN_PASSWORD` do .env)
- [ ] 10.4 Verificar todos os checks: login, theme toggle, sidebar collapse/expand, breadcrumb, active state
- [ ] 10.5 Parar aplicação `scripts/stop.sh`

## 11. Commit final

- [ ] 11.1 `git add -A`
- [ ] 11.2 `git commit -m "chore(reorg): reorganize root to Clean Architecture layout
  
  Move automation to scripts/, docs to docs/, containers to infra/,
  configs to config/, QA tools to tools/, slnx to config/.
  Update all path references in scripts, Docker, compose, CI, lint, README.
  Build gate + QA UI PASS.
  "`
- [ ] 11.3 Push branch `feature/spec-clean-architecture-reorg`

## 12. Arquivo de achados EGM (deficiencias_resolvidas.md)

- [ ] 12.1 Documentar decisões de exceção (package.json, eslint.config.js na raiz) com classificação A/S/C
- [ ] 12.2 Registrar trade-offs e mitigações