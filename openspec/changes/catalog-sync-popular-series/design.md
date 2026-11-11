# Design

## Context

O `CatalogSyncUseCase` existe em `src/PlexAutoplay.Application/UseCases/CatalogSyncUseCase.cs` e realiza upsert/remoção de bibliotecas e séries do Plex para o banco local. No entanto, ele não está registrado no container de DI nem exposto por nenhum endpoint. O `CatalogSyncUseCase` é o estágio 1 do ciclo de sincronização unificada (antes da varredura de marcadores), definido em `openspec/specs/plex-sync`.

## Goals / Non-Goals

**Goals:**
- Registrar `CatalogSyncUseCase` no DI.
- Expor `POST /api/sync/catalog` que executa a sincronização de catálogo.
- Retornar contagens de bibliotecas e séries inseridas/atualizadas/removidas.

**Non-Goals:**
- Agendamento contínuo de sincronização (pertence a `plex-sync` / `CatalogSyncUseCase` já trata o ciclo).
- Alteração no `CatalogSyncUseCase` (reuso sem modificação).
- Alteração no frontend.

## Decisions

1. **Reutilizar `CatalogSyncUseCase` existente sem modificações** em vez de criar novo serviço. Rationale: o use case já está completo e testado; o gap é apenas exposição via DI + endpoint.
2. **Expor `POST /api/sync/catalog` dentro de `SyncEndpoints.cs`** (arquivo existente com endpoints de sincronização). Rationale: coesão com endpoints de sincronização existentes (`/api/sync/status`, `/api/sync/cancel`, etc.).
3. **Registrar `CatalogSyncUseCase` como `AddSingleton`** em `ServiceCollectionExtensions.cs`, alinhado ao registro de outros use cases (ex.: `LibraryScanUseCase`, `GetSeriesQuery`).
