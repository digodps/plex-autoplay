# setup-inicial Specification (delta)

## MODIFIED Requirements

### Requirement: Setup inicial coleta credenciais e configuração do Plex

O sistema SHALL, quando não houver credencial de operador configurada, aceitar login
com a senha padrão `123456` (exibida na tela de login) e então exigir um fluxo de
primeiro acesso que colete nova senha, URL do Plex, token do Plex e ao menos um
diretório de mídia antes de liberar o restante da aplicação. O token de setup MUST NOT
ser exigido. `POST /api/auth/setup` SHALL exigir JWT (obtido no login com a senha
padrão) e SHALL criar a credencial do operador ao final.

#### Scenario: Login com senha padrão no primeiro acesso

- **WHEN** não existe credencial e o operador submete `123456` em `POST /auth/login`
- **THEN** o sistema retorna um JWT e `forcePasswordChange: true`

#### Scenario: Setup concluído com dados válidos

- **WHEN** o operador autenticado envia nova senha que atende a política, URL do Plex
  acessível com o token informado e ao menos um diretório existente
- **THEN** o sistema valida tudo, persiste `PLEX_URL`, `PLEX_TOKEN` e `MEDIA_ROOTS`,
  cria a credencial e responde `204`

#### Scenario: URL ou token do Plex inválidos

- **WHEN** `GET /identity` do Plex falha ou excede 10 segundos com URL/token informados
- **THEN** o sistema responde `400` com erro prefixado `plex_url:` ou `plex_token:` e
  não cria credencial nem persiste configuração

#### Scenario: Setup sem JWT

- **WHEN** `POST /api/auth/setup` é chamado sem JWT válido
- **THEN** o sistema responde `401` e não cria credencial

#### Scenario: Setup já concluído

- **WHEN** já existe credencial de operador
- **THEN** o sistema responde `409` e não altera a credencial existente

#### Scenario: Senha padrão deixa de valer após o setup

- **WHEN** o setup é concluído com uma nova senha
- **THEN** `123456` deixa de autenticar e somente a nova senha é aceita

### Requirement: Interface de primeiro acesso

O sistema SHALL apresentar a página `/setup` como fluxo sequencial de três passos —
(1) nova senha e confirmação, (2) URL e token do Plex, (3) diretórios de mídia — com
navegação para voltar e avançar, exibindo os erros retornados pelo backend sem perder
os valores preenchidos. O passo 1 MUST NOT solicitar token de setup.

#### Scenario: Erro de validação exibido

- **WHEN** o backend responde `400` com `errors`
- **THEN** a página mostra cada erro e mantém URL, listas de diretórios e demais campos
  preenchidos

#### Scenario: Teste de conexão no passo do Plex

- **WHEN** o operador aciona "Testar conexão" no passo 2
- **THEN** a página chama `POST /api/plex/identity` e mostra sucesso ou mensagem de erro
  do Plex

#### Scenario: Setup concluído entra autenticado

- **WHEN** o backend responde `204`
- **THEN** a página autentica com a senha recém-criada e abre o dashboard sem passar
  pela tela de login

#### Scenario: Setup já concluído antes do último passo

- **WHEN** o backend responde `409`
- **THEN** a página redireciona para `/login`
