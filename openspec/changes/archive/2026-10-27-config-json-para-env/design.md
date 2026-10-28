# Design

## Context

O `plex-autoplay` lê parte de sua configuração de arquivos JSON internos:
- `SyncBackgroundService` lê `APP_CONFIG_PATH` (default `/data/app-config.json`) e faz fallback para `syncIntervalMinutes` dentro desse JSON, além de re-ler o arquivo a cada ciclo via `ReloadInterval()`
- `appsettings.json` define `LogLevel` fixo (`Information` para Default, `Warning` para Microsoft.AspNetCore) sem override por variável de ambiente
- `AllowedHosts` é fixo em `*` no `appsettings.json`

A mudança de HTTPS (`https-somente-certificado`) já introduziu variáveis de ambiente para certificado e porta, mas não removeu a dependência de JSON para configuração geral.

## Goals / Non-Goals

**Goals:**
- `SYNC_INTERVAL_MINUTES` seja a fonte única para o intervalo de sincronização
- `LOG_LEVEL` seja configurável via variável de ambiente
- `ALLOWED_HOSTS` seja declarável via variável de ambiente
- Remover leitura de `app-config.json` para fins de configuração
- Permitir edição de todas essas configurações diretamente no `docker-compose.yml` / `.env`

**Non-Goals:**
- Converter `custom.json` para variável de ambiente (é dados de varredura de marcadores, não configuração do operador)
- Criar endpoint de settings no frontend para gravar configuração
- Suportar múltiplos formatos de arquivo de configuração além de variáveis de ambiente

## Decisions

**1. SYNC_INTERVAL_MINUTES como fonte única**
- Rationale: já existe como variável de ambiente; o fallback para JSON adiciona complexidade e dificulta operação
- Alternativa considerada: manter fallback para JSON. Rejeitada: obriga o operador a editar arquivos dentro do container

**2. Remover ReloadInterval() e leitura de app-config.json**
- Rationale: se a env var é a fonte única, não há necessidade de re-ler arquivo a cada ciclo
- Alternativa considerada: manter leitura periódica do JSON como override. Rejeitada: comportamento confuso (qual fonte vence?)

**3. LOG_LEVEL via variável de ambiente**
- Rationale: .NET já suporta override de logging via variáveis de ambiente (`Logging__LogLevel__Default`); expor como `LOG_LEVEL` simplifica para o operador
- Alternativa considerada: manter fixo em `appsettings.json`. Rejeitada: impossibilita ajuste de log sem recriar imagem

**4. ALLOWED_HOSTS via variável de ambiente**
- Rationale: o .NET já lê `AllowedHosts` de configuration; apenas precisa ser declarada no `.env`/`docker-compose.yml`
- Alternativa considerada: manter fixo em `*`. Rejeitada: restrição desnecessária para implantações atrás de proxy

## Risks / Trade-offs

[Remover leitura de app-config.json pode quebrar implantações que já usam esse arquivo] → Mitigar: documentar claramente na Migration Plan que valores em `app-config.json` não terão mais efeito; comunicar a mudança antes de aplicar

[Valor padrão hardcoded no código pode não atender todos os casos] → Mitigar: usar valores padrão razoáveis (5 minutos para sync, Information para log, * para allowed hosts) e documentar como sobrescrever

[Variáveis de ambiente são visíveis em `docker inspect` e processos] → Mitigar: não é novo risco (já existem `PLEX_TOKEN`, `JWT_SECRET` etc. como env vars); usar Docker Secrets para produção se necessário

## Migration Plan

1. Aplicar mudanças em `SyncBackgroundService.cs`, `appsettings.json`, `.env`, `.env.example`, `docker-compose.yml`
2. Rebuild da imagem: `docker compose build`
3. Reiniciar container: `docker compose up -d`
4. Verificar que o intervalo de sincronização segue o valor de `SYNC_INTERVAL_MINUTES`
5. Rollback: restaurar `.env`, `.env.example`, `docker-compose.yml`, `SyncBackgroundService.cs` e `appsettings.json` do git

## Open Questions

Nenhuma.
