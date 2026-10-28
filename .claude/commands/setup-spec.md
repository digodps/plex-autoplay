# Slash command `/setup-spec` — Claude Code

Registre este comando no Claude Code para disparar a skill de setup
isolado no working tree ativo.

## Como instalar

Copie o conteúdo abaixo para o arquivo de comandos do Claude Code
(typicamente `~/.claude/commands/setup-spec.md` ou conforme documentação do
seu cliente Claude):

```markdown
---
description: "Executa a skill de setup isolado para mudanças OpenSpec"
---

Executa `python scripts/setup-spec.py` no repositório atual, criando um
branch remoto dedicado para uma mudança OpenSpec e deixando o branch local
pronto para uso (apontando para o remoto).

Padrão de branch: `feature/<mudanca>-<span>` onde `<span>` segue
`dd-mm-yyyy HH:mm:ss:mmm` (sanitizado para nomes de branch).

Uso: `/setup-spec [--change <nome>] [--dry-run]`

Após a instalação, digite `/setup-spec` em qualquer conversa do Claude Code
com o repositório `plex-autoplay` como working tree ativo.
```

## Comportamento

O comando aponta para o mesmo ponto único de implementação:
`scripts/setup-spec.py` — usado também pelo OpenCode (`/setup-spec`)
e por execução manual no terminal.

O fluxo executa:
1. Identifica a mudança OpenSpec ativa (via `openspec list --json` ou `--change`)
2. Gera um span no formato `dd-mm-yyyy HH:mm:ss:mmm`
3. Cria branch `feature/<mudanca>-<span-sanitizado>` a partir do `main` remoto limpo
4. Faz push do branch para origin
5. Deixa o branch local pronto para uso (apontando para o remoto)

O PR é criado manualmente pelo usuário. Não usa `Co-authored-by`.
