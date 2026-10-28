# Proposal

## Why

Correção do achado [6] da revisão de código: Classe com mais de 200 linhas (315) - possível violação do SRP (Responsabilidade Única)

## What Changes

- Corrigir o problema identificado: Classe com mais de 200 linhas (315) - possível violação do SRP (Responsabilidade Única)
- Arquivos afetados: src/PlexAutoplay.Web/Services/PlexApiClient.cs

## Capabilities

### New Capabilities

- code-review-fix-6: Correção específica do achado [6] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/Services/PlexApiClient.cs
- Referências:   SOLID - S (Single Responsibility Principle)

## Resultado da aplicação

PlexApiClient.cs dividido em partial classes por domínio: PlexApiClient.cs (165 linhas — campos, ctor, GetLibrariesAsync, GetEpisodesAsync) e PlexApiClient.Series.cs (165 linhas — GetSeriesAsync, GetSeriesEpisodesAsync). Build 0 erros; 18/18 unitários; E2E 23 passed + 2 skipped.
