# Design

## Context

A skill `implement-spec` já existe em `scripts/implement-spec.py` e é compatível com Git (cria branch remoto `feature/<mudanca>-<span>`, faz push para origin, clona em pasta irmã `plex-implement-<mudanca>-<span>/`, copia artefatos da mudança e commita). Porém, não pode ser invocada diretamente pelo agente nos ambientes OpenCode e Claude, como acontece com a skill `commit-skill.py` (via comando `/commit` no OpenCode e slash command no Claude). Sem essa integração, o agente precisa executar o script Python manualmente no terminal. Veja a motivação em `proposal.md - Why`.

## Goals / Non-Goals

**Goals:**
- Criar `.opencode/commands/implement-spec.md` — comando `/implement-spec` no terminal OpenCode que executa `python scripts/implement-spec.py [--change <nome>] [--dry-run]`
- Criar `.claude/commands/implement-spec.md` — slash command `/implement-spec` no Claude Code que dispara `python scripts/implement-spec.py` no working tree ativo
- Manter `scripts/implement-spec.py` como ponto único de implementação da lógica (usado por OpenCode, Claude e execução manual)
- O padrão de branch `feature/<mudanca>-<span>` e o fluxo de clone em pasta irmã permanecem inalterados

**Non-Goals:**
- Modificar a lógica de `scripts/implement-spec.py` (já implementada e funcionando)
- Automatizar a criação de PR (feito manualmente pelo usuário)
- Substituir o pre-commit existente (ruff, actionlint)
- Alterar specs de comportamento do sistema (esta mudança é de ferramenta/processo)

## Decisions

**1. Ponto único de implementação: `scripts/implement-spec.py`**
- Rationale: o script já existe e contém toda a lógica de Git (fetch, checkout, push, clone, add, commit). Os comandos OpenCode e Claude apenas o invocam, sem duplicar lógica
- Alternativa considerada: duplicar a lógica em cada ambiente. Rejeitado: viola DRY e dificulta manutenção

**2. Comando OpenCode: `.opencode/commands/implement-spec.md`**
- Rationale: segue o mesmo padrão de `.opencode/commands/commit.md`, que já existe para a skill `commit-skill.py`. O agente pode digitar `/implement-spec` no terminal OpenCode
- Alternativa considerada: fazer o usuário digitar `python scripts/implement-spec.py` manualmente. Rejeitado: fricção desnecessária

**3. Slash command Claude: `.claude/commands/implement-spec.md`**
- Rationale: segue o mesmo padrão de `.claude/commit-skill.md`, que já existe para a skill `commit-skill.py`. O agente pode digitar `/implement-spec` no Claude Code
- Alternativa considerada: não adicionar integração Claude. Rejeitado: o usuário pediu compatibilidade com Claude, igual à skill de commit

**4. Argumentos `--change` e `--dry-run` preservados**
- Rationale: o script já aceita esses argumentos; os comandos OpenCode e Claude os expõem para o usuário informar a mudança ou simular a execução
- Exemplo de uso: `/implement-spec --change implementar-cache-generico-plugavel`

**5. `skip_specs: true` no `.openspec.yaml`**
- Rationale: mudança de ferramenta/processo sem alteração de comportamento de sistema; nenhum spec existente ou novo precisa ser criado ou modificado

## Risks / Trade-offs

[O agente pode não saber qual mudança passar no --change] → Mitigar: se houver múltiplas mudanças em andamento, o próprio script já lista as opções e pede escolha; o comando apenas repassa o argumento

[Diretório `.claude/commands/` pode não existir em todos os clientes] → Mitigar: o arquivo é documentativo; o usuário pode copiar para o local correto conforme seu cliente Claude

[Branches remotos se acumulam se os PRs não forem mesclados] → Mitigar: manutenção periódica de branches obsoletos no GitHub (fora do escopo desta mudança)

## Migration Plan

1. Criar `.opencode/commands/implement-spec.md` com o comando `/implement-spec`
2. Criar `.claude/commands/implement-spec.md` com o slash command `/implement-spec`
3. Verificar que `scripts/implement-spec.py` já existe e é executável (criado na change anterior)
4. O agente pode então invocar `/implement-spec` em OpenCode e Claude
5. Rollback: remover os dois arquivos de comando; a skill continua funcionando via execução manual do script Python

## Open Questions

Nenhuma. Todas as decisões foram resolvidas com base no padrão existente da skill `commit-skill.py`.
