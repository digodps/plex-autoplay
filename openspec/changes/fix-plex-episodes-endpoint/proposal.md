# Proposal: fix-plex-episodes-endpoint

## Why

O teste de aceitação encontrou `GET /library/{sectionKey}` retornando `502`. O backend consulta `/library/sections/{sectionKey}/items`, que a API do Plex responde com `404`. A API oficial expõe os episódios de uma seção de séries em `/library/sections/{sectionKey}/all?type=4`.

O mesmo erro afeta `POST /api/plex/scan`: cada biblioteca termina com erro ao buscar episódios, impedindo a detecção de marcadores em instalações reais.

## What Changes

- Corrigir `PlexApiClient.GetEpisodesAsync` para consultar `/library/sections/{sectionKey}/all?type=4`.
- Manter o filtro defensivo por `type = "episode"` no parsing da resposta.
- Atualizar o mock Plex dos testes E2E para responder ao endpoint real.
- Adicionar teste unitário que garanta a URL e o parsing de episódios da seção.
- Validar com o teste de aceitação `scripts/acceptance-test.sh`.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `plex-libraries`: a listagem de episódios de uma biblioteca passa a usar o endpoint real do Plex.

## Impact

- `src/PlexAutoplay.Web/Services/PlexApiClient.cs`
- `tests/unit/PlexApiClientTests.cs`
- `tests/e2e/mock-plex/server.js`
- Nenhuma alteração de contrato HTTP público; o formato de `/library/{sectionKey}` permanece igual.
