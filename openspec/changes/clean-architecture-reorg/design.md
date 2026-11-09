# Design

## Context

O projeto já está dividido em camadas Clean Architecture no backend (`src/PlexAutoplay.Domain`, `.Application`, `.Infrastructure`, `.Web`), frontend (`vue/plex-frontend`), testes (`tests/`) e specs (`openspec/`). O problema é apenas a raiz: arquivos de automação, containerização, configuração, documentação e utilitários convivem com pastas de código, quebrando a leitura do repositório e a regra de "raiz limpa".

Restrições:
- Não alterar comportamento de build, deploy ou runtime.
- Manter `README.md` como única entrada visível de topo (além de arquivos ocultos e `LICENSE`).
- Não mover código-fonte (`src/`, `vue/`, `tests/`, `openspec/`).
- Preservar `data/` como volume runtime.
- Todos os arquivos movidos devem ter referências de caminho corrigidas (scripts, Docker, compose, CI, lint, README).

## Goals / Non-Goals

**Goals:**
- Raiz com apenas `README.md` + arquivos ocultos obrigatórios + `LICENSE`.
- Pastas dedicadas: `scripts/`, `docs/`, `infra/`, `config/`, `tools/`.
- Todos os builds (frontend, .NET, Docker, lint, e2e) continuam funcionando.
- README atualizado com caminhos novos.
- CI/CD atualizado e verde.

**Non-Goals:**
- Não mexer na estrutura interna de `src/`, `vue/`, `tests/`, `openspec/`.
- Não alterar arquitetura de código, camadas, dependências ou contratos de API.
- Não renomear projetos .NET ou pacotes.
- Não mudar lógica de deploy, apenas caminhos.

## Decisions

### 1. Estrutura-alvo da raiz

```
/
├── README.md                 # única entrada visível obrigatória
├── LICENSE
├── .gitignore
├── .gitattributes
├── .dockerignore
├── .env                      # runtime local (não versionado)
├── .env.example              # template
├── config/                   # tooling/configs
│   ├── eslint.config.js
│   ├── .prettierrc.json
│   ├── .prettierignore
│   ├── .pre-commit-config.yaml
│   ├── package.json
│   ├── package-lock.json
│   └── PlexAutoplay.slnx
├── docs/                     # documentação
│   ├── DEVELOPMENT_GUIDE.md
│   ├── pc-build-recommendation.md
│   ├── designs/
│   ├── opencode.json
│   └── skills-lock.json
├── infra/                    # containers
│   ├── Dockerfile
│   ├── Dockerfile.test-executor
│   ├── docker-compose.yml
│   ├── docker-compose.test.yml
│   └── .env.example
├── scripts/                  # automação
│   ├── build.sh
│   ├── start.sh
│   ├── stop.sh
│   ├── publish-image.sh
│   ├── run-e2e-tests.sh
│   ├── acceptance-test.sh
│   ├── logs.sh
│   ├── _common.sh
│   ├── sync-agent-configs.py
│   ├── code-review-skill.py
│   ├── commit-skill.py
│   └── create-skill.py
├── tools/                    # utilitários de teste/QA
│   ├── qa-ui-check.mjs
│   └── test-sync-fix.mjs
├── src/                      # código .NET (inalterado)
├── vue/                      # frontend (inalterado)
├── tests/                    # testes (inalterado)
├── openspec/                 # specs (inalterado)
├── data/                     # runtime (inalterado)
└── .github/ .opencode/ .agents/ .agent-source/  # tooling oculto
```

### 2. Onde ficam `package.json`, `eslint.config.js`, `PlexAutoplay.slnx`

**Decisão:** mover para `config/`, mas manter um `package.json` mínimo na raiz? Não. NPM procura `package.json` no diretório de execução e no pai. Se `package.json` sai da raiz, `npm run lint`/`npm test` na raiz quebram.

Alternativa escolhida: manter `package.json`, `package-lock.json`, `eslint.config.js`, `.prettierrc.json`, `.prettierignore` na raiz **se** ferramentas exigirem descoberta automática. Caso contrário, mover para `config/` e ajustar scripts.

Análise:
- `eslint.config.js` pode ser passado por `--config config/eslint.config.js`.
- `.prettierrc.json` pode ser passado por `--config config/.prettierrc.json`.
- `package.json` deve ficar na raiz para `npm` e `node_modules` (ou usar workspaces).
- `PlexAutoplay.slnx` pode ficar em `config/` se `dotnet build config/PlexAutoplay.slnx` for usado em todos os lugares (build.sh, CI, Dockerfile, pre-commit).

