# Spec Delta

## ADDED Requirements

### Requirement: Code Review integrado como Agente 4 no workflow /dev-spec

O sistema SHALL integrar a skill de Code Review (`code-review-skill.py`) como o Agente 4 (Reviewer) formal no workflow `/dev-spec`, executando a varredura e exibindo achados interativamente antes da execução dos testes.

#### Scenario: Execução do Agente 4 no dev-spec

- **WHEN** o Backend e Frontend concluem a implementação e auto-correção local
- **THEN** o sistema aciona o Agente 4 que executa a varredura completa e apresenta os achados ao desenvolvedor para seleção de correções

### Requirement: Integração do Agente 4 com o Agente 5 (Repairer)

O sistema SHALL permitir que o Agente 4 passe os achados ao Agente 5 (Repairer) para auto-reparo de issues de formatação e geração de specs para issues manuais.

#### Scenario: Passagem de achados para o Repairer

- **WHEN** o Agente 4 conclui a varredura e o usuário seleciona itens
- **THEN** o Agente 5 tenta auto-reparar o que for possível e gera specs para o restante antes de liberar o Agente 3 (Tester)