# Slash command `/code-review` — Claude Code

Registre este comando no Claude Code para disparar a skill de revisão de
código no working tree ativo.

## Como instalar

Copie o conteúdo abaixo para o arquivo de comandos do Claude Code
(typicamente `~/.claude/commands/code-review.md` ou conforme documentação
do seu cliente Claude):

```markdown
---
description: "Executa a skill de revisão de código (lint, OWASP Top 10, NIST SSDF, Clean Code, SOLID)"
---

Executa `python scripts/code-review-skill.py` no repositório atual,
varrendo o working tree e gerando specs de correção para itens selecionados.

Uso: `/code-review [--fix 1,2,3] [--dry-run]`
```

Após a instalação, digite `/code-review` em qualquer conversa do Claude Code
com o repositório `plex-autoplay` como working tree ativo.

## Comportamento

O comando aponta para o mesmo ponto único de implementação:
`scripts/code-review-skill.py` — usado também pelo OpenCode (`/code-review`),
GitHub Copilot (command `/code-review`), Continue e outros clientes de
agente que espelham esta skill.

A skill:
1. Varre o working tree e identifica arquivos alterados em .NET/C# e JS/Vue
2. Executa lint por stack (dotnet-format + StyleCop.Analyzers; ESLint + Prettier)
3. Cruza achados com OWASP Top 10 (2021) e NIST SSDF
4. Avalia Clean Code e SOLID
5. Pergunta interativamente quais itens corrigir
6. Gera specs de correção OpenSpec (proposal.md, design.md, specs/<cap>/spec.md, tasks.md) para cada item selecionado

Não usa confirmação intermediária para a varredura; a seleção de itens para
correção é feita via `--fix 1,2,3` ou resposta ao prompt interativo.
