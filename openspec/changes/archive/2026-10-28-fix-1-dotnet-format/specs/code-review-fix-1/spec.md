# Spec Delta

## Purpose

Corrigir o achado [1] da revisão de código: dotnet-format detectou diferenças de formatação em arquivos .cs

## ADDED Requirements

### Requirement: Corrigir achado [1]
O sistema SHALL corrigir o problema: dotnet-format detectou diferenças de formatação em arquivos .cs

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos src/PlexAutoplay.Application/IPlexApiClient.cs, src/PlexAutoplay.Domain/Entities/Marker.cs, src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs, src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs, src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs, src/PlexAutoplay.Web/Program.cs, src/PlexAutoplay.Web/Services/PlexApiClient.cs, src/PlexAutoplay.Web/Endpoints/SeriesEndpoints.cs
- **THEN** o achado [1] não é mais reportado na próxima execução de /code-review
