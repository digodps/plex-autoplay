---
description: "Cria ou atualiza skill usando proposta OpenSpec como fonte e sincroniza todos clientes"
---

Invoca a skill `create-skill` para gerar uma nova skill a partir de uma proposta OpenSpec.

Uso:
```
/create-skill <nome> [--desc "descrição"] [--force]
```

Exemplos:
```
/create-skill minha-skill
/create-skill minha-skill --desc "Descrição da skill"
/create-skill minha-skill --force
```

O comando:
1. Cria mudança OpenSpec se não existir
2. Gera artefatos (proposal.md, design.md, tasks.md)
3. Cria skill em `.agent-source/skills/<nome>/SKILL.md` com material OpenSpec
4. Executa `scripts/sync-agent-configs.py` para sincronizar clientes
5. Valida cópias nos clientes `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`