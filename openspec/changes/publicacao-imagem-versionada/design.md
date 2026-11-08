# Design — publicacao-imagem-versionada

## Fluxo

```
scripts/publish-image.sh [--major|--minor|--patch] [--dry-run]
        │
        ├─ git fetch --tags origin
        ├─ maior tag 'vX.Y.Z' (regex estrita)      → versão atual
        ├─ bump (default patch)                    → vX.Y.Z
        ├─ git tag -a vX.Y.Z -m "release vX.Y.Z"
        └─ git push origin HEAD && git push origin vX.Y.Z
                        │
                        ▼
        .github/workflows/publish-container.yml (push de tag v*.*.*)
                        │
                        └─ docker/build-push-action → ghcr.io/<repo>:X.Y.Z, :X.Y, :latest
```

## Fonte de verdade da versão

A tag git é a única fonte. Nenhum arquivo do projeto é reescrito — evita divergência entre csproj/package.json e a imagem publicada. `package.json`/`.csproj` continuam irrelevantes para o versionamento da imagem.

## Script (`scripts/publish-image.sh`)

- Bash com `set -euo pipefail`, mesmo estilo de `scripts/start.sh`/`scripts/stop.sh`; usa `source scripts/_common.sh` apenas se precisar de Docker (não precisa).
- Versão base: `git fetch --tags origin` e maior tag que casa `^v[0-9]+\.[0-9]+\.[0-9]+$` (`sort -V`). Sem tags → `0.1.0` (primeira release).
- Bump:
  - patch: `X.Y.Z+1`
  - `--minor`: `X.Y+1.0`
  - `--major`: `X+1.0.0`
- Pré-condições (falha com mensagem clara):
  - estar em um repositório git com remote `origin`;
  - working tree limpo (`git status --porcelain` vazio);
  - tag alvo ainda não existir localmente nem em `git ls-remote --tags origin`;
  - HEAD não estar detached.
- Ações:
  - `git tag -a "v$VERSION" -m "release v$VERSION"`;
  - `git push origin <branch>` (falha aborta);
  - `git push origin "v$VERSION"`.
- Exit codes: 1 erro git/push, 2 uso inválido, 3 tag já existe, 4 sem remote `origin`.
- `--dry-run`: imprime versão calculada e comandos, não executa tag/push.
- `--help`: uso.

## Workflow

- `on.push.tags: ['v*.*.*']` adicionado; `on.push.branches: [main]` mantido.
- `docker/metadata-action`:
  - `type=semver,pattern={{version}}` (só ativo em ref de tag);
  - `type=semver,pattern={{major}}.{{minor}}`;
  - `type=raw,value=latest,enable=${{ startsWith(github.ref, 'refs/tags/') }}`;
  - `type=sha,enable=${{ !startsWith(github.ref, 'refs/tags/') }}`.
- `images: ghcr.io/${{ github.repository }}`.
- Permissões atuais (`contents: read`, `packages: write`) já bastam; `GITHUB_TOKEN` publica no GHCR do próprio repo.

## Riscos

| Risco | Mitigação |
|---|---|
| Tag empurrada antes do commit estar no remoto | Script empurra `HEAD` e a tag juntos |
| Reexecução acidental | Falha se a tag já existir; bump produz versão nova |
| Dois runs concorrentes | `concurrency` no workflow; `latest` só em tag de release |
| Versão inválida publicada sem tag semver | Guarda `^v[0-9]+\.[0-9]+\.[0-9]+$` no workflow e no script |
| Repo com tags não-semver (`pre-clean-arch`) | Filtro `v*.*.*` ignora |