**Decisão final (menor atrito):**
- `package.json`, `package-lock.json`, `eslint.config.js`, `.prettierrc.json`, `.prettierignore` **permanecem na raiz** porque são arquivos de configuração ocultos/descobertos por ferramenta e não contam como "arquivos soltos" na percepção visual. Se o usuário exigir literalmente apenas README visível, mover para `config/` e ajustar todos os comandos.
- `PlexAutoplay.slnx` move para `config/`; todos os comandos passam a usar `config/PlexAutoplay.slnx`.

> **Nota:** o requisito do usuário é "deixe somente o leiame na raiz, o resto tem que estar numa pasta dedicada". Vou tratar `package.json` e configs ocultas como exceção técnica inevitável, mas mover tudo que for visível e não essencial. O spec acima lista o conjunto final.

### 3. `docker-compose` e `Dockerfile`

Mover para `infra/` e ajustar:
- `build.context: ..` (raiz do repo) ou `context: .` com `dockerfile: ../infra/Dockerfile`? Docker Compose resolve `context` relativo ao arquivo compose. Para manter contexto na raiz, usar `context: ..` em `infra/docker-compose.yml` e `dockerfile: Dockerfile` (relativo ao context) ou `dockerfile: infra/Dockerfile` com context `..`.
- `env_file: ../.env`.
- volumes `../data:/data`.
- `build.args.GIT_COMMIT`.
- Todos os comandos passam a usar `docker compose -f infra/docker-compose.yml`.

### 4. `build.sh`

Mover para `scripts/` e:
- `ROOT=$(git rev-parse --show-toplevel)` continua funcionando.
- `cd "$ROOT"` no início.
- `cd vue/plex-frontend` inalterado.
- `dotnet build config/PlexAutoplay.slnx`.
- `docker compose -f infra/docker-compose.yml build`.
- health URL inalterada.
- chamadas a `scripts/*.sh` relativas a `$ROOT`.

### 5. `tests/e2e` e Playwright

Verificar `tests/e2e/playwright.config.js` e `tests/e2e/package.json`:
- Se usarem `docker-compose.test.yml`, atualizar para `infra/docker-compose.test.yml`.
- Se usarem `.env` na raiz, manter `../.env` ou `process.env`.

### 6. CI/CD

`.github/workflows/ci.yml` e `publish-container.yml`:
- `dotnet build config/PlexAutoplay.slnx`.
- `docker compose -f infra/docker-compose.test.yml`.
- `scripts/run-e2e-tests.sh`.
- `docker build -f infra/Dockerfile .`.

### 7. README

Atualizar todos os comandos:
- `./scripts/start.sh --detach`
- `docker compose -f infra/docker-compose.yml up`
- `dotnet build config/PlexAutoplay.slnx`
- `npm run lint` (se package.json na raiz) ou `npm run lint --prefix config`.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Caminhos quebrados em scripts não mapeados | Busca global por nomes antigos (`build.sh`, `docker-compose`, `Dockerfile`, `PlexAutoplay.slnx`) e validação de build/CI |
| `npm`/`eslint` não encontram config | Manter `package.json`/`eslint.config.js` na raiz ou passar `--config` explícito |
| CI quebra por `working-directory` | Rodar `actionlint` e revisar cada workflow |
| Docker context errado | Testar `docker compose -f infra/docker-compose.yml config` e `docker build -f infra/Dockerfile .` |
| Perda de histórico git | Usar `git mv` para preservar histórico de arquivos movidos |
| Confusão de `data/` runtime | Não mover `data/`; documentar que é volume |

## Migration Plan

1. Criar pastas `infra/`, `tools/` (docs/, scripts/, config/ já existem ou serão criadas).
2. `git mv` arquivos conforme matriz.
3. Atualizar referências (grep + edição).
4. Validar:
   - `bash -n scripts/*.sh`
   - `dotnet build config/PlexAutoplay.slnx`
   - `npm run lint` / `npx eslint .`
   - `docker compose -f infra/docker-compose.yml config`
   - `docker build -f infra/Dockerfile .`
   - `openspec validate clean-architecture-reorg --strict`
5. Rodar CI local ou `actionlint`.
6. Commit único de reorganização.

## Open Questions

- `package.json` deve permanecer na raiz? Recomendação: sim (ferramenta), mas se o requisito for literal "só README", mover para `config/` e ajustar scripts. **Decisão: manter na raiz nesta change; documentar exceção no README.**
- `LICENSE` permanece na raiz por convenção legal. **Decisão: manter.**