# Proposal

## Why

O PlexAutoSkip atualmente pula intro/créditos automaticamente, sem qualquer consentimento do usuário. Alguns usuários querem controle explícito (estilo Netflix: "Pular intro?"). Requisito: exibir um **aviso para autorizar o auto-skip**; se o usuário **não concordar, nada deve ser feito** (nenhum skip é executado).

Isso atende usuários que querem assistir intros/créditos completos e evita saltos inesperados.

## What Changes

- **Nova configuração operacional** `auto_skip_authorized` (bool, default `false`). Enquanto `false`, o `PlexAutoSkipBackgroundService` **não executa nenhum seek**.
- **SettingsView (Vue, ⚙ Ajustes)** — controle "Autorizar skip automático" que, ao ser ligado, abre um aviso/modal de confirmação explica o comportamento. Se o usuário **cancelar**, o toggle permanece desligado e nada é salvo.
- **Endpoint `/api/settings`** — campo `auto_skip_authorized` no GET e PUT.
- **PlexAutoSkipBackgroundService** — consulta `AutoSkipAuthorized`; se `false`, retorna cedo (sem seek).
- **Persistência SQLite** — nova chave `AUTO_SKIP_AUTHORIZED`, default `false`, semeada por `AppSettingsSeeder`.

## Impact

- **Backend (.NET)**:
  - `AppSettings.cs` — chave, default, campo no `AppSettingsSnapshot`
  - `SqliteAppSettingsStore.cs` — leitura/escrita + seeder
  - `UpdateAppSettingsUseCase.cs` — request/validação
  - `SettingsEndpoints.cs` — GET/PUT
  - `PlexAutoSkipBackgroundService.cs` — gate de autorização
- **Frontend (Vue)**:
  - `SettingsView.vue` — toggle + modal de confirmação Netflix-style
- **Testes**:
  - Unit: persistência/validação do flag; background service não faz seek quando não autorizado
  - E2E: toggle → modal → cancelar não salva; confirmar salva
- **Sem breaking changes** — default `false` apenas torna o auto-skip opt-in (mais seguro)
- **Configuração**: nenhuma obrigatória; opt-in via UI