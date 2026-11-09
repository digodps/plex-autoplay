# Tasks

## 1. Detecção de cliente moderno

- [x] 1.1 Criar `src/PlexAutoplay.Infrastructure/AutoSkip/ClientSeekStrategy.cs` com enum `ClientSeekCategory` (LegacyCompanion, ModernWeb, ModernDesktop, SmartTV, Unknown) e record `ClientSeekStrategy(bool SupportsDirectConnection, int DirectPort, bool SupportsPmsProxy, bool ExpectsNonXmlResponse)`.
- [x] 1.2 Criar `src/PlexAutoplay.Infrastructure/AutoSkip/PlexModernClientDetector.cs` com `Detect(string product, string platform, string version)` retornando `ClientSeekStrategy`.
- [x] 1.3 Cobrir regras: Web (>= 4.83.2 → ModernWeb), Desktop (>= 1.46.1 → ModernDesktop), SmartTV por substring (`Samsung`, `LG`, `Vizio`, `Hisense`, `Tizen`, `webOS`), LegacyCompanion por product exato (Android TV/Mobile, Apple TV, Roku) com porta 32500/8324, Unknown fallback.

## 2. Refatorar PlexAutoSkipBackgroundService

- [x] 2.1 Injetar `IClientSeekStrategyProvider` e substituir `GetClientPort` por detecção dinâmica.
- [x] 2.2 `PlexSessionInfo` extrai `Product`/`Platform`/`Version` de `<Player>` (sem novos campos Device/Model).
- [x] 2.3 Reescrever `SeekToEndOfMarkerAsync`: direto só se `SupportsDirectConnection && DirectPort > 0 && ClientIp`; PMS proxy como padrão/fallback; ambos com `commandID`.
- [x] 2.4 Remover `IsBrokenClient` e `VersionCompare`.
- [x] 2.5 Adicionar `IsSeekSuccessAsync` que trata 2xx + body vazio/`OK`/XML com `code="200"`.

## 3. Testes

- [x] 3.1 Adicionar `tests/PlexAutoplay.ArchitectureTests/PlexModernClientDetectorTests.cs` cobrindo ModernWeb, ModernDesktop, SmartTV, LegacyCompanion, Unknown.
- [x] 3.2 Testar `IsSeekSuccessAsync` para `OK`, XML parcial, corpo vazio, 404.

## 4. Validação

- [x] 4.1 `dotnet build PlexAutoplay.slnx` passa.
- [x] 4.2 `dotnet test tests/PlexAutoplay.ArchitectureTests` passa (34 testes).
- [x] 4.3 `openspec validate autoskip-clientes-modernos --strict` passa.
- [x] 4.4 Revisar log de decisão: sem `IsBrokenClient`, sem lista hardcoded de versão quebrada.
- [x] 4.5 Docker acceptance: `docker build --no-cache -t plex-autoplay:autoskip-clientes-modernos .` OK; container iniciado, setup e login OK.