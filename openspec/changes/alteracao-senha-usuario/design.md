# Design — alteracao-senha-usuario

## Fluxo

```text
Primeiro start (sem credencial no SQLite)
  → SetupTokenService.EnsureGenerated()
  → composition root escreve no stdout do container:
      ============================================================
      CONFIGURACAO INICIAL - TOKEN DE SETUP (USO UNICO)
      SETUP_TOKEN=<base64url-aleatorio>
      Informe este token em /setup para criar a senha do operador.
      O token deixa de valer apos a primeira conclusao do setup.
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

POST /api/auth/login { password }
  → LoginUseCase consulta AuthCredential no SQLite
      ├─ hash existe → PBKDF2 verify
      └─ hash ausente → 401 (setup não concluído)

Menu do usuário → "Alterar senha" → PasswordModal
  → PUT /api/auth/password { currentPassword, newPassword } (JWT obrigatório)
  → ChangePasswordUseCase verifica senha atual e UPSERT do novo hash
```

`/auth/login` continua mapeado como alias legado do login.

## Persistência

Migração 6 em `DatabaseInitializer`:

```sql
CREATE TABLE IF NOT EXISTS AuthCredential (
    UserId TEXT NOT NULL PRIMARY KEY,
    PasswordHash TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
)
```

Um único registro (`UserId = 'admin'`). A presença do registro é o sinal de
setup concluído. A senha nunca é gravada em texto puro: formato
`pbkdf2-sha256$<iterações>$<salt-base64>$<hash-base64>` (salt aleatório de 16
bytes, 210000 iterações, SHA-256, 32 bytes), via
`Rfc2898DeriveBytes.Pbkdf2` da BCL. Comparação com
`CryptographicOperations.FixedTimeEquals`.

## Token de setup

- `SetupTokenService` (Infrastructure/Auth) com `EnsureGenerated()` idempotente,
  `TryValidate(token)` em tempo constante e `Invalidate()`.
- Uso único: o token é descartado da memória no primeiro setup bem-sucedido e
  rejeitado em qualquer tentativa posterior (além disso, a credencial já
  existente bloqueia novas chamadas com `409`).
- 32 bytes aleatórios em base64url.
- O composition root escreve o token direto em `Console.Out` (não depende de
  `LOG_LEVEL`), em uma linha com marcador estável `SETUP_TOKEN=`.
- Não é persistido. Reiniciar antes de concluir o setup gera e registra um
  novo token; o anterior deixa de valer. Reiniciar após o setup não gera
  token.
- É invalidado em memória logo após o setup; `CreateAsync` com PK única impede
  sobrescrever credencial existente em corrida.
- Hook de teste: se `SETUP_TOKEN` existe no ambiente no primeiro start, ele é
  usado e registrado no log; ausente em produção, o token é aleatório.

## Contratos

- `GET /api/auth/setup/status` (público)
  - `200 { "setupRequired": bool }`
- `POST /api/auth/setup` (público, somente enquanto não existe credencial)
  - body: `{ "token": "...", "newPassword": "..." }`
  - `204` em sucesso
  - `400` senha vazia, menor que 8 ou maior que 128 caracteres
  - `401` token inválido/ausente
  - `409` setup já concluído
- `POST /api/auth/login` (público, alias `/auth/login`)
  - body: `{ password }` → `{ token }`; sem credencial retorna `401`.
- `PUT /api/auth/password` (JWT obrigatório)
  - body: `{ currentPassword, newPassword }`
  - `204` em sucesso
  - `400` senha nova inválida
  - `401` sem JWT
  - `403` senha atual incorreta

O middleware JWT usa allowlist exata para as três rotas públicas de auth e
protege `/api/auth/password`. Passa a depender de `ITokenService` (interface),
não de `JwtTokenService`.

## Camadas

| Camada | Artefato |
|---|---|
| Application | `IAuthCredentialStore`, `IPasswordHasher`, `PasswordPolicy`, `ISetupTokenValidator`, `CompleteSetupUseCase`, `ChangePasswordUseCase`, `LoginUseCase` |
| Domain | sem novas dependências |
| Infrastructure | `SqliteAuthCredentialStore`, `Pbkdf2PasswordHasher`, `SetupTokenService`, migração 6 |
| Web | `AuthEndpoints`, middleware, DI em `ServiceCollectionExtensions`/`Program.cs` |
| Frontend | `SetupView.vue`, `PasswordModal.vue`, `AppHeader.vue`, router, `api.js` |

## UI

- Remove o `<button aria-label="Configurações">` do header.
- Menu do usuário mantém "Configurações" (página existente) e adiciona
  "Alterar senha" antes de "Sair".
- `PasswordModal.vue`: senha atual, nova senha e confirmação; validação local
  de igualdade e mínimo; erro da API exibido no modal; fecha ao salvar com
  sucesso. Campos com `label for`, `id` e `data-testid`.
- `SettingsModal.vue` é removido; o tema continua no toggle do header.
- `SetupView.vue`: token, nova senha, confirmação, aviso de que o token está
  no log do container (`docker logs <container>`) e link para `/login` quando
  o setup já estiver concluído.
- Rotas: `setup` pública; guarda consulta `setupRequired` antes da checagem
  autenticada e evita loop.

## Segurança

- Token de setup aleatório, comparação em tempo constante e uso único.
- Troca de senha exige a senha atual além do JWT, reduzindo risco de takeover
  por token roubado.
- Endpoint de troca de senha sob `/api/`, exigindo JWT.
- Senha não é logada nem devolvida; logs de e2e são filtrados para remover
  `SETUP_TOKEN=`.
- PBKDF2 com salt por senha impede leitura direta da senha no SQLite.
- Sem `ADMIN_PASSWORD`: não existe senha padrão fraca em ambiente.
- JWTs antigos continuam válidos após a troca (JWT stateless). Risco aceito e
  mitigado pela exigência da senha atual; invalidar sessões fica fora do
  escopo.
- Recuperação após perda da senha fica fora do escopo; o operador pode apagar
  o registro `AuthCredential` (ou o banco) para refazer o setup.

## Testes

- Unitário de `Pbkdf2PasswordHasher` com iterações baixas injetáveis
  (hash/verify, salt distinto, formato inválido).
- Unitário de `PasswordPolicy` nos limites 7/8/128/129.
- Unitário do `LoginUseCase`: autentica com hash do SQLite; sem credencial
  retorna falha; senha errada falha.
- Unitário do `CompleteSetupUseCase`: token válido cria; inválido não cria;
  senha inválida não cria; setup repetido retorna conflito.
- Unitário do `ChangePasswordUseCase`: senha atual correta atualiza; incorreta
  retorna 403; senha nova inválida retorna 400.
- Teste de repositório SQLite: `CreateAsync` não sobrescreve; `UpdateAsync`
  atualiza.
- Build gate: `dotnet build PlexAutoplay.slnx`, `dotnet test`, `npm run lint`,
  `npm run build` e cópia para `wwwroot`.
- QA Playwright: `/setup` com token do log, criação de senha, login, abertura
  do menu, modal, troca de senha, logout, login com senha nova, tema e menu.

## Riscos

- Token de setup aparece no log do container: é intencional e de uso único;
  deve ser tratado como segredo até a configuração e removido de artefatos.
- Reinício antes do setup troca o token, exigindo nova leitura do log.
- Tokens JWT antigos continuam válidos após a troca; a senha nova vale para
  novos logins e a troca exige a senha atual.
