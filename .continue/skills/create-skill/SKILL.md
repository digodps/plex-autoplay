---
name: create-skill
description: Cria ou atualiza skill usando proposta OpenSpec como fonte e sincroniza todos clientes.
allowed-tools: Bash(*)
license: MIT
---

# Skill `create-skill` (`/create-skill`)

Cria skill somente depois de gerar material OpenSpec. OpenSpec define intenção, design e tarefas; skill transforma esse material em instruções reutilizáveis.

## Fluxo obrigatório

1. Receber nome kebab-case e descrição opcional.
2. Executar workflow `/opsx-propose <nome>` em background quando cliente suportar execução assíncrona. Aguardar conclusão antes de continuar.
3. Ler `proposal.md`, `design.md` e `tasks.md` da change criada.
4. Criar ou atualizar `.agent-source/skills/<nome>/SKILL.md` com material OpenSpec.
5. Executar `python3 scripts/sync-agent-configs.py`.
6. Validar cópias nos clientes `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.

## Uso

```bash
python3 scripts/create-skill.py <nome> --desc "descrição" [--force]
```

Não marque workflow como concluído antes de proposal, design e tasks existirem. Falha em OpenSpec interrompe criação; não sincronize skill incompleta.
