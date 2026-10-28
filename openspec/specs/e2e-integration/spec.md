# e2e-integration Specification

## Purpose

Define o comportamento dos testes de integração de ponta a ponta que validam a stack completa (Docker, .NET API, Vue SPA e Playwright) antes do archive da mudança.

## Requirements

### Requirement: Carregar variáveis de ambiente do .env
O sistema SHALL ler as variáveis de configuração do arquivo `.env` antes de iniciar os testes e2e.

#### Scenario: .env presente e válido
- **WHEN** o arquivo `.env` existe na raiz do projeto com PLEX_URL, PLEX_TOKEN, JWT_SECRET e ADMIN_PASSWORD definidos
- **THEN** os testes usam essas variáveis para configurar a URL base, token de autenticação e senha de administrador

#### Scenario: .env ausente ou variável obrigatória faltando
- **WHEN** o arquivo `.env` não existe ou uma variável obrigatória está vazia
- **THEN** os testes falham com mensagem clara indicando qual variável está ausente

### Requirement: Iniciar container Docker antes dos testes
O sistema SHALL iniciar o container Docker usando `docker compose up` antes de executar os testes e2e.

#### Scenario: Container inicia com sucesso
- **WHEN** o comando `docker compose up --build` é executado com o `.env` carregado
- **THEN** o container fica disponível nas portas configuradas (8080 para API/SPA, 8081 para WebSocket)

#### Scenario: Falha ao iniciar container
- **WHEN** o Docker não está disponível ou o build da imagem falha
- **THEN** os testes são abortados com mensagem de erro indicando a causa

### Requirement: Autenticar via JWT na Vue SPA
O sistema SHALL fazer login na aplicação Vue usando a senha do `.env` e obter um token JWT.

#### Scenario: Login bem-sucedido
- **WHEN** o teste acessa a página de login e envia a `ADMIN_PASSWORD` correta
- **THEN** a aplicação retorna um token JWT e redireciona para o dashboard

#### Scenario: Login com senha incorreta
- **WHEN** o teste envia uma senha incorreta na página de login
- **THEN** a aplicação retorna HTTP 401 e exibe mensagem de erro

### Requirement: Navegar e validar bibliotecas de séries
O sistema SHALL acessar a página de bibliotecas e validar que apenas bibliotecas do tipo "show" são exibidas.

#### Scenario: Bibliotecas de séries carregam
- **WHEN** o usuário autenticado acessa `/libraries`
- **THEN** a página exibe apenas bibliotecas com `type="show"` (filmes são excluídos)

#### Scenario: Plex indisponível
- **WHEN** o Plex Media Server não está acessível na `PLEX_URL` configurada
- **THEN** a aplicação exibe erro indicando que o servidor está indisponível

### Requirement: Criar, editar e remover marcadores
O sistema SHALL permitir criar, editar e remover marcadores de intro/créditos via interface e validar persistência.

#### Scenario: Criar marcador manual
- **WHEN** o usuário preenche o formulário com rating_key, kind, start_ms e end_ms e submete
- **THEN** o marcador é salvo no SQLite e escrito no `custom.json`

#### Scenario: Remover marcador
- **WHEN** o usuário remove um marcador existente
- **THEN** o marcador é deletado do SQLite e removido do `custom.json`

### Requirement: Validar proxy de imagens do Plex
O sistema SHALL requisitar imagens através do proxy da API e validar que o token Plex não é exposto.

#### Scenario: Imagem servida via proxy
- **WHEN** o frontend requisita `GET /api/images/thumb/{rating_key}`
- **THEN** o proxy adiciona `X-Plex-Token` server-side e retorna a imagem com Content-Type correto

#### Scenario: Token não exposto ao navegador
- **WHEN** o inspetor de rede do navegador analisa requisições para `/api/images/`
- **THEN** o valor de `PLEX_TOKEN` não aparece em cabeçalhos ou URLs

### Requirement: Encerrar container após os testes
O sistema SHALL parar e remover o container Docker após a conclusão dos testes e2e.

#### Scenario: Container parado com sucesso
- **WHEN** todos os testes e2e terminam (com sucesso ou falha)
- **THEN** `docker compose down` é executado para liberar recursos
