# Proposal

## Why

O `plex-autoplay` lê parte de sua configuração de arquivos JSON internos (`app-config.json`, `appsettings.json`) em vez de variáveis de ambiente. Isso obriga o operador a entrar no container ou editar arquivos no volume persistente para alterar configurações simples (ex: intervalo de sincronização, nível de log), inviabilizando a edição direta via `docker-compose.yml` / `.env`.

## What Changes

- `SYNC_INTERVAL_MINUTES` passa a ser a fonte única de verdade; remover leitura de fallback do `app-config.json` e o `ReloadInterval()` que re-lê o JSON a cada ciclo
- `LOG_LEVEL` passa a ser configurável via variável de ambiente, substituindo o valor fixo em `appsettings.json`
- `ALLOWED_HOSTS` passa a ser declarável via variável de ambiente
- `APP_CONFIG_PATH` deixa de ser necessária (o `app-config.json` não será mais lido para configuração)

**BREAKING**: alterações feitas diretamente em `app-config.json` não terão mais efeito.

## Capabilities

### New Capabilities

- `config-env`: Gerenciamento de configuração exclusivamente via variáveis de ambiente, removendo dependência de arquivos JSON internos para valores configuráveis.

### Modified Capabilities

- `plex-sync`: O requisito de intervalo configurável deixa de ler fallback do `app-config.json`, passando a depender exclusivamente de `SYNC_INTERVAL_MINUTES`.

## Impact

- `src/PlexAutoplay.Infrastructure/Sync/SyncBackgroundService.cs` — remover leitura de `app-config.json` e `ReloadInterval()`
- `src/PlexAutoplay.Web/appsettings.json` — remover `LogLevel` fixo (virá de env var)
- `src/PlexAutoplay.Web/Program.cs` — possivelmente configurar logging a partir de env var
- `.env` / `.env.example` — adicionar `SYNC_INTERVAL_MINUTES`, `LOG_LEVEL`, `ALLOWED_HOSTS`
- `docker-compose.yml` — injetar novas variáveis de ambiente
