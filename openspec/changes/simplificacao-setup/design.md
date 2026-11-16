# Design — simplificacao-setup

## Fluxo

```text
Primeiro start (sem credencial no SQLite)
→ SetupTokenService.EnsureGenerated()
composition root escreve no stdout do container:
============================================================
CONFIGURACAO INICIAL - TOKEN DE SETUP (USO UNICO)
SETUP_TOKEN=<base64url-aleatorio>
Informe este token em /setup para criar senha do operador.
O token deixa de valer apos primeira conclusao do setup.
============================================================

GET /api/auth/setup/status → { "setupRequired": true }
→ SPA redireciona para /setup

POST /api/auth/setup { token, newPassword }
→ CompleteSetupUseCase
1. credencial já existe? → 409
2. token inválido? → 401
3. senha fora de 8..128? → 400
4. Pbkdf2PasswordHasher.Hash + AuthCredential.CreateAsync
5. SetupTokenService invalidado → 204

POST /auth/login { password } (senha = "123456" ou a definida no setup)
→ LoginUseCase consulta AuthCredential no SQLite
├─ hash existe → PBKDF2 verify
└─ hash ausente → 401 (setup não concluido)

Login sucesso → JWT emitido → frontend redireciona para /change-password

PUT /api/auth/password { currentPassword, newPassword } (JWT obrigatório)
→ ChangePasswordUseCase verifica senha atual e UPSERT do novo hash
→ 204 → frontend redireciona para /dashboard
```

`/auth/login` continua mapeado como alias legado do login.

## Persistência

Migração 6 em `DatabaseInitializer` inalterada (tabela `AuthCredential`).

Setup **não persiste mais** configurações de Plex nem Media Roots em `AppSettings`. Essas configurações passam a ser feitas após login na tela de Settings (fora do escopo).

SetupTokenService inalterado:
- 32 bytes aleatórios em base64url.
- Composition root escreve o token direto em `Console.Out` (não depende de `LOG_LEVEL`), em uma linha com marcador estável `SETUP_TOKEN=`.
- Não é persistido. Reiniciar antes de concluir o setup gera e registra um novo token; o anterior deixa de valer. Reiniciar após o setup não gera token.
- É invalidado em memória logo após o setup; `CreateAsync` com PK única impede sobrescrever credencial existente em corrida.
- Hook de teste: se `SETUP_TOKEN` existe no ambiente no primeiro start, ele é usado e registrado no log; ausente em produção, o token é aleatório.

## Contratos

- `GET /api/auth/setup/status` (público)
  - `200 { "setupRequired": bool }`
- `POST /api/auth/setup` (público, somente enquanto não existe credencial)
  - body: `{ "token": "...", "newPassword": "..." }` — **campos `plexUrl`, `plexToken`, `mediaRoots` REMOVIDOS**
  - `204` em sucesso
  - `400` senha vazia, menor que 8 ou maior que 128 caracteres
  - `401` token inválido/ausente
  - `409` setup já concluído
- `POST /auth/login` (público, alias `/api/auth/login`)
  - body: `{ password }` → `{ token }`; sem credencial retorna `401`.
- `PUT /api/auth/password` (JWT obrigatório)
  - body: `{ currentPassword, newPassword }`
  - `204` em sucesso
  - `400` senha nova inválida
  - `401` sem JWT
  - `403` senha atual incorreta

## Frontend

### `SetupView.vue` (simplificado)

- **Remove**: Passos 2 (Plex) e 3 (Mídia) completamente.
- **Mantém**: Passo 1 (Senha) como única tela.
- **Alterações no Passo 1**:
  - Campo "Nova senha" pré-preenchido com `123456` (valor padrão).
  - Campo "Confirmar senha" pré-preenchido com `123456`.
  - Usuário pode editar ou aceitar o padrão.
  - Validação local: igualdade + mínimo 8 chars.
  - Botão "Continuar" → chama `submitSetup(token, newPassword)` (sem Plex/media).
- **Após sucesso (204)**:
  - `loginAfterSetup()` com a senha submetida.
  - Redireciona para `/change-password` (não mais `/dashboard`).
- **Hint do token**: mantido ("token de uso único está no log do container").
- **Tema/Menu**: usa `DefaultAuthCard` (já tem toggle de tema no header). Menu não aplicável (tela pública).

### `ChangePasswordView.vue` (já existe, ajustes menores)

