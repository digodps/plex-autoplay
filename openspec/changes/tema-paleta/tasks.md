# Tasks

## 1. Backend — domínio e persistência de tema

- [x] 1.1 Criar entidade de domínio `UserThemePreference` (accentColor, mode, userId) em `PlexAutoplay.Domain/Entities` e verificar que compila
- [x] 1.2 Criar porta `IUserThemeRepository` (GetAsync, UpsertAsync) em `PlexAutoplay.Domain/Ports` e verificar que compila
- [x] 1.3 Criar repositório SQLite `SqliteUserThemeRepository` (tabela `UserThemePreferences`, `EnsureTableCreated`, migration aditiva) em `PlexAutoplay.Infrastructure/Persistence/Repositories` e verificar que compila
- [x] 1.4 Registrar `SqliteUserThemeRepository` no DI em `ServiceCollectionExtensions.AddApplicationServices` e verificar injeção

## 2. Backend — endpoints de tema

- [x] 2.1 Criar `ThemeEndpoints.cs` com `GET /api/theme` e `PUT /api/theme` (autenticados por JWT) e verificar que os endpoints respondem
- [x] 2.2 Mapear `ThemeEndpoints` em `Program.cs` e verificar que a aplicação inicia sem erros de rota

## 3. Frontend — store de tema

- [x] 3.1 Criar store Pinia `theme.js` com estado `accent`, `mode`, ações de carregar/salvar via API e persistência, e verificar que a store é instanciada
- [x] 3.2 Adicionar helpers de tema em `api.js` (ou uso direto de `apiFetch`) e verificar que as chamadas a `/api/theme` funcionam

## 4. Frontend — CSS e aplicação de tema

- [x] 4.1 Atualizar `style.css`: expor `--accent` como variável sobrescrita pela store, adicionar classe `.theme-dark` no elemento raiz, manter fallback amarelo e modo sistema quando não houver preferência
- [x] 4.2 Atualizar `main.js` para carregar o tema do backend ao montar e aplicar a classe de escuro no elemento raiz, e verificar que a classe é aplicada

## 5. Frontend — UI de controles de tema

- [x] 5.1 Adicionar toggle claro/escuro e seletor de paleta de cores de destaque no canto superior de `DashboardView.vue`, ao lado da bolha de dark mode, e verificar que os controles aparecem
- [x] 5.2 Conectar os controles à store Pinia (mudança de cor e modo refletida imediatamente na UI) e verificar interatividade

## 6. Verificação de integração

- [x] 6.1 Escolher cor e modo na tela inicial, recarregar a página e verificar que a preferência é reaplicada (persistência no banco)
- [x] 6.2 Verificar que, sem preferência salva, o fallback é amarelo + segue o SO

## Workflow follow-up

- Quando a implementação estiver concluída e revisada, executar `/opsx-apply` para aplicar a mudança.
- Após a aplicação, arquivar a mudança com `/opsx-archive` se os requisitos de revisão do projeto forem atendidos.
