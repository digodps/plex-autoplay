# Slash command `/commit` — Claude Code

Registre este comando no Claude Code para disparar a skill de commit no
working tree ativo.

## Como instalar

Copie o conteúdo abaixo para o arquivo de comandos do Claude Code
(typicamente `~/.claude/commands/commit.md` ou conforme documentação do
seu cliente Claude):

```markdown
---
description: "Executa a skill de commit por blocos lógicos"
---

Executa `python scripts/commit-skill.py` no repositório atual, agrupando
alterações em blocos lógicos por task do OpenSpec + tipo de mudança.

Padrão: `tipo(🎯escopo): título` + corpo descritivo livre.
Cada bloco vira um `git commit` separado.

Uso: `/commit [--change <nome>] [--dry-run]`
```

Após a instalação, digite `/commit` em qualquer conversa do Claude Code
com o repositório `plex-autoplay` como working tree ativo.

## Comportamento

O comando aponta para o mesmo ponto único de implementação:
`scripts/commit-skill.py` — usado também pelo OpenCode (`/commit`) e
pelo GitHub Copilot (command `/commit`).

Não usa `Co-authored-by`. Commit direto por bloco, sem confirmação
intermediária.
