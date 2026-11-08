# Tasks — setup-midias-multiplas

## 1. Modelo de configuração (múltiplas raízes)

- [x] 1.1 `AppSettings`: chave `MEDIA_ROOTS`, default `["/media"]`, `AppSettingsSnapshot.MediaRoots`; `MediaRoots.Normalize` (canonicalização `Path.GetFullPath`, sem barra final, rejeita `/` e relativo, `Directory.Exists`, dedupe, máx. 32).
- [x] 1.2 `SqliteAppSettingsStore`: ler/gravar JSON (`media_roots` verbatim, sem normalizar); fallback de leitura para `MEDIA_ROOT` legado.
- [x] 1.3 `AppSettingsSeeder`: seed por env `MEDIA_ROOTS` (JSON ou `\n`/`,`/`:`) ou `MEDIA_ROOT`; migração "chave ausente" independente do contador da tabela.
- [x] 1.4 `UpdateAppSettingsUseCase` + `SettingsEndpoints`: `media_roots` (`[JsonPropertyName]`), `null` preserva, `[]` → 400, erros prefixados.
- [x] 1.5 Validador único de `plex_url` reutilizado por `UpdateAppSettingsUseCase`, setup e `PlexIdentityEndpoint` (sem `StartsWith`).

## 2. Resolução de caminho multi-raiz

- [x] 2.1 `MediaPathResolver.BuildCandidates(IReadOnlyList<string>, string)` ordenado por especificidade (sufixo longo → curto, raízes na ordem); `Fallback` com primeira raiz.
- [x] 2.2 `IPathResolver`/`PathResolver` com assinatura de lista.
- [x] 2.3 Atualizar chamadas em `PlexEndpoints` (scan e listagem), `SeriesEndpoints`, `SyncBackgroundService`, `LibraryScanUseCase`.
- [x] 2.4 Testes: resolução em segunda raiz, colisão raiz0×raiz1 (caminho exato vence), fallback, lista vazia.

## 3. Setup inicial

- [x] 3.1 `CompleteSetupUseCase`: ordenar token → senha → `plex_url` → raízes → `GetIdentityAsync` (timeout 10s) → `SaveAsync` → `CreateAsync` → invalidação; retorno com `errors: string[]`.
- [x] 3.2 `AuthEndpoints` `/api/auth/setup`: payload `{ token, newPassword, plexUrl, plexToken, mediaRoots }`, erros `400/401/409/204`.
- [x] 3.3 DI: `CompleteSetupUseCase` como `Transient`.
- [x] 3.4 `SetupView.vue`: wizard de 3 passos (senha → Plex → mídia), "Testar conexão" via `/api/plex/identity`, lista de erros e auto-login no dashboard após `204`.

## 4. Navegador de diretórios

- [x] 4.1 `FileSystemEndpoints` `GET /api/filesystem/directories?path=`: só diretórios, ordenado `OrdinalIgnoreCase`, ocultos filtrados, cap 2000, `parent: null` na raiz, `400/403/404`.
- [x] 4.2 `JwtAuthMiddleware`: liberar `/api/filesystem/directories` apenas quando `!IsConfiguredAsync()`; com setup concluído exige sessão.
- [x] 4.3 `components/DirectoryPicker.vue` (navegação, seleção, Esc/backdrop, tema, testids).
- [x] 4.4 `components/MediaRootsField.vue` (add/remover/picker) usado em `SetupView` e `SettingsView`.

## 5. Ajustes

- [x] 5.1 `SettingsView.vue`: `media_roots` via `MediaRootsField`.
- [x] 5.2 `api.js`: `extractErrors(res) -> string[]` compartilhado por setup/settings.

## 6. Fix transporte Plex

- [x] 6.1 `PlexApiClient`: helper `BuildUrl(plexUrl, path)` + `IAppSettingsStore`; aplicado em `GetLibraries`, `GetEpisodes`, `GetSeries`, `GetSeriesEpisodes`, `GetIdentity`.
- [x] 6.2 `GetIdentityAsync` envia `Accept: application/xml`.
- [x] 6.3 `PlexRequestHandler`: só `X-Plex-Token`/`Accept`; requisição sem URL absoluta lança `InvalidOperationException`.
- [x] 6.4 Verificar consumidores de `PlexUrl` (`ImageProxyService`, `PlexIdentityEndpoint`).

## 7. Contratos e testes existentes

- [x] 7.1 Atualizar `tests/PlexAutoplay.ArchitectureTests` (`SettingsPersistenceTests`, `PasswordAndAuthTests`) para as novas assinaturas.
- [x] 7.2 Testes novos: `MediaRoots.Normalize`, `PlexApiClient.BuildUrl` com prefixo `/plex`, `UpdateAppSettingsUseCase`; `FileSystemEndpoints` verificado via Playwright (picker) e curl.
- [x] 7.3 `tests/e2e/global-setup.js`: enviar `plexUrl`, `plexToken`, `mediaRoots` válidos.
- [x] 7.4 `tests/e2e/mock-plex/server.cjs`: adicionar `GET /identity` (XML).
- [x] 7.5 Docs: `.env.example` (`MEDIA_ROOTS` formato), `docker-compose.yml` (exemplo de múltiplos mounts), README (config no SQLite, não só env).

## 8. Verificação

- [x] 8.1 `dotnet build PlexAutoplay.slnx -c Release`.
- [x] 8.2 `dotnet test PlexAutoplay.slnx -c Release`.
- [x] 8.3 `npm run build` em `vue/plex-frontend` + copiar `dist` para `src/PlexAutoplay.Web/wwwroot`.
- [x] 8.4 `openspec validate setup-midias-multiplas --strict`.
- [x] 8.5 QA UI Playwright: tema claro/escuro, navegação de menus, modal do seletor, add/remover diretório.
