# Design — configuracoes-banco

## Princípio

Banco é a fonte de verdade para configuração operacional. Ambiente serve
apenas para seed único e bootstrap imutável (portas, certificado, JWT,
conexão SQLite, ambiente ASP.NET).

## Persistência

Migração 7 em `DatabaseInitializer`:

```sql
CREATE TABLE IF NOT EXISTS AppSetting (
    Key TEXT NOT NULL PRIMARY KEY,
    Value TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
)
```

Chaves operacionais em `AppSettingKeys`/`AppSettingDefaults` (Application):

| Chave | Padrão | Uso |
|---|---|---|
| `PLEX_URL` | `http://localhost:32400` | API e proxy de imagens |
| `PLEX_TOKEN` | `` | API e proxy de imagens |
| `MEDIA_ROOT` | `/media` | resolução de caminho local |
| `SYNC_INTERVAL_MINUTES` | `5` | `SyncBackgroundService` |
| `SYNC_CATALOG_INTERVAL_MINUTES` | `30` | `CatalogSyncBackgroundService` |

Seed em `AppSettingsSeeder` roda após a migração e somente quando
`SELECT COUNT(*) FROM AppSetting = 0`. Copia `Environment` de cada chave
existente; senão grava o padrão. Nunca re-semeia, então apagar/alterar valor
não é desfeito no próximo start.

`DatabaseInitializer` aplica `PRAGMA journal_mode=WAL` e
`PRAGMA busy_timeout=5000` para reduzir `SQLITE_BUSY` entre leituras de
settings e escritas de markers.

## Porta e repositório

`IAppSettingsStore` (Application) mínimo:

```csharp
Task<AppSettingsSnapshot> GetAsync();
Task SaveAsync(AppSettingsSnapshot settings);
```

`AppSettingsSnapshot` expõe os cinco valores tipados e aplica defaults.
`SqliteAppSettingsStore` (Infrastructure, Dapper) lê em uma query e grava
UPSERT em transação:

```sql
INSERT INTO AppSetting (Key, Value) VALUES (@Key, @Value)
ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value, UpdatedAt = datetime('now')
```

Sem cache próprio: leitura por PK é barata e garante efeito imediato.

## Validação e caso de uso

`UpdateAppSettingsUseCase` (Application) recebe valores parciais, valida tudo
antes de escrever e retorna erros por campo:

- `plex_url`: `Uri.TryCreate` absoluto com esquema `http/https`.
- intervalos: `1..1440`.
- `media_root`: `Path.IsPathRooted` e diretório existente.
- `plex_token`: ausente = preserva; string vazia = limpa; presente = grava.

Endpoints só traduzem DTO → use case. `POST /api/plex/save` e `/config` são
removidos; a UI usa apenas `/api/settings`.

## Cliente Plex dinâmico

`PlexRequestHandler : DelegatingHandler` resolve URL/token do banco em cada
`SendAsync`:

- `RequestUri` relativa → combina com `PLEX_URL` normalizada (`TrimEnd('/')`).
- `RequestUri` absoluta (checagem de identidade) → preserva.
- Adiciona `X-Plex-Token` por requisição quando ausente, sem tocar em
  `DefaultRequestHeaders`.

`PlexApiClient` usa URIs relativas. `IPlexApiClient` é registrado como
transiente via `IHttpClientFactory`, sem capturar um `HttpClient` singleton.
`GetIdentityAsync` envia URL absoluta e token em `HttpRequestMessage`
próprio, sem mutar estado compartilhado.

`ImageProxyService` recebe `IAppSettingsStore`, resolve URL/token por chamada
e usa chave de cache `image:{plexUrl}:{imagePath}` para não servir bytes do
servidor anterior.

## Background services

- `SyncBackgroundService`: lê snapshot a cada ciclo e usa intervalo e
  `MediaRoot` atuais.
- `CatalogSyncBackgroundService`: lê o intervalo a cada ciclo.
- `PlexAutoSkipBackgroundService`: resolve URL/token a cada poll.

## Endpoints e segurança

- `GET /api/settings` → snapshot sem token, com `plex_token_set`; JWT.
- `PUT /api/settings` → `UpdateAppSettingsUseCase`; JWT.
- Middleware remove `/api/plex/save` e `/api/plex/identity` da allowlist
  anônima; apenas login/setup, assets e proxy de imagens continuam públicos.

O token do Plex nunca aparece em resposta, log ou `/config` (removido). Logs
registram apenas o comprimento.

## Frontend

`SettingsView.vue`:

- `onMounted` → `GET /api/settings`.
- Campos: URL/token do Plex, `media_root`, intervalo de sync e de catálogo.
- Token em `type="password"`; vazio significa "manter token atual". Aviso
  textual quando `plex_token_set` é verdadeiro.
- Salvar → `PUT /api/settings`; usa `extractErrorMessage`.
- Remover uso de `localStorage`; limites `1..1440`.

Tema/menu permanecem inalterados.

## Testes

- Unitário: store grava/lê, seed não sobrescreve, validação rejeita sem
  escrita.
- Arquitetura: Web não implementa `IAppSettingsStore`.
- QA Playwright: página carrega valores do banco, salva, recarrega e mantém;
  tema e menus.
