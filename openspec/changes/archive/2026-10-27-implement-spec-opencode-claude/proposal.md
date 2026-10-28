# Proposal

## Why

A skill `implement-spec` já existe (`scripts/implement-spec.py`) e é compatível com Git (cria branch remoto, clona em pasta irmã, copia artefatos e commita), mas não pode ser invocada diretamente pelo agente nos ambientes OpenCode e Claude, como acontece com a skill `commit-skill.py` (via comando `/commit` no OpenCode e slash command no Claude). Sem essa integração, o agente precisa executar o script Python manualmente no terminal, perdendo a conveniência de chamar a skill pelo mesmo comando `/implement-spec` em todos os ambientes.

## What Changes

- Criar `.opencode/commands/implement-spec.md` — comando `/implement-spec` no terminal OpenCode que executa `python scripts/implement-spec.py [--change <nome>] [--dry-run]`
- Criar `.claude/commands/implement-spec.md` — slash command `/implement-spec` no Claude Code que dispara `python scripts/implement-spec.py` no working tree ativo
- Manter `scripts/implement-spec.py` como ponto único de implementação da lógica (usado por OpenCode, Claude e execução manual)

## Capabilities

### New Capabilities

Nenhum. Esta mudança é de ferramenta e processo (integração de ambiente para skill existente); não introduz comportamento de sistema durável que deva ser especificado.

### Modified Capabilities

Nenhum. Os specs existentes continuam inalterados em nível de requisito.

## Impact

- `.opencode/commands/implement-spec.md`: novo comando OpenCode `/implement-spec`
- `.claude/commands/implement-spec.md`: novo slash command Claude `/implement-spec`
- `scripts/implement-spec.py`: permanece como ponto único de implementação (já existe, não é modificado)
- Workflow: agente pode invocar `/implement-spec` no OpenCode e no Claude para criar branch remoto e clone isolado por mudança OpenSpec
