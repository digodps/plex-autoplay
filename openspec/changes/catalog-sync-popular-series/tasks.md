# Tasks

## 1. Registrar CatalogSyncUseCase no DI

- [x] 1.1 Adicionar `services.AddTransient<CatalogSyncUseCase>();` em `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs`. (Transient, não Singleton, para evitar captive dependency em `IPlexApiClient` registrado como transient.)
- [x] 1.2 Verificado: `CatalogSyncUseCase` resolve `IPlexApiClient` (transient), `ILibraryRepository` e `ISeriesRepository` (singletons, já registrados).

## 2. Expor endpoint POST /api/sync/catalog

- [x] 2.1 Adicionado `POST /api/sync/catalog` em `src/PlexAutoplay.Web/Endpoints/SyncEndpoints.cs` que injeta `CatalogSyncUseCase`, chama `RunAsync()`, retorna 200 com contagens (inseridas/atualizadas/removidas) ou 502 em falha do Plex.

## 3. Build, QA e validação

- [x] 3.1 `dotnet build PlexAutoplay.slnx -c Release` passa com 0 erros (17 warnings pré-existentes).
- [x] 3.2 `openspec validate catalog-sync-popular-series --strict` passa.
- [x] 3.3 Build do container Docker (`docker compose build`) passa.
- [x] 3.4 QA UI Playwright contra `http://localhost:8080`: tema toggle e menus 3/3 PASS. Endpoint `POST /api/sync/catalog` 200 e `GET /api/series/1` com 9 séries (antes vazio).
