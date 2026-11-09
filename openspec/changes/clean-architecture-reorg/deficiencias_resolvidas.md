# Deficiências Resolvidas — EGM (Goldfish Review)

Revisão paralela A/S/C sobre a proposta `clean-architecture-reorg` antes da implementação.

---

## A — Arquitetura, Dependências e Integração

| # | Achado | Classificação | Justificativa / Resolução |
|---|--------|---------------|---------------------------|
| A1 | `PlexAutoplay.slnx` movido para `config/` quebra descoberta automática de solução em IDEs (VS Code C# Dev Kit, Rider, Visual Studio) que esperam `.slnx` na raiz do repositório. | **Parcial** | Aceitar a mudança mas documentar: desenvolvedores devem abrir a pasta `config/` como workspace ou configurar `dotnet` para usar `config/PlexAutoplay.slnx`. Alternativa: manter `PlexAutoplay.slnx` na raiz (exceção documentada). **Decisão**: mover para `config/` e atualizar `global.json` (novo) na raiz apontando `config/` como `msbuildProjectExtensionsPath`? Não há suporte padrão. Melhor manter `.slnx` na raiz. **Resolução final**: manter `PlexAutoplay.slnx` na raiz. Atualizar tarefa 2.23 para *não mover*. |
| A2 | `package.json` e `eslint.config.js` na raiz são descobertos automaticamente por `npm`, `eslint`, `prettier`, `pre-commit`. Mover para `config/` exige `--config`/`--prefix` em todos os comandos (CI, scripts, hooks), aumentando complexidade e risco de desvio. | **Parcial** | Aceitar manter `package.json`, `package-lock.json`, `eslint.config.js`, `.prettierrc.json`, `.prettierignore` na raiz como "arquivos de configuração de ferramenta", não "código solto". Exceção documentada no design e README. |
| A3 | `opencode.json` na raiz é lido pelo OpenCode automaticamente. Mover para `docs/` quebra a descoberta. | **Aceito** | Manter `opencode.json` na raiz. Não mover. |
| A4 | `LICENSE` na raiz é convenção legal/ecossistema (GitHub, npm, SPDX). Remover quebra ferramentas de compliance. | **Aceito** | Manter `LICENSE` na raiz. |
| A5 | `docker-compose.yml` e `Dockerfile` movidos para `infra/` exigem `context: ..` e `dockerfile: infra/Dockerfile`. Isso funciona no Docker Compose v2, mas quebra `docker build .` sem flags. Scripts e CI atualizados para `docker compose -f infra/docker-compose.yml` e `docker build -f infra/Dockerfile .`. | **Aceito** | Atualizações previstas nas tarefas 3–4. Validação via `docker compose config` e `docker build`. |
| A6 | `scripts/build.sh` usa `ROOT=$(git rev-parse --show-toplevel)`. Após mover para `scripts/`, o script continua encontrando a raiz corretamente. Referências internas a `vue/plex-frontend`, `src/` permanecem válidas pois `$ROOT` é a raiz do repo. | **Aceito** | Sem mudança lógica, apenas caminhos de compose/slnx. |

---

## S — SOLID e Coesão

| # | Achado | Classificação | Justificativa / Resolução |
|---|--------|---------------|---------------------------|
| S1 | A change é puramente organizacional (reorganização física), sem introdução de nova lógica de negócio, ports/adapters, ou responsabilidades. Não viola SRP/OCP/LSP/ISP/DIP. | **Aceito** | Nenhum impacto em princípios SOLID. |
| S2 | Agrupar arquivos por responsabilidade (scripts, infra, config, tools, docs) aumenta coesão de cada pasta e reduz acoplamento acidental na raiz. | **Aceito** | Alinha com Clean Architecture (separation of concerns no nível de repo). |

---

## C — Clean Code, Escopo e Testabilidade

| # | Achado | Classificação | Justificativa / Resolução |
|---|--------|---------------|---------------------------|
| C1 | Escopo da change é bem delimitado: apenas movimentação de arquivos e atualização de caminhos. Nenhuma refatoração de código-fonte (`.cs`, `.js`, `.vue`). | **Aceito** | Baixo risco de regressão funcional. |
| C2 | Testabilidade: a mudança não altera contratos, APIs, nem UI. Testes existentes (unit, e2e) continuam válidos. QA UI obrigatória (Playwright) valida que tema/menu ainda funcionam após rebuild. | **Aceito** | QA gate suficiente. |
| C3 | `qa-ui-check.mjs` e `test-sync-fix.mjs` movidos para `tools/`; imports de `@playwright/test` resolvem via `node_modules` da raiz (node resolution sobe até encontrar). Funciona sem mudança. | **Aceito** | Verificar na execução. |
| C4 | `.pre-commit-config.yaml` movido para `config/` exige que `pre-commit` seja invocado com `-c config/.pre-commit-config.yaml` ou link simbólico. Pre-commit roda na raiz por padrão; se arquivo não estiver lá, hook falha silenciosamente. | **Parcial** | Opções: (a) manter `.pre-commit-config.yaml` na raiz; (b) criar symlink raiz→config; (c) documentar `pre-commit install -c config/.pre-commit-config.yaml`. **Decisão**: manter `.pre-commit-config.yaml` na raiz (exceção). |
| C5 | `actionlint` valida workflows GitHub Actions. Workflows referenciam caminhos antigos (`docker-compose.yml`, `Dockerfile`, `scripts/...`). Devem ser atualizados antes do commit. Tarefa 5 cobre isso. | **Aceito** | Validação `actionlint` no build gate. |

---

## Resumo de Exceções Decididas (arquivos que PERMANECEM na raiz)

| Arquivo | Motivo |
|---------|--------|
| `README.md` | Requisito explícito |
| `LICENSE` | Convenção legal |
| `.gitignore`, `.gitattributes`, `.dockerignore`, `.env`, `.env.example` | Arquivos ocultos / bootstrap obrigatórios |
| `package.json`, `package-lock.json` | Descoberta automática npm/node_modules |
| `eslint.config.js` | Descoberta automática ESLint flat config |
| `.prettierrc.json`, `.prettierignore` | Descoberta automática Prettier |
| `.pre-commit-config.yaml` | Descoberta automática pre-commit |
| `opencode.json` | Descoberta automática OpenCode |
| `PlexAutoplay.slnx` | Descoberta automática IDEs .NET (revisão A1) |

Esses 12 arquivos (9 visíveis + 3 ocultos bootstrap) são exceções técnicas aceitas. A regra "somente README visível na raiz" é atendida para **arquivos de automação, documentação, containerização e ferramentas de QA** — que eram o cerne da desorganização reportada.

---

## Aprovação para Implementação

Todos os achados críticos resolvidos ou mitigados. Change autorizada para Fase 2 (Commit + Worktree) e Fase 3 (Apply/Implement).