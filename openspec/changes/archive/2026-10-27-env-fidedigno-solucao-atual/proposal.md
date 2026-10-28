# Proposal

## Why

O arquivo `.env` e o `docker-compose.yml` injetam variáveis de ambiente no container que não são consumidas pelo código .NET (variáveis órfãs), enquanto variáveis já lidas pelo código (como `APP_CONFIG_PATH`) estão ausentes da declaração. Isso faz o container subir com configuração "lixo" e deixa variáveis úteis sem declaração explícita, afastando o `.env` da fidedignidade com a solução atual.

## What Changes

- Remover variáveis órfãs de `.env` e `.env.example` que nada consomem: `SETUP_TOKEN`, `AUTO_SKIP_INTRO`, `AUTO_NEXT_EPISODE`, `NEXT_EPISODE_THRESHOLD_SECONDS`, `ACTION_DELAY_SECONDS`, `POLL_INTERVAL_SECONDS`, `ALLOWED_CLIENT_IDS`, `ALLOWED_USERNAMES`, `DASHBOARD_COOKIE_SECURE`, `DASHBOARD_HTTPS`, `DASHBOARD_TLS_HOSTS`, `LOG_LEVEL`, `WEBHOOK_SECRET`, `REDIS_URL`, e as comentadas `PLEX_AUTOPLAY_CUSTOM_JSON_PATH`, `PLEX_AUTOPLAY_CONFIG_DIR`.
- Adicionar `APP_CONFIG_PATH=/data/app-config.json` a `.env` e `.env.example` (já lida por `SyncBackgroundService`, mas nunca declarada).
- Registrar `SyncBackgroundService` como `HostedService` em `Program.cs` para que `SYNC_INTERVAL_MINUTES` tenha efeito prático.
- Manter `OTEL_EXPORTER` e `OTEL_ENDPOINT` declaradas no ambiente (infraestrutura) sem alterar `OpenTelemetryService.cs` (responsabilidade da change de telemetria já concluída).

## Capabilities

### New Capabilities

Nenhuma. Esta mudança não introduz novo comportamento de sistema.

### Modified Capabilities

Nenhum. Os specs existentes (`plex-auth`, `plex-sync`, `plex-auto-skip`, `plex-images`, `plex-libraries`, `plex-markers`) continuam inalterados em nível de requisito. Esta é uma mudança de configuração e limpeza, não de comportamento.

## Impact

- Arquivos alterados: `.env`, `.env.example`, `src/PlexAutoplay.Web/Program.cs`
- Arquivos intocados: `src/PlexAutoplay.Infrastructure/Telemetry/OpenTelemetryService.cs` (escopo da change de telemetria)
- Não altera specs existentes nem cria novas capabilities de negócio.
