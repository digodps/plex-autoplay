# Deficiências resolvidas — EGM A/S/C

Change: `publicacao-imagem-versionada`. Três revisores paralelos (A=arquitetura, S=SOLID/coesão, C=Clean Code/escopo).

## Aceitos e aplicados

| # | Origem | Achado | Resolução |
|---|---|---|---|
| 1 | A1 | Push de branch + tag dispara dois runs, ambos gravando `:latest` | `latest` só em push de tag de release; push em `main` publica apenas `:sha`. `concurrency` no workflow. |
| 2 | A2 | Glob `v*.*.*` casa versões inválidas e `raw latest` publica mesmo assim | Step de guarda `^v[0-9]+\.[0-9]+\.[0-9]+$` antes do build; script valida a mesma regex antes de taguear. |
| 3 | A3/S2 | Cálculo parte de tags locais; tag remota existente só falha no push | `git fetch --tags origin` antes do cálculo + `git ls-remote --tags origin` antes de taguear. |
| 4 | A4 | `type=sha`/`type=semver` não condicionados ao ref | `enable=` explícito por tipo no `metadata-action`. |
| 5 | A5/S3 | Push conjunto branch+tag pode impedir a tag de subir | Push sequencial: branch primeiro (falha → aborta), tag depois. |
| 6 | A7/S5 | `v*.*.*` casa pré-release e sufixos; `head -n1` sob `pipefail` pode retornar 141 | Lista filtrada com `grep -E '^v[0-9]+\.[0-9]+\.[0-9]+$'`, `sort -V`, `tail -n1`/`awk 'NR==1'`. |
| 7 | S1/S7/C4 | Script monolítico, sem funções testáveis nem exit codes definidos | Funções `current_version` e `bump_version`; exit 1 erro git/push, 2 uso inválido, 3 tag já existe, 4 sem remote. |
| 8 | C2 | Sem teste automatizado do cálculo | `scripts/publish-image.test.sh` (asserts com repositório git temporário). |
| 9 | C6 | Cenários faltando | Spec ganha `--patch` explícito, flag inválida e push rejeitado. |
| 10 | C7 | Playwright não valida nada desta change | Mantido apenas como smoke de UI obrigatório do fluxo (tema/menu), sem novos cenários de spec. |

## Rejeitados (com motivo)

| # | Origem | Achado | Motivo |
|---|---|---|---|
| 1 | C1 | "Artefatos markdown corrompidos" | Falso positivo: `openspec validate --strict` passa e os arquivos têm `### Requirement:`/`#### Scenario:`. |
| 2 | C3 | Duplicação de parsing de versão script↔workflow | Não é duplicação: o script produz a tag; `metadata-action` apenas deriva `X.Y` da tag no CI. Implementar isso no script criaria acoplamento e um segundo passo de publicação. |
| 3 | C8 | Remover README do escopo | Aceito como remoção: `--help` documenta; README omitido. |
| 4 | A6 | `github.repository` com maiúsculas no GHCR | `docker/metadata-action` normaliza a imagem para lowercase; nome atual já é minúsculo. |
| 5 | A8 | Workflow `workflow_dispatch` pode mover `latest` | Dispatch só publica `:sha` (não publica `latest`), mesma regra de branch. |

## Parciais

| # | Origem | Achado | Decisão |
|---|---|---|---|
| 1 | C5 | Rollback documentado | Registrado no design: reexecutar o workflow sobre a tag anterior via `workflow_dispatch` (publica `:sha`), sem retag automático. `latest` é sobrescrito apenas por tag de release mais nova. |
