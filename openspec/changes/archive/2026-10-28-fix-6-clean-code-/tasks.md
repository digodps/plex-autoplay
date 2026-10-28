# Tasks

## 1. Correção do achado [6]

- [x] 1.1 Aplicar correção nos arquivos: src/PlexAutoplay.Web/Services/PlexApiClient.cs e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

PlexApiClient.cs dividido em partial classes por domínio: PlexApiClient.cs (165 linhas — campos, ctor, GetLibrariesAsync, GetEpisodesAsync) e PlexApiClient.Series.cs (165 linhas — GetSeriesAsync, GetSeriesEpisodesAsync). Build 0 erros; 18/18 unitários; E2E 23 passed + 2 skipped.

## Workflow follow-up

- Validar com `openspec validate --change fix-6-clean-code-/-solid` antes de seguir para archive/apply.
