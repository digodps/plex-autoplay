# Change Proposal — alteracao-senha-usuario

## Why

A senha do painel só pode ser trocada editando `ADMIN_PASSWORD` no `.env` e
reiniciando o container. O header ainda expõe um botão de configurações cujo
modal tem um formulário de senha que não chama nenhuma API. O usuário quer
eliminar `ADMIN_PASSWORD`, criar a senha do operador no primeiro acesso e
alterá-la depois pela interface, usando sempre a credencial persistida no
SQLite.

## What Changes

- **BREAKING**: remove `ADMIN_PASSWORD` do runtime, `.env`, compose, scripts,
  CI e documentação.
- No primeiro start, quando não existe credencial no SQLite, o backend gera um
  token de setup e o escreve no log do container com instruções de uso.
- Nova tela `/setup` solicita o token (informando que ele está no log do
  container), a nova senha e a confirmação; ao concluir, cria a senha do
  operador no SQLite e invalida o token.
- Enquanto o setup não for concluído, rotas internas redirecionam para
  `/setup`; depois do setup, `/setup` redireciona para `/login`.
- Login valida exclusivamente o hash de senha persistido no SQLite.
- Remove o botão de configurações (engrenagem) do header.
- Adiciona **Alterar senha** no menu do usuário, abrindo um modal com nova
  senha e confirmação; o endpoint autenticado persiste a troca no SQLite.
- Adiciona migração da tabela de credencial administrativa e hash PBKDF2.
- Atualiza a spec `plex-auth` e adiciona cobertura automatizada.

## Capabilities

### New Capabilities

- `password-management`: setup inicial com token emitido no log, criação e
  troca de senha do operador persistidas no SQLite.

### Modified Capabilities

- `plex-auth`: login deixa de usar `ADMIN_PASSWORD` e passa a validar somente
  a credencial persistida no SQLite.

## Impact

- Backend: `AuthEndpoints`, `LoginUseCase`, setup token service, caso de uso e
  porta de credencial, repositório SQLite, migração em `DatabaseInitializer`,
  `Program.cs` e DI.
- Frontend: nova `SetupView`, `PasswordModal`, guarda de rotas, `AppHeader.vue`
  e `api.js`.
- Configuração: remoção de `ADMIN_PASSWORD` de `.env.example`, compose, CI,
  scripts, README e testes e2e.
- Banco: nova tabela `AuthCredential`.
- Sem novas dependências; PBKDF2 da biblioteca padrão do .NET.
