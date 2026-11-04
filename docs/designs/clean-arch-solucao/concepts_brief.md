# Concepts Brief — Solução plex-autoplay conforme Clean Architecture / Clean Code / SOLID

## Demanda

Refatorar a solução `plex-autoplay` (backend .NET 4 projetos + frontend Vue) para
garantir aderência a **Clean Architecture**, **Clean Code** e **SOLID**, sem
alterar o contrato HTTP (`/api/*`) nem o comportamento funcional.

## Estado atual (medido, não estimado)

| projeto | arquivos .cs | LOC |
|---|---|---|
| PlexAutoplay.Domain | 14 | 203 |
| PlexAutoplay.Application | 5 | 207 |
| PlexAutoplay.Infrastructure | 12 | 1172 |
| PlexAutoplay.Web | 22 | 1983 |

Grafo de dependência de projeto **já correto**: Domain (sem refs) ← Application ←
Infrastructure ← Web. A violação não é estrutural de projeto: é **interna aos
arquivos**.

### Violações encontradas (com evidência)

**Clean Architecture (camada errada / regra de dependência furada)**
1. `Web/Services/PlexApiClient.cs` (194) + `PlexApiClient.Series.cs` (173) — adaptador HTTP (implementa a porta `IPlexApiClient` de Application) morando na camada de apresentação. Deve estar em Infrastructure.
2. `Domain/ProcessInfoService.cs` — classe **concreta** no Domain usando `System.Diagnostics.Process`. Vazamento de infraestrutura para o domínio (viola DIP).
3. Endpoints chamam repositório direto e ignoram os casos de uso existentes: `MarkerEndpoints` usa `IMarkerRepository` direto; `SaveMarkerUseCase` existe mas não é usado por ele. Regra de negócio e parsing de transporte na camada Web (fina).
4. Endpoints gordos: `MarkerEndpoints` (151), `PlexSaveEndpoint` (155), `PlexEndpoints` (144), `MonitoringEndpoints` (137), `PlexWebSaveEndpoint` (96).

**SOLID**
5. SRP — `SqliteMarkerRepository` executa `Directory.CreateDirectory` + DDL de schema (`CREATE TABLE` / `ALTER TABLE`) **no construtor**, com efeito colateral e I/O.
6. SRP — `PlexApiClient` é god class de 367 LOC dividido em partials.
7. OCP — parsing manual de payload com `if/else` para múltiplas variações de casing (`RatingKey`/`rating_key`/`ratingKey`) em vez de binding tipado de contrato.

**Clean Code**
8. `catch` nu engolindo exceção (sem tipo), 3 ocorrências: `AutoSkipWebSocketEndpoints.cs:75` (JSON inválido ignorado), `JwtTokenService.cs:64`, `SqliteMarkerRepository.cs:46`.
9. Sync-over-async: `conn.ExecuteAsync(...).Wait()` no `EnsureTableCreated` do `SqliteMarkerRepository` — bloqueia thread, risco de deadlock.

### O que está correto e deve ser preservado
- Direção de dependência entre projetos.
- Portas (`Domain/Ports/*`) e casos de uso com injeção por construtor (`SaveMarkerUseCase`, `ScanLibrariesUseCase`).
- Ausência de `Console.WriteLine` nas bibliotecas.
- Frontend: camada de apresentação já refatorada (change `refatorar-apresentacao-cinza`), com fronteiras verificadas por `eslint-plugin-boundaries`.

## Decisões de solução (proposta)

1. Mover todo adaptador concreto para Infrastructure (`PlexApiClient`, `ProcessInfoService`), mantendo portas no Domain/Application.
2. Endpoints viram casca HTTP: parse → chama caso de uso → devolve resultado. Regra vai para `Application/UseCases`.
3. Inicialização de schema SQLite sai do construtor para um passo explícito de migração executado no composition root.
4. Substituir `catch` nu por tipo específico + tratamento/registro explícito.
5. Eliminar sync-over-async (assíncrono de ponta a ponta).
6. Binding tipado de DTO no lugar do parsing manual por casing.

## Escopo / não escopo

- **Escopo**: camadas do backend .NET (Domain/Application/Infrastructure/Web) e o gate de verificação.
- **Não escopo**: mudança de contrato HTTP, comportamento funcional, banco (esquema preservado), frontend (já tratado em change separado, salvo ajuste de consumo se necessário).
- **Restrição**: refatoração estrutural sem regressão funcional — 13 testes e2e Playwright existentes devem continuar verdes.

## Gate de sucesso

- Regra de dependência verificável (analyzer/architecture test) sem violação.
- `dotnet build` sem warning novo; testes e2e verdes; casos de uso cobertos por teste.
- Zero `catch` nu, zero sync-over-async, zero adaptador concreto fora de Infrastructure.
