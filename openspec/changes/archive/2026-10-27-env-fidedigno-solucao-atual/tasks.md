# Tasks

## 1. Limpar variáveis órfãs do `.env`

- [x] 1.1 Remover do `.env` as variáveis órfãs: `SETUP_TOKEN`, `AUTO_SKIP_INTRO`, `AUTO_NEXT_EPISODE`, `NEXT_EPISODE_THRESHOLD_SECONDS`, `ACTION_DELAY_SECONDS`, `POLL_INTERVAL_SECONDS`, `ALLOWED_CLIENT_IDS`, `ALLOWED_USERNAMES`, `DASHBOARD_COOKIE_SECURE`, `DASHBOARD_HTTPS`, `DASHBOARD_TLS_HOSTS`, `LOG_LEVEL`, `WEBHOOK_SECRET`, `REDIS_URL`, e as comentadas `PLEX_AUTOPLAY_CUSTOM_JSON_PATH`, `PLEX_AUTOPLAY_CONFIG_DIR`. Verificar que `grep -E 'GetEnvironmentVariable|IConfiguration|configuration\[' src/` não referencia nenhuma delas.
- [x] 1.2 Adicionar `APP_CONFIG_PATH=/data/app-config.json` ao `.env` na seção de sincronização.

## 2. Limpar variáveis órfãs do `.env.example`

- [x] 2.1 Espelhar as mesmas remoções de 1.1 no `.env.example` (incluindo as linhas comentadas).
- [x] 2.2 Adicionar `APP_CONFIG_PATH=/data/app-config.json` ao `.env.example`.

## 3. Registrar `SyncBackgroundService` como `HostedService`

- [x] 3.1 Em `src/PlexAutoplay.Web/Program.cs`, adicionar `builder.Services.AddHostedService<SyncBackgroundService>();` após o registro de telemetria, com o `using` correspondente de `PlexAutoplay.Infrastructure.Sync`.
- [x] 3.2 Verificar que `dotnet build` compila com 0 erros e que `SYNC_INTERVAL_MINUTES` agora é efetivamente consumida.

## 4. Verificação de integração

- [x] 4.1 Rodar `docker compose build` e verificar que o container inicia com as novas variáveis de ambiente.
- [x] 4.2 Confirmar que `OpenTelemetryService.cs` não foi alterado (respeitar escopo da change de telemetria).
- [x] 4.3 Validar com `openspec validate` que a mudança está coerente.

## Workflow follow-up

- Revisar a mudança com o time antes de aplicar.
- Após aprovação, executar `/opsx-apply` para implementar as tarefas acima.
- Archivar a mudança após revisão e aplicação.
