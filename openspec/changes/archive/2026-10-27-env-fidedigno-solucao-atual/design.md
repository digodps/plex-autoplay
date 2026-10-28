# Design

## Context

O projeto já reescrito em .NET 10 tem um `.env` com ~29 variáveis, mas apenas 8 são efetivamente consumidas pelo código ou infraestrutura. Várias variáveis órfãs (ex: `REDIS_URL`, `WEBHOOK_SECRET`, `DASHBOARD_*`, `AUTO_SKIP_INTRO`, etc.) são injetadas no container sem consumo nenhum. Por outro lado, `APP_CONFIG_PATH` já é lida por `SyncBackgroundService` mas não está declarada em nenhum `.env`, e o próprio `SyncBackgroundService` nunca é registrado como `HostedService`, então `SYNC_INTERVAL_MINUTES` não tem efeito prático.

A change de telemetria (`implementar-telemetria-ponta-a-ponta`) já terminou e é dona de `OpenTelemetryService.cs`. Ela injeta `OTEL_EXPORTER` e `OTEL_ENDPOINT` via `docker-compose.yml`, mas o código ainda não as consome. Este design não toca nesse arquivo.

## Goals / Non-Goals

**Goals:**
- Tornar `.env` e `.env.example` fidedignos: apenas variáveis consumidas pela solução atual
- Declarar explicitamente `APP_CONFIG_PATH` (já lida pelo código)
- Registrar `SyncBackgroundService` para que `SYNC_INTERVAL_MINUTES` funcione
- Manter `OTEL_EXPORTER`/`OTEL_ENDPOINT` no ambiente sem mexer em `OpenTelemetryService.cs`

**Non-Goals:**
- Alterar comportamento de specs existentes (`plex-auth`, `plex-sync`, `plex-auto-skip`, etc.)
- Implementar leitura de `OTEL_EXPORTER`/`OTEL_ENDPOINT` no código (escopo da change de telemetria)
- Criar novas capabilities de negócio

## Decisions

**1. Remover variáveis órfãs do `.env` e `.env.example`**
- Variáveis removidas: `SETUP_TOKEN`, `AUTO_SKIP_INTRO`, `AUTO_NEXT_EPISODE`, `NEXT_EPISODE_THRESHOLD_SECONDS`, `ACTION_DELAY_SECONDS`, `POLL_INTERVAL_SECONDS`, `ALLOWED_CLIENT_IDS`, `ALLOWED_USERNAMES`, `DASHBOARD_COOKIE_SECURE`, `DASHBOARD_HTTPS`, `DASHBOARD_TLS_HOSTS`, `LOG_LEVEL`, `WEBHOOK_SECRET`, `REDIS_URL`, e as comentadas `PLEX_AUTOPLAY_CUSTOM_JSON_PATH`, `PLEX_AUTOPLAY_CONFIG_DIR`.
- Rationale: nenhuma delas é lida por `Program.cs`, `OpenTelemetryService.cs`, `SyncBackgroundService` ou qualquer outro arquivo .NET. São injetadas no container sem propósito.
- Alternativa considerada: manter como documentação futura. Rejeitada: polui o container e viola o objetivo de fidedignidade.

**2. Adicionar `APP_CONFIG_PATH=/data/app-config.json`**
- Rationale: `SyncBackgroundService.cs` lê `configuration.GetValue("APP_CONFIG_PATH", "/data/app-config.json")`. Declarar no `.env` torna o valor explícito e rastreável.
- Alternativa considerada: manter apenas o default hardcoded no código. Rejeitada: menos visível para operadores.

**3. Registrar `SyncBackgroundService` como `HostedService` em `Program.cs`**
- Rationale: o serviço existe e lê `SYNC_INTERVAL_MINUTES`, mas nunca é iniciado. Sem registro, a variável está no `.env` para nada.
- Local: `Program.cs`, após `builder.Services.AddOpenTelemetryTelemetry(builder.Configuration)`.

**4. Não alterar `OpenTelemetryService.cs`**
- Rationale: a change de telemetria já terminou e é dona desse arquivo. `OTEL_EXPORTER`/`OTEL_ENDPOINT` continuam declaradas no `docker-compose.yml` e `.env` como infraestrutura, prontas para consumo futuro.

**5. Manter `MEDIA_ROOT`, `WEB_PORT`, `WS_PORT` como infraestrutura/script**
- `MEDIA_ROOT`: usada apenas no `docker-compose.yml` para mount de volume; o .NET não lê.
- `WEB_PORT`/`WS_PORT`: usadas apenas no `start.sh` para exibição; portas internas fixas em 8080/8081.

## Risks / Trade-offs

[Remover uma variável que alguém esteja injetando manualmente no runtime externo] → Mitigar comunicando a mudança; variáveis órfãs não afetam a aplicação.

[Esquecer alguma variável lida indiretamente] → Mitigar com `openspec validate` e revisão de `grep` sobre `GetEnvironmentVariable` e `IConfiguration` antes de aplicar.

[Registro de `SyncBackgroundService` alterar comportamento de scan em produção] → Mitigar: o serviço já existe no código, apenas não era iniciado; o intervalo padrão continua 5 minutos.

## Migration Plan

1. Aplicar as edições em `.env`, `.env.example` e `Program.cs`.
2. Fazer `docker compose build` e reiniciar o container.
3. Verificar que o container sobe com `APP_CONFIG_PATH` e que `SYNC_INTERVAL_MINUTES` passa a afetar o serviço de sync.
4. Rollback: restaurar `.env` e `.env.example` do git e desfazer o registro de `HostedService` em `Program.cs`.

## Open Questions

Nenhuma.
