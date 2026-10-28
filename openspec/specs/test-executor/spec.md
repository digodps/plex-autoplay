# test-executor Specification

## Purpose
Define o comportamento do executor de testes que orquestra a validação do `.env`, a subida do container da aplicação com os dados do `.env`, a execução dos testes e2e dentro do Docker, a captura de artefatos de falha e a notificação local ao desenvolvedor.

## Requirements

### Requirement: Validar .env antes de executar testes
O sistema SHALL verificar a existência e o preenchimento das variáveis obrigatórias do `.env` antes de iniciar qualquer execução de teste.

#### Scenario: .env válido
- **WHEN** o arquivo `.env` existe com PLEX_URL, PLEX_TOKEN, JWT_SECRET, ADMIN_PASSWORD e MEDIA_ROOT preenchidos
- **THEN** o executor prossegue para a próxima etapa

#### Scenario: .env ausente
- **WHEN** o arquivo `.env` não existe na raiz do projeto
- **THEN** o executor falha com mensagem clara indicando que o `.env` está ausente e sugere copiar de `.env.example`

#### Scenario: Variável obrigatória faltando
- **WHEN** o `.env` existe mas uma variável obrigatória está vazia
- **THEN** o executor falha listando exatamente quais variáveis estão faltando

### Requirement: Subir container da aplicação com dados do .env
O sistema SHALL iniciar o container da aplicação usando `docker compose -f docker-compose.test.yml up --build`, injetando as variáveis do `.env`.

#### Scenario: Container inicia com sucesso
- **WHEN** o comando de compose up é executado com o `.env` carregado
- **THEN** o container fica disponível nas portas configuradas (8080 para API/SPA, 8081 para WebSocket)

#### Scenario: Falha ao iniciar container
- **WHEN** o Docker não está disponível ou o build da imagem falha
- **THEN** o executor aborta com mensagem de erro indicando a causa

### Requirement: Executar testes e2e dentro do Docker
O sistema SHALL rodar os testes e2e (Playwright) dentro do container do executor, contra a aplicação já em execução.

#### Scenario: Testes e2e passam
- **WHEN** todos os testes e2e são executados com sucesso
- **THEN** o executor sai com código zero

#### Scenario: Testes e2e falham
- **WHEN** pelo menos um teste e2e falha
- **THEN** o executor captura artefatos de falha e sai com código diferente de zero

### Requirement: Capturar artefatos de falha
O sistema SHALL salvar, em caso de falha, screenshots, traces, `playwright-report/`, `test-results/` e os `docker logs` do container da aplicação em uma pasta acessível ao desenvolvedor.

#### Scenario: Falha gera artefatos
- **WHEN** um teste e2e falha
- **THEN** os artefatos de falha são salvos em `./test-artifacts/` e o caminho é impresso no terminal

### Requirement: Derrubar container após execução
O sistema SHALL parar e remover o container da aplicação (`docker compose down`) após a conclusão dos testes, independentemente de sucesso ou falha.

#### Scenario: Container derrubado após sucesso
- **WHEN** os testes e2e terminam com sucesso
- **THEN** `docker compose down` é executado

#### Scenario: Container derrubado após falha
- **WHEN** os testes e2e terminam com falha
- **THEN** `docker compose down` é executado mesmo assim

### Requirement: Notificar localmente em caso de falha
O sistema SHALL notificar o desenvolvedor localmente, imprimindo no terminal o caminho dos artefatos de falha e o código de saída.

#### Scenario: Notificação local em falha
- **WHEN** os testes falham localmente
- **THEN** o terminal exibe a mensagem de falha com o caminho para `./test-artifacts/`

### Requirement: Suporte a execução local
O sistema SHALL ser executável localmente sem depender de infraestrutura de CI.

#### Scenario: Execução local
- **WHEN** o desenvolvedor roda o executor na máquina local com Docker disponível
- **THEN** o ciclo completo (validar .env, subir container, rodar e2e, capturar falha, derrubar container) é executado localmente

### Requirement: Protótipo para GitHub Actions com Plex mockado
O sistema SHALL ter um protótipo de workflow do GitHub Actions que roda o mesmo executor, com o Plex Media Server mockado (sem depender de um Plex real).

#### Scenario: CI com Plex mockado
- **WHEN** o workflow do GitHub Actions é disparado
- **THEN** o executor roda com um serviço Plex mock respondendo às rotas consumidas pelos testes e2e
