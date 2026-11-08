# Tasks — setup-midias-multiplas

## 1. Modelo de configuração (múltiplas raízes)

- [ ] 1.1 `AppSettings`: chave `MEDIA_ROOTS`, default `["/media"]`, `AppSettingsSnapshot.MediaRoots`; `MediaRoots.Normalize` (canonicalização `Path.GetFullPath`, sem barra final, rejeita `/` e relativo, `Directory.Exists`, dedupe, máx. 32).
- [ ] 1.2 `SqliteAppSettingsStore`: ler/gravar JSON (`media_roots` verbatim, sem normalizar); fallback de leitura para `MEDIA_ROOT` legado.
- [ ] 1.3 `AppSettingsSeeder`: seed por env `MEDIA_ROOTS` (JSON ou `\n`/`,`/`:`) ou `MEDIA_ROOT`; migração "chave ausente" independente do contador da tabela.
- [ ] 1.4 `UpdateAppSettingsUseCase` + `SettingsEndpoints`: `media_roots` (`[JsonPropertyName]`), `null` preserva, `[]` → 400, erros prefixados.
- [ ] 1.5 Validador único de `plex_url` reutilizado por `UpdateAppSettingsUseCase`, setup e `PlexIdentityEndpoint` (sem `StartsWith`).

## 2. Resolução de caminho multi-raiz

- [ ] 2.1 `MediaPathResolver.BuildCandidates(IReadOnlyList<string>, string)` ordenado por especificidade (sufixo longo → curto, raízes na ordem); `Fallback` com primeira raiz.
- [ ] 2.2 `IPathResolver`/`PathResolver` com assinatura de lista.
- [ ] 2.3 Atualizar chamadas em `PlexEndpoints` (scan e listagem), `SeriesEndpoints`, `SyncBackgroundService`, `LibraryScanUseCase`.
- [ ] 2.4 Testes: resolução em segunda raiz, colisão raiz0×raiz1 (caminho exato vence), fallback, lista vazia.

## 3. Setup inicial

- [ ] 3.1 `CompleteSetupUseCase`: ordenar token → senha → `plex_url` → raízes → `GetIdentityAsync` (timeout 10s) → `SaveAsync` → `CreateAsync` → invalidação; retorno com `errors: string[]`.
- [ ] 3.2 `AuthEndpoints` `/api/auth/setup`: payload `{ token, newPassword, plexUrl, plexToken, mediaRoots }`, erros `400/401/409/204`.
- [ ] 3.3 DI: `CompleteSetupUseCase` como `Transient`.
- [ ] 3.4 `SetupView.vue`: seções credencial / Plex / mídia + lista de erros.

## 4. Navegador de diretórios

- [ ] 4.1 `FileSystemEndpoints` `GET /api/filesystem/directories?path=`: só diretórios, ordenado `OrdinalIgnoreCase`, ocultos filtrados, cap 2000, `parent: null` na raiz, `400/403/404`.
- [ ] 4.2 `JwtAuthMiddleware`: liberar `/api/filesystem/directories` apenas quando `!IsConfiguredAsync()`; com setup concluído exige sessão.
- [ ] 4.3 `components/DirectoryPicker.vue` (navegação, seleção, Esc/backdrop, tema, testids).
- [ ] 4.4 `components/MediaRootsField.vue` (add/remover/picker) usado em `SetupView` e `SettingsView`.

## 5. Ajustes

- [ ] 5.1 `SettingsView.vue`: `media_roots` via `MediaRootsField`.
- [ ] 5.2 `api.js`: `extractErrors(res) -> string[]` compartilhado por setup/settings.

## 6. Fix transporte Plex

- [ ] 6.1 `PlexApiClient`: helper `BuildUrl(plexUrl, path)` + `IAppSettingsStore`; aplicado em `GetLibraries`, `GetEpisodes`, `GetSeries`, `GetSeriesEpisodes`, `GetIdentity`.
- [ ] 6.2 `GetIdentityAsync` envia `Accept: application/xml`.
- [ ] 6.3 `PlexRequestHandler`: só `X-Plex-Token`/`Accept`; requisição sem URL absoluta lança `InvalidOperationException`.
- [ ] 6.4 Verificar consumidores de `PlexUrl` (`ImageProxyService`, `PlexIdentityEndpoint`).

## 7. Contratos e testes existentes

- [ ] 7.1 Atualizar `tests/PlexAutoplay.ArchitectureTests` (`SettingsPersistenceTests`, `PasswordAndAuthTests`) para as novas assinaturas.
- [ ] 7.2 Testes novos: `MediaRoots.Normalize`, `PlexApiClient.BuildUrl` com prefixo `/plex`, `UpdateAppSettingsUseCase`, `FileSystemEndpoints` (`WebApplicationFactory`).
- [ ] 7.3 `tests/e2e/global-setup.js`: enviar `plexUrl`, `plexToken`, `mediaRoots` válidos.
- [ ] 7.4 `tests/e2e/mock-plex/server.cjs`: adicionar `GET /identity` (XML).
- [ ] 7.5 Docs: `.env.example` (`MEDIA_ROOTS` formato), `docker-compose.yml` (exemplo de múltiplos mounts), README (config no SQLite, não só env).

## 8. Verificação

- [ ] 8.1 `dotnet build PlexAutoplay.slnx -c Release`.
- [ ] 8.2 `dotnet test PlexAutoplay.slnx -c Release`.
- [ ] 8.3 `npm run build` em `vue/plex-frontend` + copiar `dist` para `src/PlexAutoplay.Web/wwwroot`.
- [ ] 8.4 `openspec validate setup-midias-multiplas --strict`.
- [ ] 8.5 QA UI Playwright: tema claro/escuro, navegação de menus, modal do seletor, add/remover diretório.
