# Design Doc v2 — Clean Architecture / Clean Code / SOLID (backend plex-autoplay)

> v1 revisada por 3 Goldfish (arquitetura, SOLID, Clean Code/escopo/regressão):
> 33 achados. v2 incorpora os aceitos; falsos positivos rejeitados em
> `deficiencias_resolvidas.md` com a evidência de verificação.

## 1. Problema

A solução tem o esqueleto de Clean Architecture (4 projetos, dependência para
dentro) mas a implementação fura as regras: adaptadores concretos na Web/Domain,
endpoints acessando repositório direto (leitura **e** escrita), regra de scan
duplicada em dois lugares, código morto não fiado, `catch` nu, e sync-over-async
em 3 repositórios + 2 endpoints + 1 serviço.

## 2. Estado atual (medido e verificado)

| projeto | arquivos | LOC |
|---|---|---|
| Domain | 14 | 203 |
| Application | 5 | 207 |
| Infrastructure | 12 | 1172 |
| Web | 22 | 1983 |

Grafo de projeto correto: `Domain ⊥` ← `Application` ← `Infrastructure` ← `Web`.

Violações verificadas por grep/leitura (evidência no registro de resolução):

- **I/O em Application**: `MediaPathResolver.cs:27` usa `File.Exists`.
- **Adaptador HTTP fora de Infrastructure**: `Web/Services/PlexApiClient.cs` (194)
  + `.Series.cs` (173), que também faz telemetria e parsing manual (SRP/OCP).
- **Adaptadores concretos na Web**: `ImageProxyService` injetado direto em
  `ImageEndpoints.cs:13`; `ScanJobStore` em `Web/Services`.
- **DDL + `Directory.CreateDirectory` + `.Wait()` no construtor** em 4
  repositórios: `SqliteMarkerRepository` (38,44), `SqliteSeriesRepository` (39),
  `SqliteLibraryRepository` (37), `SqliteUserThemeRepository`.
- **Sync-over-async**: `AuthEndpoints.cs:19` (`ReadToEndAsync().Result`),
  `PlexAutoSkipService.cs:19` (`GetAwaiter().GetResult()`).
- **`catch` nu**: `JwtTokenService.cs:24 e :64`, `SqliteMarkerRepository.cs:46`,
  `AutoSkipWebSocketEndpoints.cs:75`.
- **Regra duplicada**: fluxo de scan em `SyncBackgroundService.RunScanAsync` e em
  `PlexEndpoints` (`/api/plex/scan`); `ScanLibrariesUseCase` existe e **não é
  usado** (código morto).
- **Código morto**: `PlexWebSaveEndpoint` nunca registrado em `Program.cs`.
- **I/O de arquivo na Web**: `PlexSaveEndpoint` e `PlexWebSaveEndpoint` escrevem
  `.env` com `File.*`.
- **Endpoints com repositório direto**: `ThemeEndpoints` (IUserThemeRepository),
  `MarkerEndpoints` (IMarkerRepository ×5), `PlexEndpoints` (IMarkerRepository),
  `SeriesEndpoints` (ISeriesRepository) — leitura **e** escrita.
- **Parsing manual**: `MarkerEndpoints` POST (query vs corpo, casing) e
  `AuthEndpoints.ExtractPassword` (regex).

## 3. Alvo

| camada | contém | não contém |
|---|---|---|
| Domain | entidades, portas (interfaces) | I/O, `Process`, SQL, HTTP, DI |
| Application | casos de uso, DTOs, portas de aplicação (path, imagem, config) | implementação de adaptador, HTTP/SQL concreto |
| Infrastructure | repositórios, HTTP, JWT, detecção, background, migração de schema, store de config/job | regra de caso de uso |
| Web | mapeamento HTTP, validação, chamada de caso de uso, DTO de resposta | banco, I/O de arquivo, parsing manual de contrato |

## 4. Requisitos — ADDED

### R1. Nenhuma implementação concreta no Domain
`ProcessInfoService` migra para `Infrastructure/System/`. `IProcessInfoService`
fica em `Domain/Ports`. Domain sem `using System.Diagnostics/Data/Net.Http`.

### R2. Todo adaptador externo em Infrastructure, com responsabilidade única
- `PlexApiClient` migra de `Web/Services` para `Infrastructure/PlexProxy`,
  implementando `IPlexApiClient` (porta em Application).
- **SRP**: separar transporte HTTP (`PlexHttpClient`) de parsing de resposta
  (`PlexResponseParser`), eliminando a god class de 367 LOC.
- **OCP**: parsing tipado (`System.Text.Json`, `PropertyNameCaseInsensitive`) no
  lugar de `TryGetProperty`/`if-else` de casing.
- `ImageProxyService` ganha porta `IImageProxyService` (Application); endpoint
  passa a depender da abstração.
- `ScanJobStore` migra para Infrastructure.

### R3. Endpoints são fachada (leitura e escrita)
Nenhum endpoint injeta `IMarkerRepository`/`ISeriesRepository`/
`ILibraryRepository`/`IUserThemeRepository`. Cada rota chama um caso de uso:
`SaveMarkerUseCase`, `GetMarkersQuery`, `GetSeriesQuery`, `GetThemeQuery`,
`SaveThemeCommand`, `LibraryScanUseCase`, `LoginUseCase`. `SaveMarkerUseCase`
retorna resultado (`Created` / `AlreadyExists`) e o endpoint traduz em 201/409,
unificando o comportamento hoje paralelo.

### R4. Orquestrador de scan único
Extrair `LibraryScanUseCase` (Application) com o fluxo hoje duplicado
(skip-se-já-tem → resolve path → `IMarkerDetector.Detect` → grava). Consumido por
`SyncBackgroundService` e por `/api/plex/scan`. Remove `ScanLibrariesUseCase`
morto (substituído por ele) e a lógica inline duplicada.

