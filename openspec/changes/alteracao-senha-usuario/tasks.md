# Tasks — alteracao-senha-usuario

## Phase 1 — Requisitos e artefatos

- [ ] **EGM A/S/C** — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [ ] `openspec validate alteracao-senha-usuario --strict` passa.
- [ ] Commit somente dos artefatos da change.
- [ ] Worktree `feature/spec-alteracao-senha-usuario` criada e sessão movida.

## Phase 2 — Backend

- [ ] Porta `IPasswordHasher` (`Hash`, `Verify`) e implementação `Pbkdf2PasswordHasher` (PBKDF2-SHA256, salt 16 bytes, 210000 iterações, formato versionado, `FixedTimeEquals`, iterações injetáveis para teste).
- [ ] `PasswordPolicy`: limites 8..128 compartilhados por setup e troca.
- [ ] Porta `IAuthCredentialStore`: `IsConfiguredAsync`, `GetHashAsync`, `CreateAsync` (não sobrescreve), `UpdateAsync`.
- [ ] Repositório `SqliteAuthCredentialStore` com Dapper.
- [ ] Migração 6: tabela `AuthCredential` (`UserId` PK, `PasswordHash`, `UpdatedAt`).
- [ ] `LoginUseCase` async: valida somente hash do SQLite; remove dependência de `ADMIN_PASSWORD`.
- [ ] `CompleteSetupUseCase`: valida token de uso único, política de senha, hash e `CreateAsync`; resultados Success/InvalidToken/InvalidPassword/AlreadyConfigured.
- [ ] `ChangePasswordUseCase`: exige JWT + senha atual, valida política, verifica hash atual e atualiza; resultados Success/InvalidPassword/WrongCurrentPassword.
- [ ] `SetupTokenService` (Infrastructure/Auth): 32 bytes base64url, `EnsureGenerated` idempotente, `TryValidate` em tempo constante, `Invalidate` no primeiro sucesso; hook `SETUP_TOKEN` para testes.
- [ ] `AuthEndpoints`: `GET /api/auth/setup/status`, `POST /api/auth/setup`, `POST /api/auth/login` (+ alias `/auth/login`), `PUT /api/auth/password`; remove parâmetro `adminPassword`.
- [ ] Middleware JWT: allowlist exata de setup/login público; proteger `/api/auth/password`; injetar `ITokenService` (adicionar `ValidateToken` à interface).
- [ ] `Program.cs`/DI: registrar store, hasher, use cases e token service; após inicializar schema, gerar/logar token em stdout quando não houver credencial.
- [ ] Remover `ADMIN_PASSWORD` de `.env.example`, compose, scripts (`start.sh`, `dev-spec.py`, `run-e2e-tests.sh`), CI, README, `PlexConfigStore` e testes e2e.
- [ ] Filtrar `SETUP_TOKEN=` dos logs copiados para `test-artifacts`.
- [ ] Atualizar e2e para setup via token (`SETUP_TOKEN`) e login com senha criada; volume limpo por execução.

## Phase 3 — Frontend

- [ ] `SetupView.vue` em `/setup`: token, nova senha, confirmação, aviso de que o token é de uso único e está no log (`docker logs <container>`), erros de API.
- [ ] Guarda de rotas: consulta `/api/auth/setup/status`; `/setup` quando pendente; `/login` após setup; rotas internas para `/setup` se pendente; sem loop.
- [ ] `PasswordModal.vue`: senha atual, nova senha e confirmação; validação local; `PUT /api/auth/password`; erro e fechamento em sucesso; `label for`, `id` e `data-testid`.
- [ ] `AppHeader.vue`: remover botão de configurações; adicionar `Alterar senha` no menu do usuário.
- [ ] `api.js`: helpers de setup status/setup/change password; login em `/api/auth/login`.
- [ ] Remover `SettingsModal.vue` e referências.

## Phase 4 — Build

- [ ] Testes unitários de hasher, política, login, setup, troca e repositório SQLite.
- [ ] `dotnet build PlexAutoplay.slnx` passa.
- [ ] `dotnet test` passa.
- [ ] `npm run lint` e `npm run build` passam.
- [ ] Copiar `dist` para `src/PlexAutoplay.Web/wwwroot`.
- [ ] `docker build --no-cache -t plex-autoplay:alteracao-senha-usuario .` passa.

## Phase 5 — QA

- [ ] `openspec validate alteracao-senha-usuario --strict` passa.
- [ ] Container isolado: log contém `SETUP_TOKEN=` de uso único no primeiro start; não contém após setup nem em restart.
- [ ] Token não é reutilizado após setup (`409`/`401`).
- [ ] Playwright em `http://localhost:<port>`: `/setup` com token, criação de senha, login, menu, modal de senha, troca, logout, login com senha nova.
- [ ] Playwright: token de uso único visível na instrução da tela; tema light/dark aplicado e menus abrindo/navegando/fechando com estado ativo.
- [ ] Evidência de URL acessada e screenshots/asserts.
- [ ] `qa-report-1.md` com PASS/FAIL e achados numerados.
