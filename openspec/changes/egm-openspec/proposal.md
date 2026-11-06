# Proposal: egm-openspec

Skill de orquestração que implementa uma mudança OpenSpec em loop verificado.

## Problema

Implementações multiagente divergem do spec, e agentes que falharam no QA tendem a
repetir o mesmo raciocínio quando reutilizados. Falta um fluxo único que:

1. valide os requisitos antes de codar (EGM);
2. implemente via `/opsx-apply` com skills de dev do Antigravity/AG Kit;
3. separe implementação de QA em agentes distintos;
4. recrie agentes limpos para cada correção, até o QA passar.

## Solução

Nova skill `egm-openspec` que:

- roda EGM (Goldfish Review A/S/C) sobre requisitos/spec/design antes da implementação;
- usa `openspec instructions apply` como fonte de verdade e atualiza `tasks.md`;
- orquestra agentes de implementação que carregam skills de dev do AG Kit;
- aciona um QA independente que valida o resultado contra o spec gerado;
- em caso de falha, descarta o contexto dos agentes e cria agentes limpos com apenas
  spec + achados do QA, repetindo até PASS (com limite de iterações).

## Escopo

- Skill global em `~/.config/opencode/skills/egm-openspec/SKILL.md`.
- Comando global em `~/.config/opencode/commands/egm-openspec.md`.
- Sem alteração de código de aplicação nesta mudança.

## Fora de escopo

- Alterar o comportamento do CLI OpenSpec.
- Commits ou push automáticos (a skill para no PASS e reporta).
