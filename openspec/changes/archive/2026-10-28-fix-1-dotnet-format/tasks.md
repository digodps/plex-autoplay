# Tasks

## 1. Correção do achado [1]

- [x] 1.1 Aplicar correção nos arquivos: src/PlexAutoplay.Application/IPlexApiClient.cs, src/PlexAutoplay.Domain/Entities/Marker.cs, src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs, src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs, src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs, src/PlexAutoplay.Web/Program.cs, src/PlexAutoplay.Web/Services/PlexApiClient.cs, src/PlexAutoplay.Web/Endpoints/SeriesEndpoints.cs e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

dotnet format PlexAutoplay.slnx --include <8 arquivos> executado com sucesso; build 0 erros; 18/18 testes unitários verdes.

## Workflow follow-up

- Validar com `openspec validate --change fix-1-dotnet-format` antes de seguir para archive/apply.
