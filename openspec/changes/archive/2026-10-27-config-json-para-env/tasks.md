# Tasks

## 1. Remover leitura de app-config.json do SyncBackgroundService

- [x] 1.1 Remover o método `ReadIntervalFromConfigFile` e a chamada a `APP_CONFIG_PATH`/`_configFilePath` do construtor do `SyncBackgroundService`, mantendo apenas `SYNC_INTERVAL_MINUTES` via `IConfiguration`. Verificar que `dotnet build` compila com 0 erros.
- [x] 1.2 Remover o método `ReloadInterval()` e sua chamada no loop `ExecuteAsync`, para que o intervalo não seja re-lido de arquivo a cada ciclo. Verificar que o serviço ainda inicia com o valor da variável de ambiente.

## 2. Configurar LOG_LEVEL via variável de ambiente

- [x] 2.1 Remover a seção `LogLevel` fixa de `src/PlexAutoplay.Web/appsettings.json` (manter a estrutura de logging sem valores hardcoded).
- [x] 2.2 Adicionar `LOG_LEVEL=Information` ao `.env` e `.env.example` na seção de ambiente.
- [x] 2.3 Adicionar `- LOG_LEVEL=${LOG_LEVEL:-Information}` ao `docker-compose.yml` na seção de environment.

## 3. Declarar ALLOWED_HOSTS via variável de ambiente

- [x] 3.1 Adicionar `ALLOWED_HOSTS=*` ao `.env` e `.env.example`.
- [x] 3.2 Adicionar `- ALLOWED_HOSTS=${ALLOWED_HOSTS:-*:*}` ao `docker-compose.yml` na seção de environment.

## 4. Declarar SYNC_INTERVAL_MINUTES no docker-compose

- [x] 4.1 Adicionar `- SYNC_INTERVAL_MINUTES=${SYNC_INTERVAL_MINUTES:-5}` ao `docker-compose.yml` na seção de environment.

## 5. Verificação de integração

- [x] 5.1 Verificar que `dotnet build` compila com 0 erros após todas as mudanças.
- [x] 5.2 Verificar que `docker compose build` compila com 0 erros.
- [x] 5.3 Rodar `openspec validate` para confirmar que a mudança está coerente.

## Workflow follow-up

- Revisar a mudança com o time antes de aplicar.
- Após aprovação, executar `/opsx-apply` para implementar as tarefas acima.
- Archivar a mudança após revisão e aplicação.
