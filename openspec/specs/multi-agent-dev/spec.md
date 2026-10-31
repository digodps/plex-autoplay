# multi-agent-dev Specification

## Purpose

Define o comportamento da orquestração de desenvolvimento multi-agente (`/dev-spec`), coordenando tarefas de backend (.NET), frontend (Vue 3), portão obrigatório de code review com auto-correção, validação de testes 100% via container Distrobox e publicação automatizada de Pull Request.

## Requirements

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

O sistema SHALL exigir que cada agente de implementação execute a revisão de código correspondente à sua stack e corrija 100% das violações antes de prosseguir para a fase de testes, e executar o Agente 4 (Reviewer) como etapa formal interativa antes dos testes.

#### Scenario: Code review do backend

- **WHEN** o Agente 1 conclui a implementação do backend
- **THEN** ele executa verificação de formatação e regras Clean Code/SOLID no .NET, corrigindo quaisquer desvios encontrados

#### Scenario: Code review do frontend

- **WHEN** o Agente 2 conclui a implementação do frontend
- **THEN** ele executa ESLint e Prettier nos arquivos JS/Vue, corrigindo quaisquer desvios encontrados

#### Scenario: Code Review formal (Agente 4)

- **WHEN** o Agente 1 e Agente 2 concluem a implementação e auto-correção
- **THEN** o Agente 4 executa a `code-review-skill.py` formal, exibe achados interativamente, e o desenvolvedor seleciona itens para correção antes de prosseguir

### Requirement: Validação integral de testes em container Distrobox

O sistema SHALL acionar o Agente 3 (Tester) somente após a conclusão limpa das etapas de backend, frontend e Code Review formal (Agente 4) com reparos (Agente 5), executando a suíte de testes unitários e a stack E2E Playwright dentro do container via Distrobox, e incluir etapa de validação visual manual no navegador.

#### Scenario: Execução completa com 100% de sucesso

- **WHEN** o Agente 3 executa os testes unitários e E2E Playwright dentro do Distrobox
- **THEN** todos os testes passam com zero falhas e o executor encerra com código de saída 0

#### Scenario: Falha de teste gera reteste

- **WHEN** um teste falha durante a execução
- **THEN** o sistema reporta o diagnóstico exato, aciona a correção pelo agente responsável e reexecuta os testes até atingir 100% de sucesso

#### Scenario: Validação visual manual no navegador

- **WHEN** todos os testes automatizados passam
- **THEN** o Agente 3 orienta o desenvolvedor a abrir a aplicação no navegador dentro do Distrobox para validar visualmente o que foi solicitado

### Requirement: Commits semânticos e abertura automatizada de Pull Request

O sistema SHALL agrupar as alterações por tarefa usando a convenção de commits semânticos com emojis e abrir automaticamente o Pull Request para validação final do usuário.

#### Scenario: Finalização e criação de PR

- **WHEN** todos os testes passam com 100% de sucesso e validação manual é aprovada
- **THEN** o sistema gera os commits mapeados por tarefa via `scripts/commit-skill.py`, faz push para a branch remota e executa `gh pr create` exibindo a URL do PR para revisão manual

### Requirement: Agente 5 (Repairer) para auto-reparo e specs de correção

O sistema SHALL incluir o Agente 5 (Repairer) que detecta falhas de testes ou code review, tenta auto-reparo de issues comuns e cria specs de correção para issues que requerem intervenção humana.

#### Scenario: Auto-reparo de formatação antes de specs

- **WHEN** há falhas de formatação após code review ou testes
- **THEN** o Agente 5 tenta auto-reparo (dotnet-format, eslint --fix, prettier --write) antes de criar specs

#### Scenario: Criação de specs de reparo para issues manuais

- **WHEN** há issues que requerem decisão humana (segurança, arquitetura)
- **THEN** o Agente 5 cria change OpenSpec com proposal.md, design.md, specs/, tasks.md para correção

### Requirement: Guia de desenvolvimento obrigatório durante implementação

O sistema SHALL exigir que todos os agentes de implementação (Backend, Frontend) consultem o DEVELOPMENT_GUIDE.md antes de iniciar qualquer implementação, garantindo que padrões de estilo, segurança e boas práticas sejam seguidos proativamente.

#### Scenario: Guia consultado antes de implementação

- **WHEN** o Agente 1 ou Agente 2 inicia uma task
- **THEN** eles consultam o DEVELOPMENT_GUIDE.md e aplicam as regras relevantes durante a implementação
