# Design — simplificar-setup-inicial

## Fluxo

```text
Primeiro start (sem credencial no SQLite)
  → Nenhum token gerado; nada escrito no log
  → GET /api/auth/setup/status → { "setupRequired": true, "defaultPassword": "123456" }
    → SPA redireciona para /login
    → tela de login exibe: "Senha padrão: 123456"

POST /auth/login { password: "123456" }
  → LoginUseCase: credencial ausente → aceita senha padrão → JWT
    → resposta { success: true, token, forcePasswordChange: true }
  → SPA redireciona para /setup (fluxo obrigatório)

/setup (autenticado com JWT do login padrão)
  Passo 1 — nova senha + confirmação
  Passo 2 — URL + token do Plex
  Passo 3 — diretórios de mídia
  → POST /api/auth/setup { newPassword, plexUrl, plexToken, mediaRoots } (JWT obrigatório)
      1. credencial já existe? → 409
      2. senha fora de 8..128? → 400
      3. valida Plex (GET /identity, 10s) e diretórios → 400 em falha
      4. persiste PLEX_URL/PLEX_TOKEN/MEDIA_ROOTS
      5. Pbkdf2PasswordHasher.Hash + AuthCredential.CreateAsync
      6. → 204

Após setup → login automático com nova senha → dashboard
Login com senha custom → { success, token, forcePasswordChange: false }
```

`/auth/login` continua mapeado como alias legado do login.

## Decisões

- **Sem mudança de schema**: a ausência de credencial em `AuthCredential` é o sinal de
  "setup pendente". A senha padrão só é aceita enquanto não existe credencial; após o
  setup ela deixa de valer automaticamente.
- **Sem token de setup**: `ISetupTokenValidator`, `SetupTokenService` e o bloco
  `SETUP_TOKEN=` do `Program.cs` são removidos.
- **`defaultPassword` no status**: só presente quando `setupRequired: true`, para a tela
  de login exibir a senha padrão.
- **`forcePasswordChange`**: retornado pelo login quando a senha aceita é a padrão; o
  frontend redireciona para `/setup`. A guarda de rota (`setupRequired`) reforça.
- **Setup autenticado**: `POST /api/auth/setup` passa a exigir JWT (obtido no login
  padrão) em vez de token; a allowlist pública do middleware perde `/api/auth/setup`.

## Contratos

- `GET /api/auth/setup/status` (público)
  - `200 { "setupRequired": bool, "defaultPassword": string|null }`
  - `defaultPassword = "123456"` apenas quando `setupRequired: true`

- `POST /auth/login` e `POST /api/auth/login` (público)
  - body: `{ password }`
  - `200 { success: true, token, forcePasswordChange: bool }`
  - `401` senha incorreta; `401` quando não há credencial e a senha != `123456`

- `POST /api/auth/setup` (JWT obrigatório, somente enquanto não existe credencial)
  - body: `{ newPassword, plexUrl, plexToken, mediaRoots }` (sem token)
  - `204` em sucesso
  - `400` senha fora da política ou configuração Plex/mídia inválida
  - `409` setup já concluído

- `PUT /api/auth/password` (JWT obrigatório) — inalterado
  - body: `{ currentPassword, newPassword }` → `204` / `400` / `401` / `403`

## Camadas

| Camada | Artefato | Mudança |
|---|---|---|
| Application | `CompleteSetupUseCase` | remove `ISetupTokenValidator`; `SetupRequest.Token` some |
| Application | `LoginUseCase` | aceita senha padrão quando não há credencial; retorna `forcePasswordChange` |
| Application | `ISetupTokenValidator` | **removido** |
| Infrastructure | `SetupTokenService` | **removido** |
| Web | `AuthEndpoints` | status inclui `defaultPassword`; login inclui `forcePasswordChange` |
| Web | `Program.cs` | remove bloco `SETUP_TOKEN=` |
| Web | `ServiceCollectionExtensions` | remove registro de `SetupTokenService` |
| Web | `JwtAuthMiddleware` | remove `/api/auth/setup` da allowlist pública |
| Frontend | `SigninView.vue` | exibe "Senha padrão: 123456"; redireciona a `/setup` quando `forcePasswordChange` |
| Frontend | `SetupView.vue` | remove campo/aviso de token; passo 1 = nova senha + confirmação |
| Frontend | `api.js` | `submitSetup` sem `token`; usa `apiFetch` (JWT) |
| Frontend | `router/index.js` | guarda: `setupRequired` → `/login` se não autenticado, `/setup` se autenticado |

## UI

- `SigninView.vue`: caixa informativa "Primeira configuração: a senha padrão é **123456**"
  visível apenas quando `setupRequired` é `true`.
- `SetupView.vue`: 3 passos mantidos (senha, Plex, mídia). Passo 1 sem token; aviso de
  que a senha atual é a padrão e deve ser trocada. Após `204`, login automático com a
  nova senha e abertura do dashboard.
- Tema (light/dark) e menus inalterados.

## Segurança

- Senha padrão `123456` é fraca **intencionalmente**; o fluxo força a troca antes de
  liberar o restante da aplicação (guarda de rota + `setupRequired`).
- A senha padrão só é aceita enquanto não existe credencial; após o setup não vale mais.
- `POST /api/auth/setup` passa a exigir JWT (não é mais público).
- `PUT /api/auth/password` continua exigindo senha atual + JWT.
- Token de setup removido → não há segredo no log do container.
- PBKDF2 com salt por senha impede leitura direta da senha no SQLite.

## Testes

- Unitário: `CompleteSetupUseCase` sem token (cria credencial, rejeita duplicada, valida política).
- Unitário: `LoginUseCase` aceita `123456` sem credencial e retorna `forcePasswordChange: true`;
  rejeita `123456` quando já existe credencial custom; retorna `false` para senha custom.
- Remover testes de `SetupTokenService`.
- Build gate: `dotnet build PlexAutoplay.slnx`, `dotnet test`, `npm run lint`, `npm run build`.
- QA Playwright em `http://localhost:<port>`:
  - `/login` exibe "Senha padrão: 123456"
  - login com `123456` → redireciona a `/setup`
  - conclui setup (senha → Plex → mídia) → dashboard
  - tema (light/dark) e menu funcionando