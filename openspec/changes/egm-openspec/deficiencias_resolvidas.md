# Deficiências Resolvidas — EGM Goldfish Review (Fase 1)

## Revisor A — Arquitetura, dependências e integração

1. **Achado 1**: O fluxo de agentes efêmeros não prevê limpeza de contexto entre iterações de QA FAIL. Recomendado adicionar etapa de descarte explícito antes da recriação de agentes.
   - Evidência: `tasks.md` não tem task para limpeza de agentes anteriores; revisão de `openspec instructions apply` mostra ausência de `cleanup` no escopo.
   
2. **Achado 2**: A validação `openspec validate <change> --strict` é executada apenas na Fase 3 QA, mas não há garantia de execução prévia na Fase 1 EGM, o que pode permitir arquiteturas que não passariam no validate rígido.
   - Evidência: O `preflight` roda `openspec status` e `openspec instructions apply`, mas não `openspec validate --strict`.

3. **Achado 3**: Não há definição de diretório permitido (allowed edit roots) para os agentes implementadores, o que risco edições fora do escopo da change.
   - Evidência: `actionContext.constraints.allowedEditRoots` está vazio no JSON de apply instructions.

## Revisor S — SOLID e coesão

4. **Achado 4**: A skill `egm-openspec` mistura responsabilidades de orquestração e validação em mesma arquivo SKILL.md, violando princípio de separação de concerns (SOLID - Single Responsibility).
   - Evidência: O mesmo arquivo define fluxo EGM, apply instructions e guardrails; recomendado split em skill de fluxo e skill de validação.

5. **Achado 5**: O loop "até PASS" com limite de 5 iterações não valida se o problema é de escopo do spec vs limitação de implementação; pode mascarar falta de clareza nos requisitos.
   - Evidência: `guardrails` menciona "Não reduzir escopo do spec para fazer o QA passar" mas não há mecanismo automático de detecção.

6. **Achado 6**: Dependência circular implícita: QA valida `openspec validate --strict` mas a validação só passa se implementadores seguirem o design.md, que por sua vez foi gerado a partir do proposal — criando caminho fechado sem entrada externa.

## Revisor C — Clean Code, escopo e testabilidade

7. **Achado 7**: `tasks.md` tem 4 tasks marcadas como done, mas a task "Rodar `openspec validate egm-openspec --strict`" foi executada e passou, however a validação não cobre artefatos de EGM (deficiencias_resolvidas.md). Recomendado incluir validação desse arquivo no strict mode.
   - Evidência: `openspec validate` foca em schema e tasks, não em relatórios de revisão.

8. **Achado 8**: Commentários em `proposal.md` dizem "Sem alteração de código de aplicação nesta mudança" mas o fluxo EGM implica geração de código via agentes Antigravity. Recomendado atualizar escopo ou aclarar que skill é de orquestração apenas.
   - Evidência: `proposal.md` line "Fora de escopo: Alterar o comportamento do CLI OpenSpec."

9. **Achado 9**: Não há exemplo de relatório QA `qa-report-<n>.md` na change; o fluxo Fase 3 dita formato obrigatório mas nenhum exemplo existe para referência inicial.
   - Evidência: `openspec/changes/egm-openspec/` não contém arquivos `qa-report-*.md`.

---

### Classificação geral

- **Aceitos**: Achados 1, 2, 3, 5 (recomendações de melhoria processual).
- **Rejeitados**: Nenhum (todos relevantes).
- **Parciais**: Achado 4 (concernente a split de skill — pode ser adiado se não bloquear implementação).
- **Ação requerida**: Atualizar `tasks.md` com task de limpeza de agentes, adicionar validação de `deficiencias_resolvidas.md` no `openspec validate --strict`, e clarificar escopo do proposal.
