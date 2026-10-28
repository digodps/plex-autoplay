# Design

## Context

Achado [1] da revisão de código gerado por /code-review. Veja proposal.md para motivação.

## Goals / Non-Goals

**Goals:**
- Corrigir: dotnet-format detectou diferenças de formatação em arquivos .cs

**Non-Goals:**
- Alterar comportamento não relacionado ao achado

## Decisions

- Aplicar correção direta nos arquivos listados: src/PlexAutoplay.Application/IPlexApiClient.cs, src/PlexAutoplay.Domain/Entities/Marker.cs, src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs, src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs, src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs, src/PlexAutoplay.Web/Program.cs, src/PlexAutoplay.Web/Services/PlexApiClient.cs, src/PlexAutoplay.Web/Endpoints/SeriesEndpoints.cs

## Risks / Trade-offs

- Mudança localizada; risco baixo se limitada ao escopo do achado
