# Proposal

## Why

A página inicial do Plex Autoplay tem um tema amarelo fixo (`--accent: #e5a00d` em `style.css`) e o modo escuro só entra automaticamente via `prefers-color-scheme` do sistema operacional, sem qualquer controle do usuário. Não há como o usuário escolher a cor de destaque nem forçar claro/escuro, e a escolha não persiste entre sessões. Esta mudança adiciona uma paleta de cores de destaque selecionável e um toggle de modo claro/escuro na tela inicial, com persistência no banco de dados vinculada ao usuário autenticado, para que a preferência seja mantida ao reabrir o aplicativo.

## What Changes

- Novo store Pinia `theme` (frontend) com `accent` (cor de destaque) e `mode` (`light`/`dark`/`system`), lido e gravado via API REST.
- Novos endpoints backend `GET /api/theme` e `PUT /api/theme` que leem e gravam a preferência de tema em SQLite (tabela `UserThemePreferences`).
- Nova entidade de domínio `UserThemePreference` e porta `IUserThemeRepository` com implementação SQLite (`SqliteUserThemeRepository`), seguindo o padrão existente de `SqliteMarkerRepository`.
- CSS atualizado: `--accent` exposto como variável sobrescrita pela store; classe `.theme-dark` aplicada ao elemento raiz para forçar modo escuro independente do SO.
- Controles de UI no canto superior da tela inicial (`DashboardView.vue`): toggle claro/escuro + seletor de paleta de cores de destaque (cores pré-definidas, não picker livre, para proteger contraste).
- Registro do novo repositório no DI (`ServiceCollectionExtensions`).
- Leitura do tema salvo ao montar o frontend (`main.js`) e aplicação da classe de escuro.

## Capabilities

### New Capabilities

- `ui-theme`: Preferência de tema do usuário — cor de destaque selecionável e modo claro/escuro forçado ou seguido do sistema, persistido no banco e aplicado ao carregar a aplicação.

### Modified Capabilities

- (nenhum)

## Impact

- Código afetado: `vue/plex-frontend/src/stores/theme.js` (novo), `vue/plex-frontend/src/style.css`, `vue/plex-frontend/src/views/DashboardView.vue`, `vue/plex-frontend/src/main.js`, `vue/plex-frontend/src/api.js` (possível helper).
- Backend: `src/PlexAutoplay.Domain/Entities/UserThemePreference.cs` (novo), `src/PlexAutoplay.Domain/Ports/IUserThemeRepository.cs` (novo), `src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteUserThemeRepository.cs` (novo), `src/PlexAutoplay.Web/Endpoints/ThemeEndpoints.cs` (novo), `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs` (registro DI).
- Banco: nova tabela `UserThemePreferences` no SQLite existente (`/data/app.db`).
- APIs: `GET /api/theme`, `PUT /api/theme` (autenticados por JWT).
- Nenhuma mudança quebrante (não-BREAKING): endpoints novos, tabela nova, comportamento anterior preservado quando nenhuma preferência está salva (fallback amarelo + segue o SO).
