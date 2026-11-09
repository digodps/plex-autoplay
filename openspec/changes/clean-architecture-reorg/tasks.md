# Tasks

## 1. Preparação de pastas

- [x] 1.1 Criar pastas `infra/`, `tools/` (e confirmar `scripts/`, `docs/`)
- [x] 1.2 Confirmar que `scripts/` já existe com `_common.sh`

## 2. Movimentação de arquivos (git mv para preservar histórico)

- [x] 2.1 `git mv Dockerfile infra/`
- [x] 2.2 `git mv Dockerfile.test-executor infra/`
- [x] 2.3 `git mv docker-compose.yml infra/`
- [x] 2.4 `git mv docker-compose.test.yml infra/`
- [x] 2.5 `git mv .env.example infra/`
- [x] 2.6 `git mv qa-ui-check.mjs tools/`
- [x] 2.7 `git mv test-sync-fix.mjs tools/`
- [x] 2.8 `git mv DEVELOPMENT_GUIDE.md docs/`
- [x] 2.9 `git mv pc-build-recommendation.md docs/`
- [x] 2.10 `git mv skills-lock.json docs/`
- [x] 2.11 `git mv build.sh scripts/`
- [x] 2.12 **NÃO mover** `opencode.json` (descoberta automática OpenCode)
- [x] 2.13 **NÃO mover** `PlexAutoplay.slnx` (descoberta automática IDEs .NET)
- [x] 2.14 **NÃO mover** `eslint.config.js`, `.prettierrc.json`, `.prettierignore`, `.pre-commit-config.yaml`, `package.json`, `package-lock.json` (descoberta automática de ferramentas)

## 3. Atualização de referências — Scripts

- [x] 3.1 `scripts/build.sh`: `dotnet build PlexAutoplay.slnx`, `docker compose -f infra/docker-compose.yml`
- [x] 3.2 `scripts/start.sh`: `.env` na raiz, `infra/docker-compose.yml`, mensagem `cp infra/.env.example .env`
- [x] 3.3 `scripts/stop.sh`: `PROJECT_ROOT`, `infra/docker-compose.yml`
- [x] 3.4 `scripts/logs.sh`: `infra/docker-compose.yml`
- [x] 3.5 `scripts/run-e2e-tests.sh`: `infra/docker-compose.test.yml`, mensagem `infra/.env.example`

## 4. Atualização de referências — Docker / Compose

- [x] 4.1 `infra/docker-compose.yml`: `context: ..`, `dockerfile: infra/Dockerfile`, `env_file: ../.env`, `volumes: ../data:/data`
- [x] 4.2 `infra/docker-compose.test.yml`: `context: ..`, dockerfiles em `infra/`, volumes `../*`
- [x] 4.3 `infra/Dockerfile`: COPY paths relativos ao context `..` validados
- [x] 4.4 `infra/Dockerfile.test-executor`: COPY paths relativos ao context `..` validados

## 5. Atualização de referências — CI/CD

- [x] 5.1 `.github/workflows/ci.yml`: `docker compose --env-file .env -f infra/docker-compose.test.yml`
- [x] 5.2 `.github/workflows/publish-container.yml`: `file: ./infra/Dockerfile`
- [x] 5.3 `actionlint` nos workflows

## 6. Atualização de referências — Lint / Package

- [x] 6.1 `eslint.config.js` permanece na raiz (ferramenta); nenhuma mudança de caminho necessária
- [x] 6.2 `.prettierrc.json` / `.prettierignore` permanecem na raiz; nenhuma mudança necessária
- [x] 6.3 `package.json` permanece na raiz; nenhuma mudança necessária

## 7. Atualização de referências — Testes e2e

- [x] 7.1 `tests/e2e/playwright.config.js` sem caminhos de compose hardcoded
- [x] 7.2 `tests/e2e/package.json` sem referência a compose
- [x] 7.3 `tools/qa-ui-check.mjs` / `tools/test-sync-fix.mjs` usam env `PLAYWRIGHT_BASE_URL`

## 8. Atualização README

- [x] 8.1 Comandos de deploy atualizados para `./scripts/start.sh` e `infra/docker-compose.yml`
- [x] 8.2 Estrutura de pastas documentada com `scripts/`, `infra/`, `docs/`, `tools/`
- [x] 8.3 Exceções de raiz documentadas (configs de ferramenta + LICENSE)

## 9. Validação (Build Gate)

- [x] 9.1 `bash -n scripts/*.sh`
- [x] 9.2 `dotnet build PlexAutoplay.slnx -c Release` (0 erros)
- [x] 9.3 `npx eslint . --config eslint.config.js`
- [x] 9.4 `docker compose -f infra/docker-compose.yml config`
- [x] 9.5 `docker build -f infra/Dockerfile -t plex-autoplay:reorg-test .`
- [x] 9.6 `docker compose -f infra/docker-compose.test.yml --env-file .env config`
- [x] 9.7 `actionlint .github/workflows/*.yml`
- [x] 9.8 `openspec validate clean-architecture-reorg --strict`

## 10. QA UI (Playwright)

- [x] 10.1 Subir aplicação via container
- [x] 10.2 Health check `/settings` 200
- [x] 10.3 Executar `node tools/qa-ui-check.mjs`
- [x] 10.4 Verificar checks: login, theme toggle, sidebar, breadcrumb, active state
- [x] 10.5 Parar aplicação

## 11. Commit final

- [x] 11.1 `git add -A`
- [x] 11.2 Commit de reorganização
- [x] 11.3 Push branch `feature/spec-clean-architecture-reorg`

## 12. EGM

- [x] 12.1 `deficiencias_resolvidas.md` com achados A/S/C
- [x] 12.2 Trade-offs e mitigações registrados