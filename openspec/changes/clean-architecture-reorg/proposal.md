# Proposal

## Why

A raiz do repositório acumula 25+ arquivos visíveis (scripts, Dockerfiles, docker-compose, configs, docs, ferramentas de teste, relatórios) misturados com pastas de código (src/, vue/, tests/, openspec/). Isso viola o princípio de Clean Architecture de manter a raiz limpa — apenas `README.md` — e organizar artefatos auxiliares em pastas dedicadas (scripts/, docs/, infra/, config/, tools/). O estado atual dificulta onboarding, CI/CD e manutenção; scripts referenciam caminhos hardcoded na raiz, e mudanças de estrutura quebram build.sh, Dockerfile, docker-compose, workflows.

## What Changes

- **Mover** todos os arquivos de raiz (exceto `README.md`, `.gitignore`, `.gitattributes`, `.env`, `.env.example`, `.dockerignore`, `LICENSE`) para pastas semânticas:
  - `scripts/` → scripts de automação (`build.sh`, `start.sh`, `stop.sh`, `publish-image.sh`, `run-e2e-tests.sh`, `acceptance-test.sh`, `logs.sh`, `sync-agent-configs.py`, `code-review-skill.py`, `commit-skill.py`, `create-skill.py`)
  - `docs/` → documentação solta (`DEVELOPMENT_GUIDE.md`, `pc-build-recommendation.md`, `qa-ui-check.mjs`, `test-sync-fix.mjs`, `skills-lock.json`, `opencode.json`) — *note:* `docs/` já existe; consolidar lá.
  - `infra/` (nova) → Dockerfiles, docker-compose, `.env*` de exemplo, `Dockerfile.test-executor`, `docker-compose.test.yml`
  - `config/` (nova) → configs de lint/format (`eslint.config.js`, `.prettierrc.json`, `.prettierignore`, `.pre-commit-config.yaml`, `package.json`, `package-lock.json`, `PlexAutoplay.slnx`)
  - `tools/` (nova) → utilitários de teste/QA que não são scripts de deploy (`qa-ui-check.mjs`, `test-sync-fix.mjs`)
- **Atualizar** todos os caminhos referenciados nos arquivos movidos e nos que permanecem:
  - `build.sh` (caminhos de `vue/plex-frontend`, `src/`, `docker compose`)
  - `Dockerfile` (COPY paths)
  - `docker-compose.yml` / `docker-compose.test.yml` (context, volumes, env_file)
  - `.github/workflows/*.yml` (working-directory, paths)
  - `eslint.config.js` (rootDir, project paths)
  - `package.json` (scripts, workspaces)
  - `openspec/config.yaml` (se referenciar paths)
  - `README.md` (instruções de uso)
- **Manter** na raiz apenas: `README.md`, `.gitignore`, `.gitattributes`, `.env`, `.env.example`, `.dockerignore`, `LICENSE` (licença costuma ficar na raiz por convenção).
- **Garantir** que `dotnet build`, `npm run build`, `docker compose build`, testes e2e e CI continuem passando sem alteração de comportamento.

## Capabilities

### New Capabilities

- `project-structure`: Organização física do repositório seguindo Clean Architecture — raiz limpa, pastas dedicadas por responsabilidade, caminhos relativos consistentes.

### Modified Capabilities

- `deploy`: Caminhos de build e publicação atualizados para nova estrutura de pastas.
- `ci-pipeline`: Workflows GitHub Actions ajustados para novos caminhos.
- `development-guide`: `DEVELOPMENT_GUIDE.md` movido para `docs/` e referências atualizadas.

## Impact

- **Arquivos movidos**: ~25 arquivos de raiz redistribuídos em 4 pastas novas/existentes.
- **Arquivos editados** (referências de caminho): `build.sh`, `Dockerfile`, `docker-compose.yml`, `docker-compose.test.yml`, `.github/workflows/ci.yml`, `.github/workflows/publish-container.yml`, `eslint.config.js`, `package.json`, `README.md`, `openspec/config.yaml` (se aplicável), `scripts/*.sh` (internos), `vue/plex-frontend/vite.config.js` (se referenciar root), `tests/e2e/playwright.config.js` (se existir).
- **Backend .NET**: `src/` inalterado (já segue Clean Architecture).
- **Frontend Vue**: `vue/` inalterado.
- **Testes**: `tests/` inalterado.
- **OpenSpec**: `openspec/` inalterado.
- **Dados runtime**: `data/` inalterado.
- **Sem mudança de comportamento** — apenas reorganização física.