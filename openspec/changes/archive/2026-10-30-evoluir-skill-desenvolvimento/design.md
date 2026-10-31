# Design: Evoluir Skill de Desenvolvimento

## Context

O workflow multi-agente atual (`dev-spec.py`) implementa o ciclo de vida de uma mudança OpenSpec com três agentes: Backend (implementa e roda code review), Frontend (implementa em background) e Tester (executa testes automatizados e E2E). O Code Review (`code-review-skill.py`) já realiza verificações completas (lint, OWASP, NIST, Clean Code/SOLID), mas não está integrado como etapa obrigatória antes dos testes, causando retrabalho quando achados são descobertos após o início dos testes.

Além disso, não existe guia de desenvolvimento proativo quepadronize o que deve ser seguido durante a implementação, levando a violações de padrões que poderiam ser evitadas.

## Goals

- Adicionar Agente 4 (Reviewer) como etapa formal antes dos testes, integrando code-review-skill.py com feedback interativo.
- Adicionar Agente 5 (Repairer) para auto-fix de issues comuns e criação de specs de reparo.
- Enriquecer Agente 3 (Tester) com validação manual no navegador.
- Criar DEVELOPMENT_GUIDE.md como guia de padrões proativos baseado nas validações da Code Review.
- Atualizar todos os arquivos de skill/documentation para referenciarem o novo fluxo.

## Non-Goals

- Não alterar a lógica existente de `dev-spec.py` de forma que quebra o fluxo atual sem o novo esquema.
- Não criar agentes totalmente novos - evoluir os existentes dentro do mesmo framework.
- Não remover nenhuma funcionalidade atual - apenas adicionar camadas.

## Decisions

### 1. Inserir etapa de Code Review Formal antes de Testes

**Decisão**: Adicionar Agente 4 (Reviewer) entre implementation e testing.

**Racional**: As verificações do `code-review-skill.py` (lint, estilo, segurança, SOLID) são valiosas, mas hoje ocorrem tardiamente ou são opcionais. Inserir como etapa obrigatória com checkpoint garante que o código está limpo antes dos testes rodarem, evitando falhas de teste causadas por problemas de formatação ou violações básicas.

**Alternativas consideradas**:
- Manter status quo: Risco de retrabalho após testes começarem.
- Auto-fix apenas: Não resolve questões que requerem decisão humana (ex: padrão de design, arquitetura).
- Escolha esta: Equilibra automação (lint auto-fix) com decisão humana (code review interativa).

### 2. Adicionar Agente 5 (Repairer) para auto-reparo

**Decisão**: Criar `repair-skill.py` que tenta auto-fix e gera specs quando necessário.

**Racional**: Quando o Tester (Agente 3) ou Reviewer (Agente 4) encontra falhas, ter um agente que tenta resolver o óbvio (formatação, console.log removidos, etc.) economiza tempo do desenvolvedor humano, que foca nas decisões complexas.

**Alternativas consideradas**:
- Deixar todo reparo para o humano: Ineficiente, muitos passos simples.
- Auto-fix total: Perde contexto, pode quebrar intenções.
- Escolha esta: Reparo guiado + specs para casos que requerem decisão humana.

### 3. Enriquecer Tester com validação manual no navegador

**Decisão**: O Agente 3 agora inclui instrução de abrir app no navegador antes de reportar sucesso.

**Racional**: O requisito original pede "teste manual logando na aplicação pelo navegador". Isso deve ser uma etapa obrigatória de sanity check, não apenas opcional.

**Alternativas consideradas**:
- Apenas testes automatizados (E2E Playwright): Não cobre todos os cenários visuais/UX.
- Documentar manual separado: Fácil de ser pulado/esquecido.
- Escolha esta: Integrar no fluxo do Agente 3 garante que acontece.

### 4. Criar DEVELOPMENT_GUIDE.md baseado no Code Review

**Decisão**: O guia deve consolidar exatamente o que `code-review-skill.py` valida.

**Racional**: Se o desenvolvedor seguir o guia durante implementation, zero achados na Code Review. Isso é a prevenção máxima.

**Alternativas consideradas**:
- Criar guia próprio: Risco de divergir das verificações da skill.
- Usar apenas a skill: Desenvolvedor não tem referência rápida durante coding.
- Escolha esta: Guia único, fonte de verdade única, mantido junto com a skill.

### 5. Atualizar todos os arquivos de skill/documentation

**Decisão**: Inserir aviso/reference ao DEVELOPMENT_GUIDE.md em todos os markdowns.

**Racional**: Consistência - qualquer um que use `/dev-spec` veja imediatamente o guia.

**Alternativas consideradas**:
- Deixar como is: Usuários descobrem o guia acidentalmente.
- Escolha esta: Visibilidade obrigatória na entrada de qualquer skill.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Agentes não seguirem o DEVELOPMENT_GUIDE.md | Documento visível em todos os pontos de entrada (`/dev-spec`); checklist no tasks.md |
| Nova etapa de Code Review atrasar delivery | Stage é rápida (lint + style auto-fix); apenas questões complexas vão para interação humana |
| Curva de aprendizagem para novos agentes | Guia próprio + aviso nos scripts; após 1 ciclo, padrão é internalizado |
| Repairer auto-fix quebrar intenções | Reparo guiado + specs de correção que humano revisa antes de aplicar |
| Validação manual no navegador ser pulada | Integrada ao Agente 3 como checkpoint obrigatório antes de reportar 100% sucesso |

## Migration Plan

1. Esta change `evoluir-skill-desenvolvimento` encapsula todas as alterações.
2. Após merge, novos branches devem usar o fluxo de 5 etapas.
3. Desenvolvedores devem consultar DEVELOPMENT_GUIDE.md antes de iniciar qualquer task.
4. O pipeline `dev-spec.py` será atualizado na próxima iteração para inserir as novas etapas automaticamente.
5. Validação: após merge, rodar `openspec validate --change evoluir-skill-desenvolvimento` para confirmar zero requisitos pendentes.

## Open Questions

- A stage de Code Review (Agente 4) será acionada automaticamente pelo `dev-spec.py` ou o desenvolvedor deve rodar `code-review-skill.py` manualmente antes?
  - *Decisão tomada: Será automática na próxima atualização de `dev-spec.py`, mas por enquanto o desenvolvedor pode rodar manualmente para testar.*
- O Repairer (Agente 5) tentará auto-fix em todos os achados ou apenas nos de "baixa severidade"?
  - *Decisão tomada: Apenas issues de lint/estilo (severity "info"/"warning"); issues de segurança/SOLID requerem interação humana via `--fix` no code-review-skill.py.*

---