# Tasks

## 1. Portas puras e adaptadores na camada certa

- [x] 1.1 Mover `Domain/ProcessInfoService.cs` para `Infrastructure/System/ProcessInfoService.cs` (mesmo namespace de impl `PlexAutoplay.Infrastructure.System`), manter `IProcessInfoService` em `Domain/Ports` e ajustar o registro em `ServiceCollectionExtensions`; verificar `dotnet build`
- [x] 1.2 Mover `Web/Services/PlexApiClient.cs` e `PlexApiClient.Series.cs` para `Infrastructure/PlexProxy/`, implementando `IPlexApiClient`; ajustar namespaces e o registro; verificar `dotnet build`
- [x] 1.3 Aplicar SRP no `PlexApiClient`: separar transporte HTTP de parsing de resposta (`PlexResponseParser` ou equivalente), reduzindo a classe a orquestração HTTP; verificar `dotnet build`
- [x] 1.4 Substituir o parsing manual de resposta (`TryGetProperty`/`if-else` de casing) por desserialização tipada com `System.Text.Json` (`PropertyNameCaseInsensitive`); verificar `dotnet build`
- [x] 1.5 Criar porta `IImageProxyService` em `Application` e fazer `ImageEndpoints` depender da abstração; registrar impl em Infrastructure; verificar `dotnet build`
- [x] 1.6 Mover `Web/Services/ScanJobStore.cs` para `Infrastructure/` preservando `IScanJobStore`; ajustar registro; verificar `dotnet build`
- [x] 1.7 Extrair porta `IPathResolver` (Application) e implementação com I/O em Infrastructure; `MediaPathResolver` deixa de chamar `File.Exists`; verificar `dotnet build`
- [x] 1.8 Extrair porta `IPlexConfigStore` (Application) e implementação que lê/escreve `.env` em Infrastructure; remover `File.*` de `PlexSaveEndpoint`; verificar `dotnet build`

## 2. Inicialização de schema assíncrona

- [x] 2.1 Criar `Infrastructure/Persistence/DatabaseInitializer.cs` com lista versionada de migrações cobrindo os 4 repositórios (`Markers`, `UserThemePreferences`, `Libraries`, `Series`), com API assíncrona
- [x] 2.2 Remover `EnsureTableCreated` e `Directory.CreateDirectory` dos construtores de `SqliteMarkerRepository`, `SqliteSeriesRepository`, `SqliteLibraryRepository`, `SqliteUserThemeRepository`; remover `catch` nu e `.Wait()` associados
- [x] 2.3 Chamar `await DatabaseInitializer.InitializeAsync()` no composition root (`Program.cs`) antes de `app.Run()`; verificar `dotnet build` e que a app sobe

## 3. Casos de uso como fronteira

- [x] 3.1 Criar `LibraryScanUseCase` (Application) com o fluxo skip→resolve→detect→grava; substituir a lógica inline em `SyncBackgroundService.RunScanAsync` e em `/api/plex/scan` por chamada ao caso de uso; remover `ScanLibrariesUseCase` morto
- [x] 3.2 Criar casos de uso de leitura/escrita faltantes (`GetMarkersQuery`, `GetSeriesQuery`, `GetThemeQuery`, `SaveThemeCommand`, `LoginUseCase` conforme rotas)
- [x] 3.3 Refatorar `SaveMarkerUseCase` para retornar resultado (`Created`/`AlreadyExists`) e ajustar `/api/markers` POST para mapear 201/409, removendo o código paralelo
- [x] 3.4 Fazer `ThemeEndpoints`, `MarkerEndpoints` (read+write), `PlexEndpoints`, `SeriesEndpoints` consumirem casos de uso; remover injeção de `IMarkerRepository`/`ISeriesRepository`/`ILibraryRepository`/`IUserThemeRepository` dos endpoints; verificar `dotnet build`

## 4. Higiene (erro, async, contrato tipado)

- [x] 4.1 Substituir todo `catch` nu por tipo específico + log via `ILogger` (`JwtTokenService` ×2, `SqliteMarkerRepository`, `AutoSkipWebSocketEndpoints`)
- [x] 4.2 Tornar `IPlexAutoSkipService` assíncrono (`GetMarkersForEpisodeAsync`); atualizar `PlexAutoSkipService` e `AutoSkipWebSocketEndpoints` para `await`; remover `GetAwaiter().GetResult()`
- [x] 4.3 Remover `ReadToEndAsync().Result` de `AuthEndpoints` e `ExtractPassword` (regex) substituindo por binding tipado de DTO com validação (400 em formato inválido)
- [x] 4.4 Substituir o parsing manual de query vs corpo em `/api/markers` POST por binding tipado (DTO aceitando query e corpo); verificar `dotnet build`
- [x] 4.5 Remover `PlexWebSaveEndpoint` (código morto não registrado) e a classe associada; verificar `dotnet build`

## 5. Enforcement e verificação

- [x] 5.1 Adicionar teste de arquitetura (`NetArchTest` ou equivalente) no projeto de teste: Domain não referencia Infrastructure; Application não referencia Web; Web não contém tipo que implementa porta de persistência/rede; verificar que o teste falha numa violação proposital
- [x] 5.2 Rodar `dotnet build` limpo (sem warning novo) e `grep` de aceite: zero `catch` nu, zero `.Wait()`/`.Result`/`GetAwaiter`, zero endpoint injetando repositório, Domain sem `System.Diagnostics/Data/Net.Http`
- [x] 5.3 Rodar os 13 testes e2e Playwright e confirmar verdes
