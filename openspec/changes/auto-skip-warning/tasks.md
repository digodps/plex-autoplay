# Tasks — Auto-Skip Authorization Warning

## Backend (.NET)

- [ ] **AppSettings.cs** — Adicionar `AutoSkipAuthorized` key, default, campo no snapshot
  - `AppSettingKeys.AutoSkipAuthorized = "AUTO_SKIP_AUTHORIZED"`
  - `AppSettingDefaults.AutoSkipAuthorized = false`
  - `AppSettingsSnapshot` adicionar `bool AutoSkipAuthorized` (posição 6)
  - `All` array incluir nova chave

- [ ] **SqliteAppSettingsStore.cs** — Leitura/escrita/seeder
  - Helper `GetBool` privado
  - `GetAsync()` ler `AUTO_SKIP_AUTHORIZED` via `GetBool`
  - `SaveAsync()` incluir chave no dicionário `values`
  - `AppSettingsSeeder.SeedAsync()` switch fallback para nova chave

- [ ] **UpdateAppSettingsUseCase.cs** — Request/validação
  - `UpdateAppSettingsRequest` adicionar `bool? AutoSkipAuthorized`
  - `ExecuteAsync` ler request ou manter atual, passar para snapshot

- [ ] **SettingsEndpoints.cs** — GET/PUT
  - `SettingsRequest` adicionar `[JsonPropertyName("auto_skip_authorized")] public bool? AutoSkipAuthorized`
  - `ToResponse` incluir `auto_skip_authorized = settings.AutoSkipAuthorized`

- [ ] **PlexAutoSkipBackgroundService.cs** — Gate de autorização
  - Em `PollAndSkipAsync`: no início, `if (!settings.AutoSkipAuthorized) return;`
  - Log debug informativo

- [ ] **Testes Unit (xUnit)**
  - `AppSettingsTests`: snapshot inclui flag
  - `SqliteAppSettingsStoreTests`: leitura/escrita bool
  - `UpdateAppSettingsUseCaseTests`: validação bool
  - `PlexAutoSkipBackgroundServiceTests`: `AutoSkipAuthorized=false` → sem seek

## Frontend (Vue)

- [ ] **SettingsView.vue** — Toggle + Modal Netflix
  - `form.auto_skip_authorized` reativo
  - `showAuthModal`, `pendingAuthToggle` refs
  - `applySettings` incluir `auto_skip_authorized`
  - `toggleAutoSkipAuthorized()`: se ligando → modal; se desligando → save direto
  - `confirmAuth()` / `cancelAuth()`
  - Template: toggle checkbox + label + texto explicativo
  - Template: `<Teleport>` modal com ícone warning, título, explicação, botões Cancelar/Autorizar
  - Estilos: `bg-orange-100`, `text-orange-600`, transições `animate-in slide-in-from-top-4`

## E2E Tests (Playwright)

- [ ] **test/auto-skip-authorization.spec.js**
  - Setup: login admin
  - Teste: toggle OFF → clicar → modal abre → Cancelar → toggle OFF
  - Teste: toggle OFF → clicar → modal abre → Autorizar → toggle ON
  - Teste: GET `/api/settings` retorna `auto_skip_authorized` correto
  - Teste (integração): mock session + marker → com autorizado → seek executado
  - Teste: sem autorizado → seek NÃO executado

## Commits Sugeridos

1. `feat(settings): add auto_skip_authorized config key`
2. `feat(backend): gate auto-skip on authorization flag`
3. `feat(frontend): add Netflix-style authorization modal in Settings`
4. `test: unit + e2e for auto-skip authorization`

## Acceptance Criteria

- [ ] Default `false` — auto-skip não executa sem autorização
- [ ] Settings → toggle exibe modal explicativo ao ligar
- [ ] Cancelar modal → toggle permanece OFF, nada salvo
- [ ] Confirmar modal → toggle ON, persistido no SQLite
- [ ] Background service respeita flag (unit + e2e)
- [ ] Lint/format/build passam