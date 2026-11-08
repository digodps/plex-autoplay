# Spec Delta — password-management

## ADDED Requirements

### Requirement: Setup token emitted on first start

Quando não existe credencial administrativa persistida, o sistema SHALL gerar
um token de setup aleatório no start do backend e SHALL escrevê-lo no log do
container com instruções de uso e marcador `SETUP_TOKEN=`. O token SHALL ser
**de uso único**: SHALL ser invalidado imediatamente após a primeira conclusão
bem-sucedida do setup e MUST NOT ser aceito novamente.

#### Scenario: Primeiro start sem credencial

- **WHEN** o backend inicia e não existe registro em `AuthCredential`
- **THEN** gera um token aleatório
- **AND** registra no log do container uma mensagem contendo `SETUP_TOKEN=<token>`
- **AND** informa que o token deve ser usado em `/setup`

#### Scenario: Start após setup concluído

- **WHEN** o backend inicia e já existe registro em `AuthCredential`
- **THEN** nenhum token de setup é gerado ou registrado no log

#### Scenario: Reinício antes do setup

- **WHEN** o backend reinicia sem credencial persistida
- **THEN** gera um novo token e o registra no log
- **AND** o token anterior não é mais aceito

### Requirement: Setup screen creates operator password

O sistema SHALL oferecer a rota pública `/setup` com campos de token, nova
senha e confirmação. A tela SHALL informar que o token está no log do
container. Enquanto o setup não for concluído, as rotas internas SHALL
redirecionar para `/setup`. Após a conclusão, `/setup` SHALL redirecionar para
`/login`.

#### Scenario: Acesso sem setup concluído

- **WHEN** o usuário acessa uma rota interna e `setupRequired` é `true`
- **THEN** é redirecionado para `/setup`

#### Scenario: Aviso do token no log

- **WHEN** o usuário abre `/setup`
- **THEN** a tela exibe instrução de que o token está no log do container
- **AND** sugere o comando `docker logs <container>` para obtê-lo

#### Scenario: Setup já concluído

- **WHEN** o usuário acessa `/setup` e `setupRequired` é `false`
- **THEN** é redirecionado para `/login`

### Requirement: Setup endpoint validates token and persists password

`POST /auth/setup` SHALL aceitar `{ token, newPassword }`, validar o token em
comparação de tempo constante, validar a senha entre 8 e 128 caracteres,
persistir somente o hash PBKDF2 da senha e invalidar o token de setup. O
endpoint SHALL ser público apenas enquanto não existir credencial.

#### Scenario: Setup válido

- **WHEN** o setup recebe o token emitido e uma senha válida
- **THEN** grava `AuthCredential` com hash PBKDF2 e salt aleatório
- **AND** retorna `204`
- **AND** o token deixa de ser aceito

#### Scenario: Token inválido

- **WHEN** o setup recebe token diferente do emitido
- **THEN** retorna `401`
- **AND** nenhuma credencial é gravada

#### Scenario: Senha inválida

- **WHEN** o setup recebe senha com menos de 8 ou mais de 128 caracteres
- **THEN** retorna `400`
- **AND** nenhuma credencial é gravada

#### Scenario: Setup repetido

- **WHEN** o setup é chamado após já existir credencial
- **THEN** retorna `409`

#### Scenario: Reutilização do token após sucesso

- **WHEN** o token de setup já foi usado com sucesso uma vez
- **THEN** ele não é mais aceito por `POST /api/auth/setup`
- **AND** retorna `409` (setup já concluído) ou `401` (token inválido)

### Requirement: Authenticated password change from user menu

O sistema SHALL remover o botão de configurações do header e SHALL oferecer a
opção **Alterar senha** no menu do usuário. A opção SHALL abrir um modal com
nova senha e confirmação. `PUT /api/auth/password` SHALL exigir JWT válido,
validar a senha entre 8 e 128 caracteres e atualizar o hash persistido.

#### Scenario: Alteração bem-sucedida

- **WHEN** um usuário autenticado envia uma nova senha válida
- **THEN** o hash em `AuthCredential` é atualizado
- **AND** retorna `204`
- **AND** o login seguinte aceita somente a nova senha

#### Scenario: Confirmação divergente na UI

- **WHEN** nova senha e confirmação diferem no modal
- **THEN** o modal exibe erro
- **AND** não chama a API

#### Scenario: Requisição sem autenticação

- **WHEN** `PUT /api/auth/password` é chamado sem JWT válido
- **THEN** retorna `401`

#### Scenario: Botão de configurações removido

- **WHEN** o header é renderizado
- **THEN** não existe botão com rótulo `Configurações`
- **AND** o menu do usuário contém `Alterar senha`

### Requirement: Password stored as PBKDF2 hash

O sistema SHALL persistir a senha apenas como hash PBKDF2-SHA256 com salt
aleatório por senha e SHALL comparar hashes em tempo constante. A senha em
texto puro MUST NOT ser gravada no banco nem registrada em log.

#### Scenario: Senha não aparece no banco

- **WHEN** a credencial é criada ou alterada
- **THEN** `AuthCredential.PasswordHash` contém `pbkdf2-sha256$...`
- **AND** não contém a senha em texto puro

#### Scenario: Salt distinto

- **WHEN** a mesma senha é gravada duas vezes
- **THEN** os hashes resultantes são diferentes
