# setup-inicial Specification (delta)

## REMOVED Requirements

### Requirement: Setup inicial coleta credenciais e configuração do Plex

**Reason**: O fluxo de primeiro acesso deixa de exigir token de setup. O operador faz login com a senha padrão `123456` (exibida na tela de login), é forçado a trocar a senha e depois configura Plex e mídia no mesmo fluxo `/setup`.

**Migration**: A configuração de Plex e mídia permanece no `/setup`; a única mudança é a remoção do token e a introdução da senha padrão no login.

## ADDED Requirements

### Requirement: Setup inicial por senha padrão

O sistema SHALL, quando não houver credencial de operador configurada, aceitar login com a senha padrão `123456` (exibida na tela de login), emitir JWT com `forcePasswordChange: true`, e exigir que o operador autenticado conclua o fluxo `/setup` trocando a senha, informando URL/token do Plex e diretórios de mídia antes de liberar o dashboard.

#### Scenario: Login com senha padrão no primeiro acesso

- **WHEN** não existe credencial e o operador submete `123456` em `POST /auth/login`
- **THEN** o sistema retorna um JWT e `forcePasswordChange: true`

#### Scenario: Setup concluído com dados válidos

- **WHEN** o operador autenticado envia nova senha que atende à política, URL do Plex acessível com o token informado e ao menos um diretório existente
- **THEN** o sistema valida tudo, persiste `PLEX_URL`, `PLEX_TOKEN` e `MEDIA_ROOTS`, cria a credencial e responde `204`

#### Scenario: URL ou token do Plex inválidos

- **WHEN** `GET /identity` do Plex falha ou excede 10 segundos com a URL/token informados
- **THEN** o sistema responde `400` com erro prefixado `plex_url:` ou `plex_token:` e não cria credencial nem persiste configuração

#### Scenario: Esquema de URL não suportado

- **WHEN** a URL do Plex não é `http://` nem `https://` absoluta
- **THEN** o sistema responde `400` com `plex_url: deve ser uma URL absoluta http:// ou https://`

#### Scenario: Nenhum diretório de mídia válido

- **WHEN** a lista de diretórios está vazia ou contém caminho inexistente/relativo
- **THEN** o sistema responde `400` listando os caminhos inválidos e não cria credencial

#### Scenario: Falha ao criar credencial

- **WHEN** a gravação da configuração do Plex/mídia teve sucesso mas a criação da credencial falha
- **THEN** o setup continua disponível para nova tentativa e o login permanece bloqueado

#### Scenario: Setup sem JWT

- **WHEN** `POST /api/auth/setup` é chamado sem JWT válido
- **THEN** o sistema responde `401` e não cria credencial

#### Scenario: Setup já concluído

- **WHEN** já existe credencial de operador
- **THEN** o sistema responde `409` e não altera a credencial existente

#### Scenario: Senha padrão deixa de valer após o setup

- **WHEN** o setup é concluído com uma nova senha
- **THEN** `123456` deixa de autenticar e somente a nova senha é aceita

## MODIFIED Requirements

### Requirement: Interface de primeiro acesso

O sistema SHALL apresentar a página `/setup` como fluxo sequencial de três passos — (1) nova senha e confirmação, (2) URL e token do Plex, (3) diretórios de mídia — com navegação para voltar e avançar, exibindo os erros retornados pelo backend sem perder os valores preenchidos. O passo 1 MUST NOT solicitar token de setup. A tela de login SHALL exibir "Senha padrão: 123456" enquanto o setup estiver pendente.

#### Scenario: Erro de validação exibido

- **WHEN** o backend responde `400` com `errors`
- **THEN** a página mostra cada erro e mantém URL, listas de diretórios e demais campos preenchidos

#### Scenario: Teste de conexão no passo do Plex

- **WHEN** o operador aciona "Testar conexão" no passo 2
- **THEN** a página chama `POST /api/plex/identity` sem sessão (permitido enquanto o setup está pendente) e mostra sucesso ou a mensagem de erro do Plex

#### Scenario: Setup concluído entra autenticado

- **WHEN** o backend responde `204`
- **THEN** a página autentica com a senha recém-criada e abre o dashboard sem passar pela tela de login

#### Scenario: Setup já concluído antes do último passo

- **WHEN** o backend responde `409`
- **THEN** a página redireciona para `/login`

#### Scenario: Diretório escolhido pelo navegador antes de existir sessão

- **WHEN** o operador abre o seletor de diretórios durante o setup
- **THEN** a listagem funciona mesmo sem sessão autenticada