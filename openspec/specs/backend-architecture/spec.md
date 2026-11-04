# backend-architecture Specification

## Purpose
Definir o comportamento da camada de aplicação do plex-autoplay conforme
Clean Architecture, SOLID e Clean Code: portas em Domain/Application, adaptadores
concretos em Infrastructure, Web como fachada sem lógica de negócio,
inicialização assíncrona de schema, erro tratado explícito e nenhum
sync-over-async.

## Requirements

### Requirement: Nenhuma implementação concreta no Domain
The Domain layer SHALL contain only entities and port interfaces. No concrete
implementation may reference `System.Diagnostics`, `System.Data`, or
`System.Net.Http`, and no I/O (filesystem, SQL, HTTP) may run in Domain.

#### Scenario: Domain layer stays free of external dependencies
- **WHEN** any `.cs` file under `PlexAutoplay.Domain` is inspected
- **THEN** it contains no `using System.Diagnostics`/`System.Data`/`System.Net.Http`, no `new` of external adapters, and no filesystem/network call

### Requirement: Adapters in Infrastructure with single responsibility
The system SHALL place every concrete external adapter (HTTP client, filesystem,
configuration store, job store, auth token service) in `PlexAutoplay.Infrastructure`,
each with a single responsibility; `PlexApiClient` SHALL separate HTTP transport
from response parsing, using typed JSON deserialization (no `TryGetProperty`
casing branches).

#### Scenario: No concrete adapter outside Infrastructure
- **WHEN** any `.cs` file under `PlexAutoplay.Web` or `PlexAutoplay.Application` is inspected
- **THEN** no file instantiates a repository, HTTP client adapter, or file-based store directly

### Requirement: Endpoints are a façade (read and write)
Every HTTP endpoint SHALL delegate business behavior to an Application use case
and return a DTO response; no endpoint SHALL inject `IMarkerRepository`,
`ISeriesRepository`, `ILibraryRepository` or `IUserThemeRepository` directly.

#### Scenario: Marker write goes through a use case
- **WHEN** a POST to `/api/markers` is issued
- **THEN** the behavior runs inside `SaveMarkerUseCase`, which returns `Created` or `AlreadyExists`, translated to HTTP 201 or 409

#### Scenario: Marker read goes through a use case
- **WHEN** a GET to `/api/markers` or `/api/markers/{ratingKey}` is issued
- **THEN** the data comes from a query use case, not from direct repository access in the endpoint

### Requirement: Single scan orchestrator in Application
The system SHALL define `LibraryScanUseCase` in Application implementing skip-if-done → resolve path → detect → store markers, consumed by both `SyncBackgroundService` and `POST /api/plex/scan`, removing the duplicated inline implementation and the unused `ScanLibrariesUseCase`.

#### Scenario: Scan logic lives in one place
- **WHEN** a sync run or an explicit scan request occurs
- **THEN** both paths call `LibraryScanUseCase`; `ScanLibrariesUseCase` no longer exists

### Requirement: Async schema initialization outside repository constructors
The system SHALL execute `CREATE TABLE` / `ALTER TABLE` migrations for all repositories through `DatabaseInitializer` (Infrastructure), called `await`ed in the composition root before serving traffic; repository constructors SHALL have no DDL, no `Directory.CreateDirectory`, and no `.Wait()`.

#### Scenario: Repository constructor is pure DI
- **WHEN** a repository is constructed via DI
- **THEN** its constructor performs no filesystem, DDL, or blocking call

### Requirement: No error swallowed
The system SHALL replace every bare `catch` with a typed catch and explicit logging via `ILogger`.

#### Scenario: Typed exception handling
- **WHEN** token validation or SQLite DDL fails
- **THEN** the caught exception type is declared and logged, never silently discarded

### Requirement: Fully async execution path
The system SHALL remove every `.Wait()`, `.Result`, and
`GetAwaiter().GetResult()` from persistence and network paths; `IPlexAutoSkipService`
SHALL be asynchronous.

#### Scenario: Async all the way
- **WHEN** an async method is invoked in a persistence or network path
- **THEN** it is awaited; no synchronous blocking remains

### Requirement: Typed input binding
The system SHALL replace manual payload parsing (query vs body casing, regex
extraction) with typed `[FromQuery]`/`[FromBody]` DTO binding, returning HTTP 400
on invalid input.

#### Scenario: Malformed body yields 400
- **WHEN** an endpoint receives an invalid JSON body
- **THEN** binding validation returns 400 instead of a parse exception

### Requirement: Configuration I/O outside Web
The system SHALL read/write `.env` configuration through `IPlexConfigStore`
(Application port, Infrastructure implementation); Web endpoints SHALL call it
instead of calling `System.IO.File.*`.

#### Scenario: Config change via port
- **WHEN** an endpoint persists Plex config
- **THEN** it delegates to `IPlexConfigStore`, no direct file I/O in Web

### Requirement: No dead code
The system SHALL remove `PlexWebSaveEndpoint` (never registered) and any
unreferenced use case.

#### Scenario: Build without dead artifacts
- **WHEN** the project builds
- **THEN** `PlexWebSaveEndpoint` is not present nor referenced

### Requirement: Architecture enforcement
The system SHALL include an architecture test failing the build when Domain
references Infrastructure, Application references Web, or Web contains a type
implementing a persistence/network port.

#### Scenario: Architecture test rejects a layer violation
- **WHEN** a new file adds a forbidden cross-layer reference
- **THEN** the build fails

### Requirement: Media path resolution without I/O in Application
`MediaPathResolver` SHALL expose only path-building logic; filesystem existence
checks SHALL move behind an `IPathResolver` port implemented in Infrastructure.

#### Scenario: Path resolution has no filesystem access in Application
- **WHEN** the Application code resolves a media path
- **THEN** it delegates existence checks to `IPathResolver`, and `MediaPathResolver` contains no `File.Exists` call
