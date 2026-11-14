# Tasks — simplificar-setup-inicial

## Phase 1 — Requisitos e artefatos

- [ ] **EGM A/S/C** — 3 revisores paralelos, achados triados em `deficiencias_resolvidas.md`.
- [ ] `openspec validate simplificar-setup-inicial --strict` passa.
- [ ] Commit somente dos artefatos da change.
- [ ] Worktree `feature/spec-simplificar-setup-inicial` criada e sessão movida.

## Phase 2 — Backend

- [ ] Remover `ISetupTokenValidator` (`Application`) e `SetupTokenService` (`Infrastructure`).
- [ ] `Program.cs`: remover bloco `SETUP_TOKEN=` / `EnsureGenerated()`.
- [ ] `ServiceCollectionExtensions`: remover registro de `SetupTokenService`.
- [ ] `LoginUseCase`: quando não há credencial, aceitar a senha padrão `123456` e retornar `forcePasswordChange: true`; senha custom retorna `false`.
- [ ] `CompleteSetupUseCase`: remover `ISetupTokenValidator` e o campo `Token`; manter validação de política, Plex e mídia.
- [ ] `AuthEndpoints`: `setup/status` inclui `defaultPassword` quando pendente; login inclui `forcePasswordChange`; `/api/auth/setup` deixa de tratar `InvalidToken`.
- [ ] `JwtAuthMiddleware`: remover `/api/auth/setup` da allowlist pública (passa a exigir JWT).

## Phase 3 — Frontend

- [ ] `SigninView.vue`: exibir aviso "Senha padrão: 123456" quando `setupRequired`; redirecionar para `/setup` quando `forcePasswordChange`.
- [ ] `SetupView.vue`: remover campo e aviso de token; passo 1 = nova senha + confirmação.
- [ ] `api.js`: `submitSetup` sem `token`, usando `apiFetch` (JWT).
- [ ] `router/index.js`: guarda — `setupRequired` → `/login` se não autenticado, `/setup` se autenticado; `/setup` exige autenticação.

## Phase 4 — Testes e docs

- [ ] Remover teste `SetupToken_is_single_use_after_invalidation` e `FakeTokenValidator`.
- [ ] Atualizar testes de `LoginUseCase` e `CompleteSetupUseCase` (sem token).
- [ ] `tests/e2e/global-setup.js` e scripts e2e: setup via senha padrão (sem `SETUP_TOKEN`).
- [ ] `infra/docker-compose.test.yml`: remover `SETUP_TOKEN`.
- [ ] Atualizar docs/README que citam token de setup.

## Phase 5 — Build

- [ ] `dotnet build PlexAutoplay.slnx` passa.
- [ ] `dotnet test` passa.
- [ ] `npm run lint` e `npm run build` passam em `vue/plex-frontend`.
- [ ] Copiar `dist` para `src/PlexAutoplay.Web/wwwroot`.
- [ ] `./scripts/build.sh` passa.

## Phase 6 — QA

- [ ] `openspec validate simplificar-setup-inicial --strict` passa.
- [ ] Container isolado: log **não** contém `SETUP_TOKEN`.
- [ ] `/login` exibe "Senha padrão: 123456" no primeiro acesso.
- [ ] Playwright: login `123456` → `/setup` → conclui → dashboard.
- [ ] Playwright: tema (light/dark) e menu funcionando.
- [ ] `/api/auth/setup` sem JWT retorna `401`.