# publicacao-imagem Specification

## Purpose
TBD - created by archiving change publicacao-imagem-versionada. Update Purpose after archive.

## Requirements

### Requirement: Version bump from git tags

O script `scripts/publish-image.sh` SHALL derivar a próxima versão semver da maior tag existente no formato `vX.Y.Z` e SHALL aplicar patch bump por padrão.

#### Scenario: Repositório com tag existente

- **WHEN** a maior tag é `v1.4.2` e o script roda sem flags
- **THEN** a versão calculada é `1.4.3`

#### Scenario: Repositório sem tags semver

- **WHEN** não existe nenhuma tag `vX.Y.Z`
- **THEN** a versão calculada é `0.1.0`

#### Scenario: Bump explícito

- **WHEN** a maior tag é `v1.4.2` e o script roda com `--minor`
- **THEN** a versão calculada é `1.5.0`

- **WHEN** a maior tag é `v1.4.2` e o script roda com `--major`
- **THEN** a versão calculada é `2.0.0`

### Requirement: Tag creation and push

O script SHALL criar uma tag anotada `vX.Y.Z` no commit atual e SHALL enviar o branch atual e a tag para `origin`.

#### Scenario: Publicação bem-sucedida

- **WHEN** o working tree está limpo, a tag alvo não existe e o script roda
- **THEN** é criada a tag anotada `vX.Y.Z`
- **AND** o branch atual e a tag são enviados para `origin`

#### Scenario: Tag já existente

- **WHEN** a tag calculada já existe no repositório
- **THEN** o script falha com mensagem clara e não executa push

#### Scenario: Working tree sujo

- **WHEN** `git status --porcelain` retorna linhas
- **THEN** o script falha antes de criar qualquer tag

#### Scenario: Push remoto rejeitado

- **WHEN** o push do branch atual falha
- **THEN** o script aborta sem enviar a tag e retorna código 1

### Requirement: Argument validation

O script SHALL rejeitar flags desconhecidas e flags de bump conflitantes com código de saída 2.

#### Scenario: Flag inválida

- **WHEN** o script roda com `--foo`
- **THEN** imprime o uso e retorna código 2

#### Scenario: Bump explícito de patch

- **WHEN** a maior tag é `v1.4.2` e o script roda com `--patch`
- **THEN** a versão calculada é `1.4.3`

### Requirement: Dry run

O script SHALL oferecer `--dry-run` que imprime a versão calculada e os comandos sem criar tag nem executar push.

#### Scenario: Simulação

- **WHEN** o script roda com `--dry-run`
- **THEN** a saída mostra a tag alvo e o comando de push
- **AND** nenhuma tag é criada e nada é enviado para `origin`

### Requirement: Versioned image publication

O workflow `publish-container.yml` SHALL disparar em push de tags `v*.*.*` e SHALL publicar a imagem no GHCR com as tags `X.Y.Z`, `X.Y` e `latest`.

#### Scenario: Push de tag de release

- **WHEN** a tag `v1.4.3` é enviada para o repositório
- **THEN** o workflow publica `ghcr.io/<owner>/<repo>:1.4.3`, `:1.4` e `:latest`

#### Scenario: Push em main

- **WHEN** um commit chega em `main`
- **THEN** o workflow publica apenas a tag de sha, sem `latest` e sem tags semver
