# Tasks — alteracao-senha-usuario

## Phase 1 — Requisitos e artefatos

- [x] **EGM A/S/C** — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [x] `openspec validate alteracao-senha-usuario --strict` passa.
- [x] Commit somente dos artefatos da change.
- [x] Worktree `feature/spec-alteracao-senha-usuario` criada e sessão movida.

## Phase 2 — Backend

- [x] Porta `IPasswordHasher` (`Hash`, `Verify`) e implementação `Pbkdf2PasswordHasher` (PBKDF2-SHA256, salt 16 bytes, 210000 iterações, formato versionado, `FixedTimeEquals`, iterações injetáveis para teste).
- [x] `PasswordPolicy`: limites 8..128 compartilhados por setup e troca.
- [x] Porta `IAuthCredentialStore`: `IsConfiguredAsync`, `GetHashAsync`, `CreateAsync` (não sobrescreve), `UpdateAsync`.
- [x] Repositório `SqliteAuthCredentialStore` com Dapper.
- [x] Migração 6: tabela `AuthCredential` (`UserId` PK, `PasswordHash`, `UpdatedAt`).
- [x] `LoginUseCase` async: valida somente hash do SQLite; remove dependência de `ADMIN_PASSWORD`.
- [x] `CompleteSetupUseCase`: valida token de uso único, política de senha, hash e `CreateAsync`; resultados Success/InvalidToken/InvalidPassword/AlreadyConfigured.
- [x] `ChangePasswordUseCase`: exige JWT + senha atual, valida política, verifica hash atual e atualiza; resultados Success/InvalidPassword/WrongCurrentPassword.
- [x] `SetupTokenService` (Infrastructure/Auth): 32 bytes base64url, `EnsureGenerated` idempotente, `TryValidate` em tempo constante, `Invalidate` no primeiro sucesso; hook `SETUP_TOKEN` para testes.
- [x] `AuthEndpoints`: `GET /api/auth/setup/status`, `POST /api/auth/setup`, `POST /api/auth/login` (+ alias `/auth/login`), `PUT /api/auth/password`; remove parâmetro `adminPassword`.
- [x] Middleware JWT: allowlist exata de setup/login público; proteger `/api/auth/password`; injetar `ITokenService` (adicionar `ValidateToken` à interface).
- [x] `Program.cs`/DI: registrar store, hasher, use cases e token service; após inicializar schema, gerar/logar token em stdout quando não houver credencial.
- [x] Remover `ADMIN_PASSWORD` de `.env.example`, compose, scripts (`start.sh`, `dev-spec.py`, `run-e2e-tests.sh`), CI, README, `PlexConfigStore` e testes e2e.
- [x] Filtrar `SETUP_TOKEN=` dos logs copiados para `test-artifacts`.
- [x] Atualizar e2e para setup via token (`SETUP_TOKEN`) e login com senha criada; volume limpo por execução.

## Phase 3 — Frontend

- [x] `SetupView.vue` em `/setup`: token, nova senha, confirmação, aviso de que o token é de uso único e está no log (`docker logs <container>`), erros de API.
- [x] Guarda de rotas: consulta `/api/auth/setup/status`; `/setup` quando pendente; `/login` após setup; rotas internas para `/setup` se pendente; sem loop.
- [x] `PasswordModal.vue`: senha atual, nova senha e confirmação; validação local; `PUT /api/auth/password`; erro e fechamento em sucesso; `label for`, `id` e `data-testid`.
- [x] `AppHeader.vue`: remover botão de configurações; adicionar `Alterar senha` no menu do usuário.
- [x] `api.js`: helpers de setup status/setup/change password; login em `/api/auth/login`.
- [x] Remover `SettingsModal.vue` e referências.

## Phase 4 — Build

- [x] Testes unitários de hasher, política, login, setup, troca e repositório SQLite.
- [x] `dotnet build PlexAutoplay.slnx` passa.
- [x] `dotnet test` passa.
- [x] `npm run lint` e `npm run build` passam.
- [x] Copiar `dist` para `src/PlexAutoplay.Web/wwwroot`.
- [x] `docker build --no-cache -t plex-autoplay:alteracao-senha-usuario .` passa.

## Phase 5 — QA

- [x] `openspec validate alteracao-senha-usuario --strict` passa.
- [x] Container isolado: log contém `SETUP_TOKEN=` de uso único no primeiro start; não contém após setup nem em restart.
- [x] Token não é reutilizado após setup (`409`/`401`).
- [x] Playwright em `http://localhost:<port>`: `/setup` com token, criação de senha, login, menu, modal de senha, troca, logout, login com senha nova.
- [x] Playwright: token de uso único visível na instrução da tela; tema light/dark aplicado e menus abrindo/navegando/fechando com estado ativo.
- [x] Evidência de URL acessada e screenshots/asserts.
- [x] `qa-report-1.md` com PASS/FAIL e achados numerados.