- Rota: `/change-password` (nova rota, guardada por autenticação).
- Recebe `currentPassword` (obrigatório), `newPassword`, `confirmPassword`.
- Validação local: igualdade + mínimo 8 chars.
- Chama `changePassword(currentPassword, newPassword)` do `api.js`.
- Sucesso → redireciona para `/dashboard`.
- **Guard de navegação**: usuários autenticados que ainda não trocaram a senha (detectado via flag ou primeira vez) são redirecionados para `/change-password` ao tentar acessar outras rotas.

### Nova rota `/change-password`

```javascript
{
  path: "/change-password",
  name: "change-password",
  component: () => import("../views/Authentication/ChangePasswordView.vue"),
  meta: { title: "Alterar senha", requiresAuth: true, requiresPasswordChange: true }
}
```

### Guard de navegação (router/index.js)

- Adicionar verificação `requiresPasswordChange` no `beforeEach`:
  - Se rota tem `meta.requiresPasswordChange` e usuário não trocou senha (flag no `authStore` ou verificação via API), permitir acesso apenas a `/change-password` e `/logout`.
  - Alternativa simples: após login bem-sucedido no `loginAfterSetup()`, setar flag `mustChangePassword = true` no `authStore`; guard verifica essa flag e redireciona para `/change-password` se verdadeira e rota não for `/change-password`.

### `api.js` ajustes

- `submitSetup(token, newPassword)`: remover parâmetros `plexUrl`, `plexToken`, `mediaRoots`.
- Body do POST: `{ token, newPassword }`.

## Backend

### `CompleteSetupUseCase` (modificado)

- **Remove**: validação de `PlexUrl`, `PlexToken`, `MediaRoots`.
- **Remove**: chamada a `_plexClient.GetIdentityAsync`.
- **Remove**: persistência em `_settings.SaveAsync` (Plex/media).
- **Mantém**:
  - Verificação `IsConfiguredAsync` → 409.
  - Validação `SetupTokenValidator.TryValidate` → 401.
  - Validação `PasswordPolicy.IsValid(newPassword)` → 400.
  - `_credentials.CreateAsync(_hasher.Hash(newPassword))`.
  - `_setupTokens.Invalidate()`.
- **Signature**: `ExecuteAsync(CompleteSetupRequest request)` onde `CompleteSetupRequest` agora só tem `Token` e `NewPassword`.

### `CompleteSetupRequest` record (modificado)

```csharp
public sealed record CompleteSetupRequest(
    string? Token,
    string? NewPassword);
```

### `SetupRequest` DTO em `AuthEndpoints.cs` (modificado)

```csharp
public sealed record SetupRequest(
    string? Token,
    string? NewPassword);
```

### `AuthEndpoints.cs` (modificado)

- `MapPost("/api/auth/setup", ...)` usa `SetupRequest` simplificado.
- Passa apenas `request.Token` e `request.NewPassword` para `CompleteSetupUseCase`.

### `IAppSettingsStore` / `AppSettingsSnapshot` (inalterados, mas não usados no setup)

- Setup não chama mais `SaveAsync` com Plex/media.
- Configuração de Plex/media será feita via endpoints de Settings (fora do escopo).

### `ChangePasswordUseCase` / `ChangePasswordEndpoints` (inalterados)

- Já implementados e funcionando.

## Segurança

- Senha padrão `123456` é fraca intencionalmente; troca obrigatória no primeiro login mitiga.
- Token de setup: aleatório, comparação em tempo constante, uso único, logado no container.
- Troca de senha exige senha atual além do JWT.
- Endpoint de troca sob `/api/`, exigindo JWT.
- Senha não é logada nem devolvida; logs de e2e filtram `SETUP_TOKEN=`.
- PBKDF2 com salt por senha.
- Sem `ADMIN_PASSWORD`: não existe senha padrão fraca em ambiente (exceto `123456` no setup, que exige troca imediata).
- JWTs antigos continuam válidos após troca (stateless). Risco aceito.

## Testes

- `dotnet test`, `npm run lint`, `npm run build` e cópia para `wwwroot`.
- QA Playwright:
  1. `/setup` com token do log, senha padrão `123456`, confirmação → login automático → redireciona `/change-password`.
  2. `/change-password` com senha atual `123456`, nova senha válida, confirmação → sucesso → `/dashboard`.
  3. Tema light/dark toggle funcionando em `/setup` e `/change-password`.
  4. Menus (header/sidebar) funcionando após login.

## Riscos

- Senha padrão `123456` exposta no código/frontend: mitigada por troca obrigatória e token de setup aleatório.
- Usuário pode não trocar a senha se fechar aba: guard de navegação força `/change-password` no próximo acesso.
- Reinício antes do setup troca o token, exigindo nova leitura do log.
- Tokens JWT antigos continuam válidos após troca; a senha nova vale para novos logins e troca exige senha atual.