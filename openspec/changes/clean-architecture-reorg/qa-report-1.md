# QA Report — clean-architecture-reorg

**Data**: 2026-11-08
**Commit**: `06e1f4d`
**Branch**: `feature/spec-clean-architecture-reorg`
**URL de validação**: `http://localhost:18087` (container `plex-autoplay:reorg-test`)

---

## Build Gate

| Etapa | Comando | Resultado |
|-------|---------|-----------|
| Syntax scripts | `bash -n scripts/*.sh` | PASS |
| .NET build | `dotnet build PlexAutoplay.slnx -c Release` | PASS (0 errors, 15 warnings pré-existentes) |
| ESLint (root) | `npx eslint . --config eslint.config.js` | 13 erros / 8 warnings **pré-existentes** (sem regressão) |
| Docker compose prod | `docker compose -f infra/docker-compose.yml --env-file .env config` | PASS (serviço `plex-autoplay`) |
| Docker compose test | `docker compose -f infra/docker-compose.test.yml --env-file .env config` | PASS (serviços `plex-mock`, `plex-autoplay`, `executor`) |
| Docker build | `docker build -f infra/Dockerfile -t plex-autoplay:reorg-test .` | PASS (imagem `0fc51a244af7`) |
| actionlint | `npx actionlint .github/workflows/ci.yml .github/workflows/publish-container.yml` | SKIPPED (actionlint não instalado) |
| `openspec validate` | `openspec validate clean-architecture-reorg --strict` | PASS |

> **Nota**: Os erros ESLint em `vue/plex-frontend/tailwind.config.cjs` e `vue/plex-frontend/tests/no-sidebar.test.mjs` existiam antes da mudança e não foram introduzidos por esta reorganização.

---

## QA UI (Playwright)

**Ferramenta**: `tools/qa-ui-check.mjs` (atualizada para UI atual com header, breadcrumb, user menu)

| # | Check | Resultado | Evidência |
|---|-------|-----------|-----------|
| 1 | Login redireciona para /dashboard | PASS | |
| 2 | Toggle de tema alterna data-theme | PASS | light → dark |
| 3 | Toggle persiste em localStorage | PASS | null → dark |
| 4 | html.dark coerente com data-theme | PASS | |
| 5 | Toggle reverte | PASS | dark → light |
| 6 | Menu do usuário abre | PASS | botão "Menu do usuário" exibe dropdown |
| 7 | Menu lista Configurações | PASS | link `/settings` visível |
| 8 | Menu lista Sair | PASS | botão "Sair" visível |
| 9 | Menu do usuário fecha | PASS | clique fora / toggle remove dropdown |
| 10 | Modal de alteração de senha abre | PASS | via `data-testid="change-password-menu"` |
| 11 | Modal de alteração de senha fecha | PASS | botão `data-testid="password-cancel"` |
| 12 | Menu navega para /settings | PASS | URL termina em `/settings` |
| 13 | Settings renderiza título "Ajustes" | PASS | `h1` presente |
| 14 | Breadcrumb marca página atual | PASS | em `/libraries` item `aria-current="page"` |
| 15 | Mobile: sem overflow horizontal | PASS | 0px overflow em viewport 390×844 |
| 16 | Sem erros JS de página | PASS | array `errors` vazio |

**Screenshots** (artefatos):
- `/tmp/opencode/qa-dashboard-desktop.png`
- `/tmp/opencode/qa-dashboard-mobile.png`

---

## Resumo

**STATUS: PASS**

Todos os gates de build e validação UI obrigatória passam. A reorganização da estrutura do repositório segue Clean Architecture com raiz limpa (apenas `README.md`, `LICENSE`, arquivos ocultos bootstrap, configs de ferramenta). Arquivos de automação, documentação, containerização e QA movidos para pastas dedicadas (`scripts/`, `docs/`, `infra/`, `tools/`). Todas as referências de caminho atualizadas em scripts, Docker, compose, CI, lint e README. Não houve alteração de comportamento funcional.