# Spec Delta

## Purpose

Define o comportamento da orquestração de desenvolvimento multi-agente (`/dev-spec`), coordenando tarefas de backend (.NET), frontend (Vue 3), portão obrigatório de code review com auto-correção, validação de testes 100% via container e publicação automatizada de Pull Request.

## ADDED Requirements

### Requirement: Inicialização e isolamento de branch
O sistema SHALL identificar a mudança OpenSpec ativa ou informada via parâmetro e criar um branch isolado a partir do commit remoto limpo `origin/main` usando o padrão `feature/<mudanca>-<span>`.

#### Scenario: Branch criado com sucesso
- **WHEN** o usuário executa `/dev-spec` com uma mudança em andamento
- **THEN** o sistema executa o setup e deixa o repositório posicionado no branch `feature/<mudanca>-<span>` com tracking remoto configurado

### Requirement: Execução paralela de Backend e Frontend
O sistema SHALL distribuir tarefas de desenvolvimento entre dois agentes concorrentes: Agente 1 (Backend) para código C#/.NET e Agente 2 (Frontend) para código Vue 3/JavaScript.

#### Scenario: Backend e Frontend desenvolvem em paralelo
- **WHEN** as tarefas do `tasks.md` são identificadas
- **THEN** o Agente 1 implementa os requisitos de backend em `src/` enquanto o Agente 2 implementa os componentes e stores em `vue/` simultaneamente

### Requirement: Portão obrigatório de Code Review com auto-correção
O sistema SHALL exigir que cada agente de implementação execute a revisão de código correspondente à sua stack e corrija 100% das violações antes de prosseguir para a fase de testes.

#### Scenario: Code review do backend
- **WHEN** o Agente 1 conclui a implementação do backend
- **THEN** ele executa verificação de formatação e regras Clean Code/SOLID no .NET, corrigindo quaisquer desvios encontrados

#### Scenario: Code review do frontend
- **WHEN** o Agente 2 conclui a implementação do frontend
- **THEN** ele executa ESLint e Prettier nos arquivos JS/Vue, corrigindo quaisquer desvios encontrados

### Requirement: Validação integral de testes em container
O sistema SHALL acionar o Agente 3 (Tester) somente após a conclusão limpa das etapas de backend e frontend, executando a suíte de testes unitários e a stack E2E Playwright diretamente na máquina do dev.

#### Scenario: Execução completa com 100% de sucesso
- **WHEN** o Agente 3 executa os testes unitários e E2E Playwright
- **THEN** todos os testes passam com zero falhas e o executor encerra com código de saída 0

#### Scenario: Falha de teste gera reteste
- **WHEN** um teste falha durante a execução
- **THEN** o sistema reporta o diagnóstico exato, aciona a correção pelo agente responsável e reexecuta os testes até atingir 100% de sucesso

### Requirement: Commits semânticos e abertura automatizada de Pull Request
O sistema SHALL agrupar as alterações por tarefa usando a convenção de commits semânticos com emojis e abrir automaticamente o Pull Request para validação final do usuário.

#### Scenario: Finalização e criação de PR
- **WHEN** todos os testes passam com 100% de sucesso
- **THEN** o sistema gera os commits mapeados por tarefa via `scripts/commit-skill.py`, faz push para a branch remota e executa `gh pr create` exibindo a URL do PR para revisão manual
