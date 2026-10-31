# Proposal: Evoluir Skill de Desenvolvimento

## Why

O atual workflow multi-agente (`dev-spec.py`) tem três agentes (Backend, Frontend, Tester) mas **falta uma camada dedicada de Code Review e Reparo** antes dos testes serem executados. Isso causa retrabalho quando achados de lint/segurança/SOLID são descobertos tarde, após os testes terem começado. O objetivo é evoluir a skill para incluir agentes especializados que previnem erros e mantêm a qualidade de forma proativa, eliminando a necessidade de correções em fase avançada.

## What Changes

- **Adicionar Agente 4 (Reviewer)**: Executa a `code-review-skill.py` formal antes dos testes, apresenta achados interativamente e cria specs de correção.
- **Adicionar Agente 5 (Repairer)**: Tenta auto-fix para issues comuns (formatação, estilo) e cria specs de reparo quando intervenção manual é necessária.
- **Enriquecer o Tester (Agente 3)**: Incluir etapa de validação manual no navegador dentro do distrobox antes de relatar sucesso.
- **Criar DEVELOPMENT_GUIDE.md**: Guia de padrões proativos baseado exatamente nas validações da Code Review, para que desenvolvedores evitem erros antes de commitar.
- **Atualizar todos os arquivos de skill/documentation**: Inserir referências ao novo guia e fluxo atualizado em todos os markdowns (.opencode, .agents).

## Capabilities

### New Capabilities

- **development-guide**: Guia de padrões proativos (DEVELOPMENT_GUIDE.md) baseado nas heurísticas da Code Review, usado por todos os agentes durante implementação para prevenir erros antes de ocorrerem.
- **repair-agent**: Detecta falhas de testes e code review, tenta auto-reparo de issues comuns e cria specs de correção para issues que requerem intervenção humana.

### Modified Capabilities

- **multi-agent-dev**: O workflow principal passa de 3 para 5 agentes: adiciona Agente 4 (Reviewer - code review formal) e Agente 5 (Repairer - auto-reparo), além de etapa de validação visual manual no navegador dentro do distrobox no Agente 3 (Tester).
- **code-review**: Agora opera como agente dedicado no fluxo `/dev-spec`, com etapa interativa de seleção de achados antes dos testes, integrada ao portão de qualidade.

## Impact

- **Codebase**: Adição de novos arquivos (`DEVELOPMENT_GUIDE.md`, templates de repair skill).
- **Documentation**: Atualização de 6 arquivos markdown (.opencode/skills/dev-spec/SKILL.md, .opencode/commands/dev-spec.md, .opencode/dev-spec-skill.md, .agents/workflows/dev-spec.md, .agents/commands/dev-spec.md, .agents/dev-spec-skill.md).
- **Workflow**: Nova etapa "Code Review Formal" inserida entre implementation e testing, com checkpoint antes de tests E2E.
- **Developer Experience**: Zero retrabalho esperado se o DEVELOPMENT_GUIDE.md for seguido durante implementation; nova etapa de "manual test no browser" adiciona confirmação visual.
- **Riscos**: Se agentes não seguirem o guia, retrabalho persiste; curva de aprendizagem para novos agentes sobre o fluxo de 5 etapas.

---