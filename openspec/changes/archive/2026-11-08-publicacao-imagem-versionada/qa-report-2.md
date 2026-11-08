PASS

# QA Report 2 — publicacao-imagem-versionada

Worktree: `/home/hermes/plex-autoplay-wt-publicacao`
Branch: `feature/spec-publicacao-imagem-versionada`
URL testada: `http://localhost:8095` (login, dashboard, `/libraries/1/series`)

## Verificação

### 1. Script e teste

```
$ bash -n scripts/publish-image.sh
syntax ok

$ bash scripts/publish-image.test.sh
ok
```

Cobertura confirmada em `scripts/publish-image.test.sh`: sem tags → `v0.1.0`, patch (`v1.4.3`), minor (`v1.5.0`), major (`v2.0.0`), `--dry-run` sem tag criada, `--help`, flag inválida (`--foo`, rc=2), flags conflitantes (`--minor --major`, rc=2), working tree sujo (falha), sem remote (`rc=4`).

### 2. OpenSpec

```
$ openspec validate publicacao-imagem-versionada --strict
Change 'publicacao-imagem-versionada' is valid
```

### 3. Coerência artefatos ↔ workflow

`.github/workflows/publish-container.yml`:

- `on.push.tags: ["v*.*.*"]` e `on.push.branches: [main]`.
- `concurrency: group: publish-container-${{ github.ref }}` presente.
- Guarda `^v[0-9]+\.[0-9]+\.[0-9]+$` no step `Validate release tag` (só `refs/tags/`).
- `images: ghcr.io/${{ github.repository }}`.
- `type=semver` + `type=raw,value=latest` com `enable=startsWith(github.ref, 'refs/tags/')` → `latest` só em push de tag.
- `type=sha` com `enable=!startsWith(...)` → push em `main` publica só `:sha`.

`spec.md`, `design.md` e `tasks.md` batem com esse comportamento (spec "Push em main" agora diz "apenas a tag de sha, sem `latest` e sem tags semver").

### 4. Mensagem de erro sem vazamento de exit code

`die()` em `scripts/publish-image.sh` usa `echo "erro: $msg"` e `exit "$code"` separado.

Reprodução em repo temporário:

```
$ publish-image.sh --foo
erro: opção desconhecida: --foo          # rc=2, última linha sem "2"

$ publish-image.sh  # tag alvo v1.0.1 já existe no remote
erro: tag v1.0.1 já existe                # rc=3, última linha sem "3"
```

### 5. Docker acceptance

```
$ docker run -d --name qa2-publicacao -p 8095:8080 \
    -v /tmp/qa2-publicacao-data:/data --env-file .env \
    plex-autoplay:publicacao-imagem-versionada
$ curl -o /dev/null -w '%{http_code}' http://localhost:8095/login
200
```

Imagem reutilizada da iteração 1: `plex-autoplay:publicacao-imagem-versionada` (`1aa0499abde0`).

Playwright (Chromium em `~/.cache/ms-playwright`), 10 asserts / 0 falhas:

```
PASS: login ok -> http://localhost:8095/dashboard
PASS: theme button label changed: "Tema escuro" -> "Tema claro"
PASS: html dark class toggled: false -> true (class="dark")
PASS: body background changed: rgb(255, 255, 255) -> rgb(24, 24, 27)
PASS: theme restored (class="")
PASS: hamburger aria-expanded toggled: true -> false
PASS: sidebar width changed: 256 -> 64
PASS: library nav /libraries/1/series -> http://localhost:8095/libraries/1/series
PASS: active link aria-current=page present (1)
PASS: user menu opened (aria-expanded false -> true)
ASSERTS: 10 pass, 0 fail -> PASS
```

Screenshots:

- `openspec/changes/publicacao-imagem-versionada/qa2-ui-1.png` (tema dark)
- `openspec/changes/publicacao-imagem-versionada/qa2-ui-2.png` (sidebar colapsada)
- `openspec/changes/publicacao-imagem-versionada/qa2-ui-3.png` (pós menu/Escape)

Container `qa2-publicacao` removido ao final.

## Achados (não bloqueantes)

1. **[Baixo — doc] `tasks.md` Phase 2 ainda diz "tag existente"** — `tasks.md:12` afirma "asserts de bump/dry-run/tag existente", mas o teste automatizado não tem assert do exit 3 (tag já existente). A linha Phase 3 (`tasks.md:17`) foi corrigida; a Phase 2 ficou. Exit 3 foi validado manualmente nesta QA (`rc=3`, mensagem correta). Sugestão: remover "tag existente" da linha 12 ou adicionar o assert.

2. **[Baixo — doc] `proposal.md:15` ainda diz "além das tags de branch já existentes"** — em push de `main` o workflow publica apenas `:sha`; não há "tags de branch". Sugestão: "além da tag `:sha` em push de `main`".

3. **[Baixo — cobertura] Exit 1 não é assertado exatamente** — `tree sujo` e `push rejeitado` (exit 1) só verificam "comando falhou", não `rc=1`; exit 4 é assertado exato. Opcional: assertar o código exato também no tree sujo.

## Arquivos

- `scripts/publish-image.sh`
- `scripts/publish-image.test.sh`
- `.github/workflows/publish-container.yml`
- `openspec/changes/publicacao-imagem-versionada/{proposal.md,design.md,tasks.md,specs/publicacao-imagem/spec.md}`
- `openspec/changes/publicacao-imagem-versionada/qa-report-2.md`
- `openspec/changes/publicacao-imagem-versionada/qa2-ui-{1,2,3}.png`
