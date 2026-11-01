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

**Racional**: O requisito original pede "teste manual logando na aplicação pelo navegador que estiver". Isso deve ser uma etapa obrigatória de sanity check, não apenas opcional.
---