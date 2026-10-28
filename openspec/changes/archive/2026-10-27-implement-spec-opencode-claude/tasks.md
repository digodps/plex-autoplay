# Tasks

## 1. Integração OpenCode

- [x] 1.1 Criar `.opencode/commands/implement-spec.md` que registra o comando `/implement-spec` no terminal OpenCode, executando `python scripts/implement-spec.py [--change <nome>] [--dry-run]`. Verificar que o arquivo é criado em `.opencode/commands/` e segue o mesmo padrão de `.opencode/commands/commit.md`.
- [x] 1.2 Documentar no comando OpenCode o padrão de branch `feature/<mudanca>-<span>`, o formato do span `dd-mm-yyyy HH:mm:ss:mmm`, o fluxo de clone em pasta irmã e o fato de que o PR é criado manualmente. Verificar que a documentação está presente e legível.

## 2. Integração Claude Code

- [x] 2.1 Criar `.claude/commands/implement-spec.md` que registra o slash command `/implement-spec` no Claude Code, disparando `python scripts/implement-spec.py` no working tree ativo. Verificar que o arquivo é criado e segue o mesmo padrão de `.claude/commit-skill.md`.
- [x] 2.2 Documentar no comando Claude o mesmo padrão de branch, formato do span, fluxo de clone e PR manual. Verificar que a documentação está presente e legível.

## 3. Verificação de consistência

- [x] 3.1 Verificar que `scripts/implement-spec.py` já existe e é executável (criado na change anterior `implement-spec`). Verificar que ambos os comandos (OpenCode e Claude) apontam para o mesmo script como ponto único de implementação.
- [x] 3.2 Rodar `openspec validate` para confirmar que a mudança está coerente (proposal, design, tasks consistentes, `skip_specs: true` aplicado corretamente). Verificar saída válida sem erros.

## Workflow follow-up

- Revisar a mudança com o time antes de aplicar.
- Após aprovação, executar `/opsx-apply` para implementar as tarefas acima.
- Archivar a mudança após revisão e aplicação.
