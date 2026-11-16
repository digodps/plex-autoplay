# Tasks — simplificacao-setup

## 1. Backend — CompleteSetupUseCase & DTOs

- [ ] 1.1 Modificar `CompleteSetupRequest` record em `CompleteSetupUseCase.cs`: remover `PlexUrl`, `PlexToken`, `MediaRoots`; manter apenas `Token` e `NewPassword`
- [ ] 1.2 Modificar `CompleteSetupUseCase.ExecuteAsync`: remover validação de PlexUrl/PlexToken/MediaRoots; remover chamada a `_plexClient.GetIdentityAsync`; remover persistência em `_settings.SaveAsync`; manter validações de token, senha, criação de credencial e invalidação do token
- [ ] 1.3 Modificar `SetupRequest` DTO em `AuthEndpoints.cs`: remover `PlexUrl`, `PlexToken`, `MediaRoots`; manter apenas `Token` e `NewPassword`
- [ ] 1.4 Ajustar `AuthEndpoints.MapPost("/api/auth/setup")` para usar `SetupRequest` simplificado e passar apenas `Token` e `NewPassword` para o use case
- [ ] 1.5 Verificar que `dotnet build PlexAutoplay.slnx -c Release` passa

## 2. Frontend — SetupView.vue (simplificação)

- [ ] 2.1 Remover `step` state e lógica de navegação entre passos (next/back)
- [ ] 2.2 Remover `plexUrl`, `plexToken`, `mediaRoots`, `testing`, `plexCheck` states
- [ ] 2.3 Remover `testPlex()` function
- [ ] 2.4 Remover `steps` array (ou manter apenas passo 1)
- [ ] 2.5 Pré-preencher `newPassword` e `confirmPassword` com `"123456"` no `ref`
- [ ] 2.6 Simplificar `validateStep()`: validar apenas token, senha ≥ 8, confirmação igual
- [ ] 2.7 Simplificar `finish()` (renomear para `submit`): chamar `submitSetup(token, newPassword)` sem parâmetros Plex/media
- [ ] 2.8 No sucesso (204): chamar `loginAfterSetup()` com a senha submetida e redirecionar para `/change-password` (não `/dashboard`)
- [ ] 2.9 Remover template dos passos 2 e 3 (Plex, Mídia); manter apenas formulário do passo 1
- [ ] 2.10 Manter hint do token de setup, toggle de visibilidade de senha, validação local, error handling
- [ ] 2.11 Manter `DefaultAuthCard`, tema, acessibilidade (labels, data-testid)

## 3. Frontend — Rota /change-password e Guard

- [ ] 3.1 Adicionar rota `/change-password` em `router/index.js` apontando para `ChangePasswordView.vue` com `meta: { title: "Alterar senha", requiresAuth: true, requiresPasswordChange: true }`
- [ ] 3.2 Em `router/index.js` `beforeEach`: adicionar verificação `requiresPasswordChange` — se usuário autenticado e `authStore.mustChangePassword === true` e rota não for `/change-password` nem `/logout`, redirecionar para `/change-password`
- [ ] 3.3 Em `auth.js` store: adicionar state `mustChangePassword: false` e action `setMustChangePassword(value)`
- [ ] 3.4 Em `SetupView.vue` `loginAfterSetup()`: após login bem-sucedido, `authStore.setMustChangePassword(true)` antes de redirecionar para `/change-password`
- [ ] 3.5 Em `ChangePasswordView.vue` `apply()`: no sucesso (204), `authStore.setMustChangePassword(false)` e redirecionar para `/dashboard`

## 4. Frontend — ChangePasswordView.vue (ajustes)

- [ ] 4.1 Substituir `/* TODO: chamar API */` por chamada real a `changePassword(oldPassword, newPassword)` do `api.js`
- [ ] 4.2 Remover `setTimeout` mock; usar `await` da API real
- [ ] 4.3 Tratar respostas de erro: 400 (senha fraca), 403 (senha atual errada), 401 (sem JWT), genérico
- [ ] 4.4 Exibir erros da API no template (similar a `SetupView.vue`)
- [ ] 4.5 Manter toggle de visibilidade, validação local, acessibilidade, data-testid

## 5. Frontend — api.js (ajuste submitSetup)

- [ ] 5.1 Modificar `submitSetup(token, newPassword, plexUrl, plexToken, mediaRoots)` para `submitSetup(token, newPassword)` — remover 3 últimos parâmetros
- [ ] 5.2 Ajustar body do POST para `{ token, newPassword }`

## 6. Build & Testes

- [ ] 6.1 `cd vue/plex-frontend && npm run lint` — deve passar
- [ ] 6.2 `cd vue/plex-frontend && npm run build` — deve passar e copiar para `src/PlexAutoplay.Web/wwwroot`
- [ ] 6.3 `dotnet build PlexAutoplay.slnx -c Release` — deve passar
- [ ] 6.4 `docker compose -f infra/docker-compose.yml build` — deve passar
- [ ] 6.5 `dotnet test` — deve passar

## 7. QA Playwright (validação obrigatória)

- [ ] 7.1 Subir container local (`docker compose up -d`) e obter `SETUP_TOKEN` do log
- [ ] 7.2 Acessar `http://localhost:<port>/setup` com Playwright
- [ ] 7.3 Validar: tela mostra apenas token, senha (pré-preenchida `123456`), confirmação (pré-preenchida `123456`); NÃO mostra Plex/Mídia
- [ ] 7.4 Submeter setup com token válido + senha padrão → login automático → redireciona `/change-password`
- [ ] 7.5 Validar tema light/dark toggle em `/setup` e `/change-password`
- [ ] 7.6 Em `/change-password`: submeter senha atual `123456`, nova senha válida, confirmação → sucesso → `/dashboard`
- [ ] 7.7 Validar menus (header/sidebar) funcionando após login no dashboard
- [ ] 7.8 `openspec validate simplificacao-setup --strict` — deve passar