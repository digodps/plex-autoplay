# Proposal

## Why

O serviço de autoskip (WebSocket `/ws`) lê marcadores exclusivamente do arquivo `custom.json`, enquanto toda gravação (API, scan, sync) escreve no SQLite e espelha para o JSON. Isso mantém duas fontes de verdade para os mesmos marcadores (`intro`/`credits` por `RatingKey`), gerando inconsistência (por exemplo, `DELETE` não atualiza o JSON) e complexidade desnecessária (`FileSystemWatcher`, caminho de arquivo, variável `CUSTOM_JSON_PATH`).

## What Changes

- O `PlexAutoSkipService` passa a ler marcadores do SQLite (`IMarkerRepository`) em vez de `custom.json`, consultando o banco a cada mensagem de `play` do WebSocket.
- Remove a porta `ICustomJsonWriter` e a implementação `CustomJsonWriterService`.
- Remove o `FileSystemWatcher` e a variável de ambiente `CUSTOM_JSON_PATH` do autoskip.
- Endpoints de gravação (POST/PUT/DELETE `/api/markers`, scan `/api/plex/scan`, `SyncBackgroundService`) escrevem apenas no SQLite; deixam de gravar `custom.json`.
- `DELETE /api/markers/{ratingKey}/{kind}` passa a ser consistente (remove apenas do SQLite).
- **BREAKING**: `custom.json` não é mais gerado nem consumido pelo serviço; a variável `CUSTOM_JSON_PATH` é removida.

## Capabilities

### New Capabilities

(nenhum)

### Modified Capabilities

- `plex-auto-skip`: o requisito "Read custom.json for markers" muda para "Read markers from SQLite" — o WebSocket de autoskip consome marcadores do SQLite, não do arquivo JSON.
- `plex-markers`: o requisito "Persist markers in custom.json format" muda para persistência apenas em SQLite; a gravação em `custom.json` é removida.

## Impact

- `src/PlexAutoplay.Infrastructure/AutoSkip/*` (PlexAutoSkipService, CustomJsonWriterService)
- `src/PlexAutoplay.Domain/Ports/ICustomJsonWriter.cs`
- `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs`
- `src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs`
- `src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs`
- `src/PlexAutoplay.Infrastructure/Sync/SyncBackgroundService.cs`
- `tests/unit/MarkerEndpointsTests.cs`, `tests/unit/MarkerEndpointsFixture.cs`
- `openspec/specs/plex-auto-skip/spec.md` (delta spec)
- `openspec/specs/plex-markers/spec.md` (delta spec)
- `README.md`, `package.json`, `docker-compose.yml`, `stop.sh` (referências ao `custom.json`)
