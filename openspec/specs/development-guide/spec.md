# development-guide Specification

## Purpose
Define o comportamento do guia de desenvolvimento (DEVELOPMENT_GUIDE.md) que consolida todos os padrões de código, estilo, segurança e boas práticas que os agentes de desenvolvimento devem seguir proativamente para evitar erros comuns e garantir que o código passe na Code Review na primeira tentativa.

## Requirements

### Requirement: Guia de desenvolvimento deve existir e estar atualizado

O sistema SHALL manter o DEVELOPMENT_GUIDE.md atualizado com todas as regras, checklists e exemplos baseados nas validações da Code Review (`code-review-skill.py`). O guia deve ser consultado antes de qualquer implementação e atualizado quando a Code Review detectar novas regras ou quando houver mudanças significativas nos processos.

#### Scenario: Guia de desenvolvimento atualizado

- **WHEN** um desenvolvedor inicia qualquer task
- **THEN** o guia deve estar disponível na raiz do projeto e conter as regras atuais de formatação, estilo, segurança e boas práticas

### Requirement: Guia deve ser consultado durante implementação

O sistema SHALL garantir que o DEVELOPMENT_GUIDE.md seja consultado pelo desenvolvedor antes de commitar qualquer código, especialmente quando houver dúvidas sobre padrões de estilo, segurança ou boas práticas.

#### Scenario: Desenvolvedor consulta guia antes de commitar

- **WHEN** o desenvolvedor tem dúvida sobre padrão de commit ou estrutura de código
- **THEN** ele consulta o DEVELOPMENT_GUIDE.md e aplica as regras relevantes antes de commitar

### Requirement: Guia deve ser atualizado quando Code Review detectar novas regras

O sistema SHALL atualizar automaticamente o DEVELOPMENT_GUIDE.md quando a Code Review detectar novas regras ou heurísticas que não estejam documentadas, com notificação ao desenvolvedor.

#### Scenario: Guia atualizado com nova regra de lint

- **WHEN** a Code Review detecta uma nova verificação de estilo que não está no DEVELOPMENT_GUIDE.md
- **THEN** o guia é atualizado com a nova regra e o desenvolvedor recebe notificação para revisão

### Requirement: Guia deve conter checklists e exemplos práticos

O DEVELOPMENT_GUIDE.md SHALL conter checklists de validação para cada etapa do workflow e exemplos práticos de correção de erros comuns, referenciando as regras da Code Review.

#### Scenario: Checklist de validação antes de commit

- **WHEN** o desenvolvedor roda `python scripts/code-review-skill.py --dry-run`
- **THEN** o guia deve conter um checklist de validação pré-commit que o desenvolvedor pode usar para auto-verificação

### Requirement: Guia deve ser parte do fluxo de validação

O DEVELOPMENT_GUIDE.md SHALL ser mencionado como parte do fluxo de validação formal, com referência específica nos arquivos de skill e workflow.

#### Scenario: Guia referenciado no workflow

- **WHEN** o dev-spec.py executa a etapa de code review formal
- **THEN** o fluxo deve referenciar explicitamente o DEVELOPMENT_GUIDE.md como guia para os desenvolvedores seguirem durante a implementação
