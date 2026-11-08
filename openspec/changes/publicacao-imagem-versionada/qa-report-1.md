# QA Report 1 — publicacao-imagem-versionada

FAIL

Veredito estrito: implementação funciona e bate com o design, mas o `spec.md` contém um Scenario cujo comportamento não existe no código (`:latest` em push de `main`). Correção necessária é de artefato (spec), não de código.

## Achados

### 1. [Alto — spec vs implementação] Scenario "Push em main" pede `:latest`, workflow publica só `:sha`

`openspec/changes/publicacao-imagem-versionada/specs/publicacao-imagem/spec.md`:

> #### Scenario: Push em main
> - **WHEN** um commit chega em `main`
> - **THEN** o workflow publica `:latest` e a tag de sha, sem tags semver

`.github/workflows/publish-container.yml`:

```yaml
type=raw,value=latest,enable=${{ startsWith(github.ref, 'refs/tags/') }}
type=sha,enable=${{ !startsWith(github.ref, 'refs/tags/') }}
```

Em `main` só existe `:sha`; `:latest` só é publicada em push de tag. Isso segue a decisão aceita em `deficiencias_resolvidas.md` (achado A1: "`latest` só em push de tag de release; push em `main` publica apenas `:sha`"), `design.md` e `tasks.md`. Logo a implementação está coerente com o design, e o texto do `spec.md` (e a frase "além das tags de branch já existentes" do `proposal.md`) ficou desatualizado. Necessário: alinhar spec/proposal à decisão A1.

### 2. [Baixo] `tasks.md` afirma teste de "tag existente" que não existe em `publish-image.test.sh`

`tasks.md` Phase 3:

- `[x] Teste ... (bump patch/minor/major, sem tags, tag existente, dry-run)`

O teste executa apenas: sem tags → `v0.1.0`, `v1.4.2` → patch/minor/major, flags inválidas/conflitantes (só "exit != 0"), publicação real, `v1.4.4` após `v1.4.3`. Não há assert do exit code 3 para tag já existente, nem dos códigos 1/4. Esses códigos foram verificados manualmente por QA (abaixo), não pelo teste automatizado.

### 3. [Baixo] `die()` imprime o exit code na mensagem

`scripts/publish-image.sh:27-30`:

```bash
die() {
  echo "erro: $*" >&2
  exit "${2:-1}"
}
```

Evidência (reprodução com remote contendo tag não presente localmente):

```
erro: tag v1.4.3 já existe 3
tag-exists rc=3
```

O código sai correto (3), mas a mensagem ganha o sufixo "3" (idem para erros de uso, sufixo "2"). Usar `echo "erro: $1" >&2`.

### 4. [Médio — comando de aceitação do ticket incompleto] `docker run -p 8095:80` falha

Comando do enunciado: `docker run -d --name qa-publicacao -p 8095:80 --env-file .env plex-autoplay:publicacao-imagem-versionada`.

Resultado: `GET /login` → `HTTP 000`; container `Exited (139)`.

Logs (`/tmp/qa-docker.log`):

```
Listener HTTP ativado na porta 8080 (DISABLE_HTTP=false)
Inicializando schema do banco...
Unhandled exception. Microsoft.Data.Sqlite.SqliteException (0x80004005): SQLite Error 14: 'unable to open database file'.
```

Causa 1: `.env` define `WEB_PORT=8080`, sobrescrevendo o `ENV WEB_PORT=80` do Dockerfile, então o container escuta em 8080 (não 80).
Causa 2: `DATA`/`/data` não é bind-mounted; `DatabaseInitializer.DefaultConnectionString` usa `Data Source=/data/app.db`. Os containers existentes (`plex-autoplay`, `qa-sidebar`) montam `/home/hermes/plex-autoplay/data:/data`.

Comando de aceitação efetivo usado por QA (cópia do DB para não tocar o host):

```bash
cp /home/hermes/plex-autoplay/data/app.db /tmp/qa-publicacao-data/
docker run -d --name qa-publicacao -p 8095:8080 \
  -v /tmp/qa-publicacao-data:/data --env-file .env \
  plex-autoplay:publicacao-imagem-versionada
# GET /login => 200
```

Sugestão: documentar o mapeamento `8080` e o bind de `/data` no changelog/README de aceitação.

### 5. [Info] `tasks.md` Phase 3 (QA) e Phase 4 (Docker/UI) estão desmarcadas

Condizente com o estado: não executei marcação, conforme instrução. Após este report, as duas linhas de Phase 4 podem ser marcadas; a de QA também, mas o veredito estrito continua FAIL pelo achado 1 até o spec ser corrigido.

## Evidências executadas

### Sintaxe e teste

```
$ bash -n scripts/publish-image.sh
SYNTAX_OK

$ bash scripts/publish-image.test.sh
...
 * [new tag]         v1.4.3 -> v1.4.3
ok
```

