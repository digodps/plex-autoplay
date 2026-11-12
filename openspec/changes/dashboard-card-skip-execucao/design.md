# Design — Dashboard Card: Sessões Ativas e Histórico de Skip

## Objetivo
Expor no Dashboard (card clicável) e na página `/sessions` quais sessões estão ativas no Plex, quando o skip será executado (próximo marcador + tempo restante) e quando o skip já foi executado (histórico recente). Tudo para validar se o skip ocorreu.

## Arquitetura (espelho do Monitor)
Todo o estado de sessões/skips é mantido em memória por um singleton `ISkipMonitor`, atualizado a cada ciclo (2s) do `PlexAutoSkipBackgroundService`. O endpoint `GET /api/sessions/skip-status` apenas lê esse snapshot — nenhuma consulta extra ao Plex no momento da requisição.

```
┌─────────────────────────────┐      a cada 2s         ┌──────────────────┐
│ PlexAutoSkipBackgroundService│ ─────────────────────▶│  ISkipMonitor    │
│ - lê /status/sessions       │   (snapshot + skips)   │  - LastSessions  │
│ - registra skips executados │ ◀───────────────────── │  - RecentSkips   │
└─────────────────────────────┘                        └──────────────────┘
                                                          │
                                                          ▼
                                               ┌────────────────────┐
                                               │ GET /api/sessions/ │
                                               │ /skip-status       │
                                               │ (JSON)             │
                                               └────────────────────┘
                                                          │
                                                          ▼
                                                Vue /sessions + card Dashboard
```

## 1. Backend (.NET)

### 1.1 `SkipHistoryEntry` (entidade)
**Arquivo:** `src/PlexAutoplay.Domain/Entities/SkipHistoryEntry.cs`
```csharp
public sealed record SkipHistoryEntry(
    DateTime Timestamp,
    string MachineIdentifier,
    string Product,
    string Platform,
    int RatingKey,
    string MarkerKind,
    long SkippedFromMs,
    long SkippedToMs);
```

### 1.2 `ISkipMonitor` (port)
**Arquivo:** `src/PlexAutoplay.Domain/Ports/ISkipMonitor.cs`
```csharp
public interface ISkipMonitor
{
    /// <summary>Último snapshot de sessões ativas (posição + próximo marcador).</summary>
    IReadOnlyList<SessionStatus> Sessions { get; }
    /// <summary>Skips recentes (últimos 50), mais recente primeiro.</summary>
    IReadOnlyList<SkipHistoryEntry> RecentSkips { get; }
    void UpdateSessions(IReadOnlyList<SessionStatus> sessions);
    void RecordSkip(SkipHistoryEntry entry);
}
```

### 1.3 `SessionStatus` + `MarkerTarget` (DTOs de status de sessão)
**Arquivo:** `src/PlexAutoplay.Domain/Entities/SessionStatus.cs`
```csharp
public sealed record SessionStatus(
    string MachineIdentifier,
    string Product,
    string Platform,
    int RatingKey,
    long PositionMs,
    long DurationMs,
    string State,
    MarkerTarget? NextMarker);

public sealed record MarkerTarget(
    string Kind,
    long StartMs,
    long EndMs,
    long MsUntilSkip);
```

### 1.4 `SkipMonitor` (implementação singleton)
**Arquivo:** `src/PlexAutoplay.Infrastructure/SkipMonitor.cs`
- Thread-safe (`ConcurrentQueue`/lock).
- `RecentSkips` anel de 50 entradas.
- `sessions` substituído inteiro a cada atualização.

### 1.5 Registro DI
**Arquivo:** `src/PlexAutoplay.Web/Configuration/ServiceCollectionExtensions.cs`
Adicionar `services.AddSingleton<ISkipMonitor, SkipMonitor>();`

