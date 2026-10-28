# Proposal

## Why

Correção do achado [1] da revisão de código: dotnet-format detectou diferenças de formatação em arquivos .cs

## What Changes

- Corrigir o problema identificado: dotnet-format detectou diferenças de formatação em arquivos .cs
- Arquivos afetados: src/PlexAutoplay.Application/IPlexApiClient.cs, src/PlexAutoplay.Domain/Entities/Marker.cs, src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs, src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs, src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs, src/PlexAutoplay.Web/Program.cs, src/PlexAutoplay.Web/Services/PlexApiClient.cs, src/PlexAutoplay.Web/Endpoints/SeriesEndpoints.cs

## Capabilities

### New Capabilities

- code-review-fix-1: Correção específica do achado [1] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Application/IPlexApiClient.cs, src/PlexAutoplay.Domain/Entities/Marker.cs, src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs, src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs, src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs, src/PlexAutoplay.Web/Program.cs, src/PlexAutoplay.Web/Services/PlexApiClient.cs, src/PlexAutoplay.Web/Endpoints/SeriesEndpoints.cs
- Referências:   Clean Code - formatação consistente

## Resultado da aplicação

dotnet format PlexAutoplay.slnx --include <8 arquivos> executado com sucesso; build 0 erros; 18/18 testes unitários verdes.
