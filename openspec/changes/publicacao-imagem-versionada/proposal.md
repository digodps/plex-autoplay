# Change Proposal — publicacao-imagem-versionada

## Why

O workflow `publish-container.yml` publica a imagem no GHCR apenas com `latest` e `sha`. Não existe versão legível nem histórico de releases, e não há script para cortar uma versão: hoje o passo é manual e sujeito a erro.

## What Changes

- Novo script `scripts/publish-image.sh` que:
  - calcula a próxima versão semver a partir das tags `vX.Y.Z` existentes (default: patch; flags `--patch`, `--minor`, `--major`);
  - valida o formato estrito `X.Y.Z` e sincroniza tags do remoto;
  - cria tag anotada `vX.Y.Z` no commit atual;
  - envia o branch atual e a tag para `origin`;
  - suporta `--dry-run` (mostra versão calculada sem efeitos) e `--help`.
- Workflow `publish-container.yml` passa a rodar também em push de tags `v*.*.*` e a publicar a imagem com tags de versão (`X.Y.Z`, `X.Y`, `latest`); pushes em `main` publicam apenas a tag de sha.
- Nome da imagem deixa de ser hardcoded (`digodps/plex-autoplay`) e passa a usar `${{ github.repository }}`.

## Scope

- `scripts/publish-image.sh` (novo)
- `.github/workflows/publish-container.yml`
- `scripts/publish-image.test.sh` (novo)

## Out of Scope

- Bump automático de versão em arquivos do projeto (csproj/package.json): a versão vive na tag git.
- Criação de GitHub Release, changelog automático ou assinatura de imagem.
- Alteração do processo de build/teste (CI existente permanece).
