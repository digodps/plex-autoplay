# Proposal

## Why

O README descreve um dashboard para gerenciar marcadores de intro/créditos no Plex que gera `custom.json` para o PlexAutoSkip embutido e envia comandos de *seek* aos players. No entanto, a implementação atual está incompleta: o scan de bibliotecas é simulado (`Task.Delay`), o `SyncBackgroundService` é um *stub*, a API de marcadores não grava em `custom.json`, o WebSocket do PlexAutoSkip faz apenas *echo* (não interpreta mensagens do player nem envia *seek*), e endpoints listados no README (`/api/status`, `/config`, `/library/<section_key>`) não existem. Sem fechar essas lacunas, o fluxo prometido — biblioteca → marcadores → *auto-skip* — não funciona de ponta a ponta.

## What Changes

- Implementar scanner real de bibliotecas Plex: listar episódios por seção, mapear caminho local via `MEDIA_ROOT`, rodar `ffprobe` para detecção de intro/créditos e gravar resultados em SQLite e `custom.json`.
- Substituir o *stub* do `SyncBackgroundService` por varredura periódica real que orquestra a detecção de marcadores.
- Alterar a API de marcadores para persistir em `custom.json` (além do SQLite) ao criar/editar/remover marcadores.
- Implementar o WebSocket do PlexAutoSkip para interpretar mensagens do protocolo Plex Companion e responder com comandos de *seek* (pular intro/créditos).
- Adicionar endpoints ausentes: `GET /api/status`, `GET`/`POST /config`, `GET /library/<section_key>`.
- Adicionar proteção CSRF em rotas mutantes (`POST`/`PUT`/`DELETE`) conforme prometido no README.
- Substituir o endpoint `/metrics` *stub* por métricas Prometheus reais do OpenTelemetry e implementar `/api/status` com métricas de processo (CPU, RAM, rede, disco).
- Consumir variáveis de ambiente documentadas no README que hoje são ignoradas (`WS_PORT`, `LOG_LEVEL`, `ALLOWED_HOSTS`, `APP_CONFIG_PATH`, `OTEL_EXPORTER`, `OTEL_ENDPOINT`).
- Ajustar o README para remover a contradição com o spec `config-env` (o sistema lê config exclusivamente de variáveis de ambiente, não de `app-config.json`) e corrigir `ws://` vs `wss://` na porta 8081.

## Capabilities

### New Capabilities
- `plex-scan-real`: Scanner real que lista episódios do Plex, resolve caminhos locais via `MEDIA_ROOT`, executa `ffprobe` para detecção de intro/créditos e persiste marcadores em SQLite e `custom.json`.
- `plex-autoplay-seek`: Servidor WebSocket (WSS na porta 8081) que interpreta mensagens do protocolo Plex Companion e envia comandos de *seek* para pular segmentos de intro e créditos nos players compatíveis.

### Modified Capabilities
- `plex-sync`: O `BackgroundService` passa de *stub* para orquestrar scan periódico real de bibliotecas de séries com detecção de marcadores.
- `plex-markers`: A API de criação/edição/remoção de marcadores passa a escrever em `custom.json` (formato PlexAutoSkip) além do SQLite.
- `plex-auto-skip`: O WebSocket passa de *echo* para interpretação de mensagens do player e envio de comandos de *seek*.
- `plex-libraries`: Passa a incluir mapeamento de episódios para arquivos locais via `MediaPart.file` + `MEDIA_ROOT`.
- `telemetry`: Métricas Prometheus passam a ser reais (não *stub* hardcoded) e o endpoint `/api/status` passa a expor métricas de processo e último job.
- `config-env`: Novo requisito de que todas as variáveis documentadas no README (`WS_PORT`, `LOG_LEVEL`, `ALLOWED_HOSTS`, `APP_CONFIG_PATH`, `OTEL_EXPORTER`, `OTEL_ENDPOINT`) sejam consumidas exclusivamente por variáveis de ambiente.

## Impact

- Código afetado: `src/PlexAutoplay.Web/Program.cs` (novos endpoints, WebSocket real, CSRF), `src/PlexAutoplay.Infrastructure/Sync/SyncBackgroundService` (scan real), `src/PlexAutoplay.Infrastructure/AutoSkip/PlexAutoSkipService` (interpretação de mensagens + seek), `src/PlexAutoplay.Application/UseCases/ScanLibrariesUseCase.cs` (listagem de episódios + mapeamento de caminho), novas classes para consumo de episódios do Plex e escrita de `custom.json`.
- APIs afetadas: `/api/status`, `/config`, `/library/<section_key>`, `/ws` (mudança de comportamento), rotas mutantes (CSRF).
- Dependências: pode ser necessário adicionar pacote NuGet para manipulação de `custom.json` ou reuso do `ImageCacheService` para cache de imagens.
- Especs afetados: novos specs `plex-scan-real` e `plex-autoplay-seek`; delta specs em `plex-sync`, `plex-markers`, `plex-auto-skip`, `plex-libraries`, `telemetry`, `config-env`.
- Documentação: atualização do README para alinhar com o spec `config-env` e corrigir esquema de porta WebSocket.
