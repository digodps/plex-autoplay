# Spec Delta

## Purpose

Define o comportamento do Agente 5 (Repairer) que detecta falhas de testes ou code review, tenta auto-reparo de issues comuns (formatação, estilo) e cria specs de correção para issues que requerem intervenção humana.

## ADDED Requirements

### Requirement: Agente de reparo deve ser acionado quando testes ou code review falham

O sistema SHALL acionar o Agente 5 automaticamente quando o Agente 3 (Tester) ou Agente 4 (Reviewer) reporta falhas, com prioridade para issues que podem ser auto-corretas.

#### Scenario: Falha de testes aciona agente de reparo

- **WHEN** o Agente 3 (Tester) relatar falha de teste unitário
- **THEN** o Agente 5 é acionado para tentar auto-reparo

### Requirement: Agente de reparo deve tentar auto-reparo de issues comuns

O sistema SHALL tentar auto-reparo para issues comuns como formatação incorreta de código (`dotnet-format`, `ESLint`, `Prettier`), formatação de commit não convencional e outras violações de lint automáticas.

#### Scenario: Auto-reparo de formatação .NET

- **WHEN** há falhas de formatação `.cs` no working tree
- **THEN** o Agente 5 tenta `dotnet format` e corrige as diferenças automaticamente

#### Scenario: Auto-reparo de ESLint

- **WHEN** o Agente 2 (Frontend) reporta falhas de ESLint
- **THEN** o Agente 5 tenta `npx eslint --fix` e corrige as diferenças

#### Scenario: Auto-reparo de Prettier

- **WHEN** o Agente 2 (Frontend) reporta falhas de Prettier
- **THEN** o Agente 5 tenta `npx prettier --write` e corrige as diferenças

#### Scenario: Auto-reparo de descrição de commit

- **WHEN** o commit-skill.py reporta padrão de commit inválido
- **THEN** o Agente 5 tenta gerar descrição de commit semântica padrão

### Requirement: Agente de reparo deve criar specs de correção para issues manuais

O sistema SHALL criar uma change OpenSpec completa para issues que requerem decisão humana, usando o mesmo template que o code-review-skill.py (proposal.md, design.md, specs/<capability>/spec.md, tasks.md).

#### Scenario: Criação de change de reparo para issue de segurança

- **WHEN** o Agente 3 reporta falha de segurança (ex: CORS não configurado)
- **THEN** o Agente 5 cria change OpenSpec com specs de correção para o desenvolvedor corrigir manualmente

### Requirement: Agente de reparo deve executar reparos auto-corretos antes de criar specs

O sistema SHALL executar auto-repariáveis antes de criar specs para o mesmo item, seguindo a regra: se (então auto-reparo possível) → auto-reparar, else (criar specs).

#### Scenario: Auto-reparo de formatação, então specs para issue de segurança

- **WHEN** há falha de formatação e falha de segurança no mesmo commit
- **THEN** o Agente 5 primeiro auto-repara a formatação, então cria specs apenas para a falha de segurança

### Requirement: Agente de reparo deve ser parte do fluxo de validação do dev-spec

O sistema SHALL incluir o Agente 5 entre o Agente 4 (Reviewer) e Agente 3 (Tester) para garantir que issues sejam corrigidas antes dos testes executarem.

#### Scenario: Fluxo de validação completo

- **WHEN** o Agente 4 (Reviewer) completa a revisão formal e reporta achados
- **THEN** o Agente 5 aciona para auto-reparo, depois o Agente 4 sinaliza conclusão, depois o Agente 5 cria specs para issues manuais
- **CONTINUA** para o Agente 3 (Tester) somente após o Agente 5 finalizar todas as reparações e criação de specs