### R5. Inicialização de schema fora do construtor, assíncrona
`DatabaseInitializer` em `Infrastructure/Persistence` executa `CREATE TABLE` /
`ALTER TABLE` de **todos os 4** repositórios, de forma assíncrona, chamado no
composition root antes de servir tráfego. Repositórios ficam só com CRUD; nenhum
`Directory.CreateDirectory`/DDL/`.Wait()` no construtor. Migração organizada como
lista versionada (adicionar coluna nova = nova entrada, sem alterar as existentes).

### R6. Sem sync-over-async
Zero `.Wait()` / `.Result` / `.GetAwaiter().GetResult()`. `IPlexAutoSkipService`
vira assíncrono (`GetMarkersForEpisodeAsync`) e `AutoSkipWebSocketEndpoints` faz
`await`. `AuthEndpoints` deixa de ler o corpo manualmente (vira binding tipado).

### R7. Contrato de entrada tipado
Parsing manual (query vs corpo, casing, regex de senha) substituído por binding
tipado (`[FromQuery]`/`[FromBody]` com DTO + validação). Formato inválido → 400.

### R8. Erro tratado, nunca engolido
Zero `catch` nu. Cada captura declara a exceção esperada e registra via
`ILogger`: `SecurityTokenException` (JWT, 2 pontos), `SqliteException` (DDL
idempotente — removida de qualquer forma por R5), `JsonException` (WebSocket).

### R9. I/O de configuração fora da Web
Escrita/leitura de `.env` sai dos endpoints para `IPlexConfigStore` (porta em
Application, impl em Infrastructure). Web chama o caso de uso/serviço.

### R10. Código morto removido
`PlexWebSaveEndpoint` (nunca registrado) removido; `ScanLibrariesUseCase`
substituído por `LibraryScanUseCase` e fiado.

### R11. Enforcement mecânico
Teste de arquitetura falha o build quando Domain referencia Infrastructure,
Application referencia Web, ou Web contém tipo que implementa porta de
persistência/rede.

## 5. Requisitos — MODIFIED

### R12. `MediaPathResolver` sem I/O
`File.Exists` sai do Application: porta `IPathResolver` + implementação em
Infrastructure (verifica existência no filesystem de mídia). A regra de
montagem do caminho permanece testável na Application.

### R13. Repositórios com responsabilidade única
`Sqlite*Repository` só consultam/persistem. Assinaturas das portas mantidas.

## 6. Ordem de execução

1. Mover `ProcessInfoService` → Infrastructure.
2. Criar `DatabaseInitializer` cobrindo os 4 repositórios; remover DDL/`Directory`
   /`.Wait()` dos construtores; registrar assíncrono no composition root.
3. Extrair `IPathResolver` (MediaPathResolver sem I/O).
4. Mover `PlexApiClient` → Infrastructure/PlexProxy; split HTTP/parser; typed
   parsing; porta `IImageProxyService`.
5. Mover `ScanJobStore` → Infrastructure.
6. Extrair `LibraryScanUseCase`; fiar `SyncBackgroundService` e `/api/plex/scan`;
   remover duplicação e `ScanLibrariesUseCase` morto.
7. Criar casos de uso de leitura/escrita faltantes; endpoints (leitura+escrita)
   passam a consumir casos de uso; `SaveMarkerUseCase` retorna resultado (201/409).
8. Extrair `IPlexConfigStore` (`.env`) para Infrastructure.
9. Substituir `catch` nu; eliminar sync-over-async (`IPlexAutoSkipService` async,
   `AuthEndpoints` tipado); typed binding nos endpoints.
10. Remover `PlexWebSaveEndpoint` morto.
11. Adicionar teste de arquitetura.
12. `dotnet build` + 13 e2e + gate.

Cada passo roda `dotnet build`. Nenhum passo altera o contrato HTTP observável.

## 7. Critérios de aceite

| # | critério | verificação |
|---|---|---|
| A1 | Domain sem `System.Diagnostics/Data/Net.Http` e sem classe concreta | grep + teste de arquitetura |
| A2 | Nenhum adaptador concreto (repo/HTTP/store) fora de Infrastructure | grep + teste de arquitetura |
| A3 | Zero endpoint injetando repositório | grep nos endpoints |
| A4 | Zero `catch` nu, zero `.Wait()`/`.Result`/`GetAwaiter` | grep |
| A5 | `dotnet build` limpo | build |
| A6 | 13 e2e Playwright verdes | playwright |
| A7 | Teste de arquitetura falha com violação proposital | rodar variante |
| A8 | `.env` e schema não são tocados pela Web | grep |

## 8. Riscos e rollback

- **Payload `/api/markers`** (aceita query e corpo) — binding tipado aceita ambos;
  e2e cobre. *Mitigação*: DTO com `[FromQuery]` + `[FromBody]` explícitos.
- **Remover `.Wait()`** — inicialização passa a ser `await` no startup, antes do
  tráfego. *Mitigação*: `DatabaseInitializer` roda em `Program.cs` antes de `Run`.
- **Rollback**: `git revert` do commit; tag `pre-clean-arch` criada antes.

## 9. Fora de escopo (justificado)

- `FfmpegMarkerDetector` (235 LOC) — grande mas coeso (detecção); segundo ciclo.
- `ServerCertificateProvider` (Web/Configuration) usa `File.Exists` para carregar
  certificado TLS no composition root — aceitável (bootstrap), não é regra de
  negócio.
- Frontend: tratado no change `refatorar-apresentacao-cinza`; não reabrir.
