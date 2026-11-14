# plex-auth Specification (delta)

## MODIFIED Requirements

### Requirement: Login with single password

O sistema SHALL autenticar o usuário validando a senha submetida contra o hash PBKDF2 persistido em `AuthCredential` no SQLite e SHALL emitir um JWT assinado com `JWT_SECRET`. O sistema MUST NOT usar `ADMIN_PASSWORD` nem qualquer senha padrão de ambiente. Quando não existe credencial persistida, o login SHALL aceitar apenas a senha padrão `123456` e retornar `forcePasswordChange: true`; qualquer outra senha SHALL falhar com `401`.

#### Scenario: Successful login

- **WHEN** o usuário submete a senha correta em `POST /auth/login` e existe credencial no SQLite
- **THEN** o sistema retorna um JWT assinado com `JWT_SECRET`

#### Scenario: Failed login

- **WHEN** o usuário submete uma senha incorreta em `POST /auth/login`
- **THEN** o sistema retorna `401 Unauthorized`

#### Scenario: Successful login with default password (first login)

- **WHEN** o usuário submete `123456` em `POST /auth/login` e não existe credencial no SQLite
- **THEN** o sistema retorna um JWT e `forcePasswordChange: true`

#### Scenario: Successful login with custom password

- **WHEN** o usuário submete uma senha diferente de `123456` em `POST /auth/login` e existe credencial no SQLite com hash correspondente
- **THEN** o sistema retorna um JWT e `forcePasswordChange: false` (ou ausente)

#### Scenario: Login antes do setup com senha diferente da padrão

- **WHEN** o usuário submete uma senha diferente de `123456` em `POST /auth/login` e não existe credencial no SQLite
- **THEN** o sistema retorna `401 Unauthorized`

#### Scenario: Senha alterada

- **WHEN** o operador altera a senha com sucesso
- **THEN** logins seguintes aceitam apenas a senha nova
- **AND** a senha anterior é rejeitada

#### Scenario: ADMIN_PASSWORD ausente

- **WHEN** o container inicia sem a variável `ADMIN_PASSWORD`
- **THEN** o sistema autentica normalmente usando a credencial do SQLite