# Spec Delta — plex-auth

## MODIFIED Requirements

### Requirement: Login with single password

O sistema SHALL autenticar o usuário validando a senha submetida contra o hash
PBKDF2 persistido em `AuthCredential` no SQLite e SHALL emitir um JWT assinado
com `JWT_SECRET`. O sistema MUST NOT usar `ADMIN_PASSWORD` nem qualquer senha
padrão de ambiente. Quando não existe credencial persistida, o login SHALL
falhar com `401` e o setup inicial SHALL ser exigido.

#### Scenario: Successful login

- **WHEN** o usuário submete a senha correta em `POST /auth/login` e existe credencial no SQLite
- **THEN** o sistema retorna um JWT assinado com `JWT_SECRET`

#### Scenario: Failed login

- **WHEN** o usuário submete uma senha incorreta em `POST /auth/login`
- **THEN** o sistema retorna `401 Unauthorized`

#### Scenario: Login antes do setup

- **WHEN** o usuário submete qualquer senha em `POST /auth/login` e não existe credencial no SQLite
- **THEN** o sistema retorna `401 Unauthorized`

#### Scenario: Senha alterada

- **WHEN** o operador altera a senha com sucesso
- **THEN** logins seguintes aceitam apenas a senha nova
- **AND** a senha anterior é rejeitada

#### Scenario: ADMIN_PASSWORD ausente

- **WHEN** o container inicia sem a variável `ADMIN_PASSWORD`
- **THEN** o sistema autentica normalmente usando a credencial do SQLite
