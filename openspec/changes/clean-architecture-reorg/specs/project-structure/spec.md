# Spec Delta

## Purpose
Organização física do repositório seguindo Clean Architecture: raiz contendo apenas `README.md` (e arquivos ocultos obrigatórios), com todos os demais artefatos alocados em pastas dedicadas por responsabilidade (scripts/, docs/, infra/, config/, tools/).

## ADDED Requirements

### Requirement: Raiz limpa
O repositório SHALL conter na raiz apenas `README.md`, `.gitignore`, `.gitattributes`, `.env`, `.env.example`, `.dockerignore`, `LICENSE`. Nenhum outro arquivo visível (sem prefixo `.`) deve permanecer na raiz.

#### Scenario: Verificação de raiz
- **WHEN** executar `ls -la` na raiz do repositório (ignorando `.git/`)
- **THEN** apenas os arquivos listados acima estão presentes como entradas de topo (não diretórios)

### Requirement: Scripts em pasta dedicada
Todos os scripts de automação, deploy, teste e utilitários SHALL residir em `scripts/`.

#### Scenario: Scripts localizados
- **WHEN** listar `scripts/`
- **THEN** conter `build.sh`, `start.sh`, `stop.sh`, `publish-image.sh`, `run-e2e-tests.sh`, `acceptance-test.sh`, `logs.sh`, `sync-agent-configs.py`, `code-review-skill.py`, `commit-skill.py`, `create-skill.py`, `_common.sh`

### Requirement: Documentação em pasta dedicada
Toda documentação solta (guias, relatórios, recomendações) SHALL residir em `docs/`.

#### Scenario: Docs localizados
- **WHEN** listar `docs/`
- **THEN** conter `DEVELOPMENT_GUIDE.md`, `pc-build-recommendation.md`, `qa-ui-check.mjs`, `test-sync-fix.mjs`, `skills-lock.json`, `opencode.json` e subpasta `designs/`

### Requirement: Infraestrutura em pasta dedicada
Arquivos de containerização e orquestração (Dockerfile, docker-compose, env examples) SHALL residir em `infra/`.

#### Scenario: Infra localizada
- **WHEN** listar `infra/`
- **THEN** conter `Dockerfile`, `Dockerfile.test-executor`, `docker-compose.yml`, `docker-compose.test.yml`, `.env.example`

### Requirement: Configurações de ferramentas em pasta dedicada
Arquivos de configuração de lint, format, package manager, solution .NET SHALL residir em `config/`.

#### Scenario: Config localizada
- **WHEN** listar `config/`
- **THEN** conter `eslint.config.js`, `.prettierrc.json`, `.prettierignore`, `.pre-commit-config.yaml`, `package.json`, `package-lock.json`, `PlexAutoplay.slnx`

### Requirement: Utilitários de teste/QA em pasta dedicada
Scripts de validação UI e sincronismo de teste SHALL residir em `tools/`.

#### Scenario: Tools localizada
- **WHEN** listar `tools/`
- **THEN** conter `qa-ui-check.mjs`, `test-sync-fix.mjs`

### Requirement: Caminhos atualizados em todos os arquivos
Todos os arquivos que referenciam caminhos da raiz antiga SHALL ser atualizados para os novos caminhos relativos a partir da raiz do repositório.

#### Scenario: Build.sh funcional
- **WHEN** executar `./scripts/build.sh` a partir da raiz
- **THEN** build completa (frontend, .NET, docker) sem erros de caminho

#### Scenario: Docker compose funcional
- **WHEN** executar `docker compose -f infra/docker-compose.yml build` a partir da raiz
- **THEN** imagem construída com sucesso

#### Scenario: CI/CD funcional
- **WHEN** rodar workflows GitHub Actions (ci.yml, publish-container.yml)
- **THEN** jobs passam com caminhos corretos

#### Scenario: ESLint funcional
- **WHEN** executar `npm run lint` (ou `npx eslint .`) a partir da raiz
- **THEN** lint passa sem erros de caminho

#### Scenario: README instruções corretas
- **WHEN** ler `README.md`
- **THEN** comandos de inicio rápido usam caminhos novos (`scripts/build.sh`, `infra/docker-compose.yml`)