# Tasks

## 1. Registrar CatalogSyncUseCase no DI

- [ ] 1.1 Adicionar `services.AddSingleton<CatalogSyncUseCase>();` em `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs`.
- [ ] 1.2 Verificar que `CatalogSyncUseCase` resolve `IPlexApiClient`, `ILibraryRepository`, `ISeriesRepository` (já registrados).

## 2. Expor endpoint POST /api/sync/catalog

- [ ] 2.1 Adicionar `app.MapPost("/api/sync/catalog", ...)` em `src/PlexAutoplay.Web/Endpoints/SyncEndpoints.cs` que injeta `CatalogSyncUseCase`, chama `RunAsync()` e retorna 200 com contagens ou 502 em falha do Plex.

## 3. Build, QA e validação

- [ ] 3.1 `dotnet build PlexAutoplay.slnx -c Release` passa com 0 erros.
- [ ] 3.2 `openspec validate catalog-sync-popular-series --strict` passa.
- [ ] 3.3 Build do container Docker (`docker compose build`) passa.
- [ ] 3.4 QA UI Playwright contra `http://localhost:8080`: tema light/dark toggle e menus (abertura/navegação/fechamento/estado ativo) continuam funcionando.
