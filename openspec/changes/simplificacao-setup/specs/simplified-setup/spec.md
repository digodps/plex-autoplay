# Spec Delta — simplified-setup

## MODIFIED Requirements

### Requirement: Simplified initial setup (single step)

O setup inicial SHALL solicitar apenas: token de setup, nova senha (pré-preenchida com `123456`) e confirmação de senha. O sistema SHALL NÃO solicitar URL do Plex, token do Plex nem diretórios de mídia durante o setup.

#### Scenario: Setup page loads (no credential in SQLite)

- **WHEN** o usuário acessa `GET /setup` e não existe credencial no SQLite
- **THEN** a página exibe apenas: campo token, campo nova senha (valor padrão `123456`), campo confirmação
- **AND** NÃO exibe campos de Plex URL, Plex Token, nem Media Roots

#### Scenario: Successful setup with default password

- **WHEN** o usuário submete `POST /api/auth/setup` com token válido, `newPassword = "123456"` e `confirmPassword = "123456"`
- **THEN** o sistema cria a credencial com hash PBKDF2 da senha `123456`
- **AND** invalida o token de setup (uso único)
- **AND** retorna `204 No Content`
- **AND** o frontend faz login automático com a senha `123456`
- **AND** redireciona para `/change-password` (tela de alteração obrigatória)

#### Scenario: Successful setup with custom password

- **WHEN** o usuário submete `POST /api/auth/setup` com token válido, nova senha ≥ 8 chars e confirmação igual
- **THEN** o sistema cria a credencial com hash PBKDF2 da nova senha
- **AND** invalida o token de setup
- **AND** retorna `204 No Content`
- **AND** o frontend faz login automático com a nova senha
- **AND** redireciona para `/change-password`

#### Scenario: Setup with invalid token

- **WHEN** o usuário submete token inválido ou já utilizado
- **THEN** o sistema retorna `401 Unauthorized` com mensagem "Token de setup inválido ou já utilizado"
- **AND** mantém o usuário na tela de setup (passo 1)

#### Scenario: Setup with password mismatch

- **WHEN** o usuário submete nova senha e confirmação diferentes
- **THEN** o sistema retorna `400 Bad Request` com erro de validação
- **AND** mantém o usuário na tela de setup

#### Scenario: Setup with weak password (< 8 chars)

- **WHEN** o usuário submete nova senha com menos de 8 caracteres
- **THEN** o sistema retorna `400 Bad Request` com erro "A senha deve ter entre 8 e 128 caracteres"

#### Scenario: Setup already completed

- **WHEN** o usuário acessa `GET /setup` ou submete `POST /api/auth/setup` e já existe credencial no SQLite
- **THEN** o sistema retorna `409 Conflict` / redireciona para `/login`

### Requirement: Mandatory password change after first login

Após o primeiro login (seja com senha padrão `123456` ou senha customizada no setup), o usuário SHALL ser redirecionado para a tela de alteração de senha e NÃO PODE acessar outras áreas do sistema até concluir a troca.

#### Scenario: First login redirects to change-password

- **WHEN** o usuário faz login pela primeira vez (após setup)
- **THEN** o frontend redireciona para `/change-password`
- **AND** o usuário NÃO consegue acessar `/dashboard`, `/libraries`, etc. até trocar a senha

#### Scenario: Change password success

- **WHEN** o usuário submete `PUT /api/auth/password` com senha atual correta (`123456` ou a definida no setup), nova senha ≥ 8 chars e confirmação igual
- **THEN** o sistema atualiza o hash no SQLite
- **AND** retorna `204 No Content`
- **AND** o frontend redireciona para `/dashboard`

#### Scenario: Change password with wrong current password

- **WHEN** o usuário submete senha atual incorreta
- **THEN** o sistema retorna `403 Forbidden`

#### Scenario: Change password with weak new password

- **WHEN** o usuário submete nova senha < 8 chars ou > 128 chars
- **THEN** o sistema retorna `400 Bad Request`

## REMOVED Requirements

### Requirement: Plex configuration during setup (REMOVED)

- O sistema NÃO MAIS solicita URL do Plex, token do Plex, nem testa conexão com Plex durante o setup
- O sistema NÃO MAIS solicita diretórios de mídia durante o setup
- Essas configurações passam a ser feitas após login, na tela de Settings (fora do escopo desta change)

## UNCHANGED Requirements

- Token de setup: uso único, 32 bytes base64url, escrito no log do container, invalidado após setup
- PBKDF2 com salt por senha para hash no SQLite
- JWT stateless (tokens antigos continuam válidos após troca de senha)
- `ChangePasswordUseCase` exige senha atual + nova válida
- Guarda de navegação: `/setup` pública enquanto `setupRequired=true`; redireciona para `/login` se setup já concluído