### OpenSpec

```
$ openspec validate publicacao-imagem-versionada --strict
Change 'publicacao-imagem-versionada' is valid
EXIT=0
```

### Script — comportamento verificado em repos temporários

| Verificação | Resultado |
|---|---|
| `--help` | rc=0 |
| flag inválida `--foo` | rc=2 |
| flags conflitantes `--minor --major` | rc=2 |
| `--dry-run` | rc=0, nenhuma tag criada (`tags after=0`) |
| tag alvo já existe (local ausente / remoto presente) | rc=3, mensagem "tag v1.4.3 já existe" |
| working tree sujo | rc=1, nenhuma tag/push |
| sem remote `origin` | rc=4 |
| sem tags | `tag: v0.1.0` |
| tag `v1.4.2` → default/`--patch` | `v1.4.3` |
| `--minor` / `--major` | `v1.5.0` / `v2.0.0` |
| tag criada | `git cat-file -t v0.1.0` → `tag`, subject `release v0.1.0` (anotada) |
| push bem-sucedido | branch `refs/heads/main` + `refs/tags/v0.1.0` no origin |
| push da tag rejeitado | rc=1, tag local mantida, sem tag no origin |
| push do branch rejeitado (commit novo + hook `pre-receive`) | rc=1, tag local removida, sem tag no origin |

Ordem branch-antes-da-tag confirmada pelo caso de rejeição do branch: script emite `erro: push de main falhou; tag local v0.1.0 removida` e nunca chega a enviar a tag.

### Workflow (estático)

- `on.push.tags: ["v*.*.*"]` presente; `branches: [main]` mantido.
- Guarda `^v[0-9]+\.[0-9]+\.[0-9]+$` em step `Validate release tag` (só em `refs/tags/`), falha com `::error::`.
- Imagem `ghcr.io/${{ github.repository }}` (sem hardcode).
- `metadata-action`: `{{version}}`, `{{major}}.{{minor}}`, `raw=latest` habilitados só em tag; `type=sha` só fora de tag.
- `concurrency` e `timeout-minutes` presentes.

### Docker acceptance

```
docker build --no-cache -t plex-autoplay:publicacao-imagem-versionada .
# ... naming to docker.io/library/plex-autoplay:publicacao-imagem-versionada done

docker run -d --name qa-publicacao -p 8095:8080 -v /tmp/qa-publicacao-data:/data --env-file .env plex-autoplay:publicacao-imagem-versionada
# status: Up
GET /login -> 200
GET /api/plex/libraries -> 401 (sem token)
GET /api/dashboard -> 401 (sem token)
```

### UI — Playwright real (Chromium 1.64.1, headless)

URL validada: `http://localhost:8095/login` → login → `/dashboard`.

Saída do script `/tmp/qa-publicacao.mjs`:

```
PASS: login ok -> http://localhost:8095/dashboard
PASS: theme button label changed: "Tema escuro" -> "Tema claro"
PASS: html dark class toggled: false -> true (class="dark")
PASS: body background changed: rgb(255, 255, 255) -> rgb(24, 24, 27)
PASS: theme restored (class="")
PASS: hamburger aria-expanded toggled: true -> false
PASS: sidebar width changed: 256 -> 64
library links: 1
PASS: library nav /libraries/1/series -> http://localhost:8095/libraries/1/series
PASS: active link aria-current=page present (1)
PASS: user menu opened (aria-expanded false -> true, panel=false)

ASSERTS: 10 pass, 0 fail -> PASS
```

Screenshots no change:

- `openspec/changes/publicacao-imagem-versionada/qa-ui-1.png` (tema dark)
- `openspec/changes/publicacao-imagem-versionada/qa-ui-2.png` (sidebar colapsada)
- `openspec/changes/publicacao-imagem-versionada/qa-ui-3.png` (menu de usuário após Escape)

Nota: o menu de usuário muda `aria-expanded` e fecha com `Escape`; o painel em si não foi detectado por seletor `[role="menu"]`/texto `Sair|Logout` (painel pode usar outro markup). Assert principal é o `aria-expanded`.

## Arquivos esperados

- `scripts/publish-image.sh` (novo, executável)
- `scripts/publish-image.test.sh` (novo, executável)
- `.github/workflows/publish-container.yml` (modificado)
- `openspec/changes/publicacao-imagem-versionada/{proposal.md,design.md,tasks.md,specs/publicacao-imagem/spec.md}`
- `openspec/changes/publicacao-imagem-versionada/qa-ui-{1,2,3}.png`
- `openspec/changes/publicacao-imagem-versionada/qa-report-1.md` (este arquivo)

## Cleanup

Container de aceitação removido ao fim (`docker rm -f qa-publicacao`). Imagem `plex-autoplay:publicacao-imagem-versionada` mantida.
