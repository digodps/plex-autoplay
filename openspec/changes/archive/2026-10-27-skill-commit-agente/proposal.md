# Proposal

## Why

Os commits do repositório `plex-autoplay` não seguem um padrão unificado: alguns usam conventional commit com escopo, outros não; uns têm emoji, outros não; a maioria carece de corpo descritivo explicando o porquê e o o quê. Não há mecanismo que impeça um "commitão" com muitas alterações desconexas em um único commit. Como o agente agora participa dos commits, é necessário padronizar a forma como ele commita — e estender o mesmo padrão a todos os commits, incluindo os feitos por humanos — para manter o histórico legível, rastreável e agrupado por blocos lógicos.

## What Changes

- Criar uma skill do agente que commita por blocos lógicos (agrupados por task do OpenSpec e tipo de mudança), nunca um único commit com tudo
- Padronizar o cabeçalho de commit como `tipo(🎯escopo): título`, usando conventional commit com emoji no tipo
- Exigir corpo descritivo livre (por que + o que foi feito) após o cabeçalho
- Aplicar o padrão a todos os commits: do agente e de humanos
- Remover qualquer `Co-authored-by` fixo dos commits
- Executar o commit diretamente, bloco por bloco, sem confirmação intermediária por bloco

## Capabilities

### New Capabilities

Nenhum. Esta mudança é de ferramenta e processo (workflow de commit); não introduz comportamento de sistema durável que deva ser especificado.

### Modified Capabilities

Nenhum. Os specs existentes continuam inalterados em nível de requisito.

## Impact

- `scripts/`: nova skill de commit (ex: `scripts/commit-skill.py`) invocada pelo agente
- `openspec/changes/skill-commit-agente/`: artefatos de planejamento desta própria mudança
- Workflow de commit: validação de padrão (`tipo(emoji escopo): título` + corpo descritivo) antes de cada `git commit`
- Possível adição de hook Git (`commit-msg` ou `prepare-commit-msg`) para validação automática em commits humanos
