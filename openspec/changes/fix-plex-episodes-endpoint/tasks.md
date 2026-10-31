# Tasks: fix-plex-episodes-endpoint

## 1. Backend

- [ ] 1.1 Alterar `GetEpisodesAsync` em `src/PlexAutoplay.Web/Services/PlexApiClient.cs` para consultar `/library/sections/{sectionKey}/all?type=4`, mantendo telemetria e filtro de `type == "episode"`.
- [ ] 1.2 Adicionar teste unitário em `tests/unit/PlexApiClientTests.cs` que valide path/query e o parsing de episódios da seção.
- [ ] 1.3 Atualizar `tests/e2e/mock-plex/server.js` para responder episódios em `/library/sections/{id}/all?type=4`.

## 2. Validação

- [ ] 2.1 Executar `dotnet test tests/unit/PlexAutoplay.UnitTests.csproj` com 100% de sucesso.
- [ ] 2.2 Executar `dotnet format PlexAutoplay.slnx --verify-no-changes`.
- [ ] 2.3 Executar `./scripts/acceptance-test.sh` e exigir `RESULTADO: APROVADO` (ou registrar falhas ambientais restantes).

## 3. Entrega

- [ ] 3.1 Executar `python3 scripts/code-review-skill.py --dry-run`.
- [ ] 3.2 Criar commits semânticos com `scripts/commit-skill.py --change fix-plex-episodes-endpoint`.
- [ ] 3.3 Push da branch e abertura do Pull Request.
