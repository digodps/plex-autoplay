# Design: egm-openspec

## Fluxo

```
Preflight → EGM (requisitos) → Apply (orquestrado) → QA → PASS? fim
                                      ↑                 │
                                      └── agentes limpos ← FAIL
```

## Papéis

| Papel | Responsabilidade | Contexto |
|---|---|---|
| Orquestrador | Lê apply instructions, divide trabalho, spawna agentes, centraliza `tasks.md` | Persistente na execução |
| Revisores EGM (A/S/C) | Achados numerados sobre requisitos/spec/design | Efêmero, 3 em paralelo |
| Implementadores | Implementam tarefas por domínio com skills de dev AG Kit | Efêmero por iteração |
| QA | Valida resultado contra spec, testes/lint/build | Efêmero, nunca o mesmo dos implementadores |
| Corretor | Corrige apenas achados do QA | Efêmero, recriado a cada FAIL |

## Fase 0 — Preflight

1. Determinar `<change>` (argumento explícito; senão única mudança ativa; senão perguntar).
2. `openspec status --change "<change>" --json`.
3. `openspec instructions apply --change "<change>" --json`.
4. Ler todos os `contextFiles`.
5. Se `state: blocked` ou `missingArtifacts` não vazio: parar e reportar.

## Fase 1 — EGM nos requisitos

Goldfish Review em paralelo, cada revisor lista achados numerados:

- **A**: arquitetura, dependências e integração.
- **S**: SOLID e coesão.
- **C**: Clean Code, escopo e testabilidade.

Cada achado é classificado como aceito, rejeitado (com motivo) ou parcial e
verificado com evidência (`arquivo:linha`, grep, build/teste). Registrar em
`openspec/changes/<change>/deficiencias_resolvidas.md`.

Achados aceitos que mudem requisito exigem atualizar os artefatos da change
(`proposal.md`, `specs/`, `design.md`) antes de implementar; depois reler
`openspec instructions apply`.

## Fase 2 — Implementação orquestrada

O orquestrador:

1. Divide as tarefas por domínio e dependência.
2. Spawna implementadores independentes (paralelo quando não houver conflito de arquivos).
3. Instrui cada implementador a carregar as skills de dev do AG Kit relevantes:
   `plan-writing`, `clean-code`, `tdd-workflow`, `verify-changes`,
   `systematic-debugging`, `testing-patterns`, `lint-and-validate`,
   `code-review-checklist` (frontend: `frontend-architecture`/`webapp-testing`).
4. Cada implementador recebe apenas: trecho do spec, design, tarefa e contrato de
   diretórios. Nunca recebe o relatório do QA de outra iteração.
5. Implementador só marca `- [x]` quando o comportamento especificado estiver completo.

Contrato de diretórios e comandos de build/teste vêm do `DEVELOPMENT_GUIDE.md`.

## Fase 3 — QA independente

QA recebe contexto limpo e valida:

1. Cada critério do spec (`specs/**` e `design.md`) tem implementação correspondente.
2. `tasks.md` reflete apenas trabalho realmente concluído.
3. Testes automatizados, lint/format e build passam.
4. `openspec validate <change> --strict` passa.
5. Execução real quando aplicável (skill `verify-changes`).

Saída obrigatória em `openspec/changes/<change>/qa-report-<n>.md`:
`PASS` ou `FAIL` + achados numerados com evidência e arquivo esperado.

## Fase 4 — Loop de correção

Se `FAIL`:

1. Descartar contexto dos implementadores anteriores.
2. Criar agentes limpos com apenas: spec, design, lista de achados do QA e diretório permitido.
3. Corrigir, rodar testes/lint/build, voltar à Fase 3.

Limite padrão: 5 iterações (configurável). Ao exceder, parar e reportar bloqueio
com histórico de QA, sem mascarar falha.

## Fase 5 — Encerramento

Com PASS: reportar arquivos alterados, relatório QA final, progresso de tarefas e
próximos passos (`/opsx-archive`, `/commit`). Não commitar automaticamente.

## Guardrails

- Orquestrador nunca aprova o próprio trabalho; QA é sempre agente separado.
- Agente que falhou nunca é reutilizado para corrigir; sempre recriado limpo.
- `tasks.md` é fonte de progresso; checkbox só após comportamento completo.
- Não reduzir escopo do spec para fazer o QA passar.
- Toda correção precisa de evidência de verificação.
- Segredos, credenciais e dados sensíveis nunca entram em relatórios ou prompts.