### 1.6 `PlexAutoSkipBackgroundService` — hook
Injetar `ISkipMonitor`.
- Ao fim de cada `PollAndSkipAsync` (mesmo sem sessão), `UpdateSessions` com snapshot vazio ou com a sessão lida (posição, próximos marcadores, msUntilSkip, estado).
- Quando `SeekToEndOfMarkerAsync` retorna true, `RecordSkip(new SkipHistoryEntry(...))`.
  - `SkippedFromMs = session.PositionMs`, `SkippedToMs = marker.EndMs`.

### 1.7 Endpoint `SessionsEndpoints`
**Arquivo:** `src/PlexAutoplay.Web/Endpoints/SessionsEndpoints.cs`
```csharp
app.MapGet("/api/sessions/skip-status", ([FromServices] ISkipMonitor monitor) =>
    Results.Ok(new
    {
        activeSessions = monitor.Sessions.Select(s => new
        {
            s.MachineIdentifier, s.Product, s.Platform, s.RatingKey,
            positionMs = s.PositionMs, durationMs = s.DurationMs, state = s.State,
            nextMarker = s.NextMarker is null ? null : new
            {
                s.NextMarker.Kind, s.NextMarker.StartMs, s.NextMarker.EndMs,
                s.NextMarker.MsUntilSkip
            }
        }),
        recentSkips = monitor.RecentSkips.Select(s => new
        {
            s.Timestamp, s.MachineIdentifier, s.Product, s.Platform,
            s.RatingKey, markerKind = s.MarkerKind,
            skippedFromMs = s.SkippedFromMs, skippedToMs = s.SkippedToMs
        })
    }));
```
Registrar `app.MapSessionsEndpoints();` em `Program.cs`. Protegido pelo `JwtAuthMiddleware` (rota `/api/*`).

> **ponytail:** histórico em memória (perdido no restart). Ceiling: sem persistência entre restarts do container. Upgrade: tabela `SkipHistory` SQLite + migration v13 + `ISkipHistoryStore`.

## 2. Frontend (Vue)

### 2.1 Card no Dashboard
**`vue/plex-frontend/src/composables/useDashboardCards.js`**: adicionar `skipSessionsCard` com:
- `label`: "Sessões & Skips"
- `value`: contagem de sessões ativas (ou "—")
- `breakdown`: "Último skip" (timestamp pt-BR ou "Nenhum")
- `href`: "/sessions"

**`vue/plex-frontend/src/views/Dashboard/DashboardView.vue`**: importar `skipSessionsCard` e render `CardDataStats`.

### 2.2 Página `/sessions`
**Arquivo:** `vue/plex-frontend/src/views/Sessions/SessionsView.vue`
- Tabela de sessões ativas: cliente (product/platform), posição (barra de progresso + ms/duração), próximo marcador (tipo + ms até skip), estado.
- Tabela de skips recentes: data/hora, cliente, tipo marcador, de → para ms.
- Polling a cada 5s (`setInterval`) enquanto aba visível.
- Respeita tema (`dataset.theme` / classe `.dark`).
- Volta para dashboard se não houver dados ainda (estado "carregando"/"sem sessões").

### 2.3 Rota
**`vue/plex-frontend/src/router/index.js`**: adicionar `{ path: "/sessions", name: "sessions", component: () => import("../views/Sessions/SessionsView.vue"), meta: { title: "Sessões e Skips" } }`.

## 3. Tema / Acessibilidade
- Cores das tabelas, badges e barras usam variáveis CSS (`var(--accent)`, `var(--warning)`, `var(--danger)`) e reagem a `dark:` via Tailwind (já configurado no projeto).
- Foco visível e contraste mantidos (herdado do design system existente).

## 4. Testes
- Unit (.NET): `SkipMonitor` mantém últimos 50 skips, `Sessions` snapshot atualizado, endpoint retorna shape esperado (via WebApplicationFactory ou teste de unidade do monitor).
- E2E (Playwright): abrir `/sessions`, verificar tabela renderizada e auto-refresh; alternar tema e verificar cores; clicar card no Dashboard e navegar para `/sessions`.
