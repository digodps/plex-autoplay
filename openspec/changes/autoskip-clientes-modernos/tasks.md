# Tasks

## 1. Detecção de cliente moderno

- [ ] 1.1 Criar `src/PlexAutoplay.Infrastructure/AutoSkip/ClientSeekStrategy.cs` com enum `ClientSeekCategory` (LegacyCompanion, ModernWeb, ModernDesktop, SmartTV, Unknown) e record `ClientSeekStrategy(bool SupportsDirectConnection, int DirectPort, bool SupportsPmsProxy, bool ExpectsNonXmlResponse)`.
- [ ] 1.2 Criar `src/PlexAutoplay.Infrastructure/AutoSkip/PlexModernClientDetector.cs` com `Detect(string product, string platform, string version)` retornando `ClientSeekStrategy`.
- [ ] 1.3 Cobrir regras: Web (>= 4.83.2 → ModernWeb), Desktop (>= 1.46.1 → ModernDesktop), SmartTV por substring (`Samsung`, `LG`, `Vizio`, `Hisense`, `Tizen`, `webOS`), LegacyCompanion por product exato (Android TV/Mobile, Apple TV, Roku) com porta 32500/8324, Unknown fallback.

## 2. Refatorar PlexAutoSkipBackgroundService

- [ ] 2.1 Injetar `PlexModernClientDetector` (ou instanciar estático) e substituir `GetClientPort` por `GetClientStrategy`.
- [ ] 2.2 Expandir `PlexSessionInfo` com `Device` e `Model` (extraídos de `<Player>`) se necessário para detecção.
- [ ] 2.3 Reescrever `SeekToEndOfMarkerAsync`: tentar direto só se `SupportsDirectConnection && DirectPort > 0 && ClientIp` não vazio; sempre tentar PMS proxy se `SupportsPmsProxy`; ambos com `commandID`.
- [ ] 2.4 Remover `IsBrokenClient` e `VersionCompare`.
- [ ] 2.5 Adicionar verificador `IsSeekSuccess(HttpResponseMessage)` que trata 2xx + body vazio/`OK`/XML com `code="200"`.

## 3. Testes

- [ ] 3.1 Adicionar `tests/PlexAutoplay.ArchitectureTests/AutoSkipClientDetectionTests.cs` (xunit) cobrindo todos os scenarios do spec: ModernWeb, ModernDesktop, SmartTV, LegacyCompanion, Unknown.
- [ ] 3.2 Testar `IsSeekSuccess` para `OK`, XML parcial, 204 vazio, 404/500.

## 4. Validação

- [ ] 4.1 `dotnet build PlexAutoplay.slnx` passa.
- [ ] 4.2 `dotnet test tests/PlexAutoplay.ArchitectureTests` passa.
- [ ] 4.3 `openspec validate autoskip-clientes-modernos --strict` passa.
- [ ] 4.4 Revisar log de decisão: sem `IsBrokenClient`, sem lista hardcoded de versão quebrada.