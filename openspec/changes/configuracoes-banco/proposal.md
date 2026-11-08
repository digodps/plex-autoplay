# Change Proposal — configuracoes-banco

## Why

As configurações operacionais vivem no `.env` da raiz e são lidas por
`Environment.GetEnvironmentVariable` no start. Alterar URL/token do Plex,
`MEDIA_ROOT` ou intervalos exige editar arquivo e reiniciar o container; a
página de Ajustes só grava parte dos valores no `localStorage` do navegador.
O operador precisa configurar tudo pela interface, com persistência no SQLite
e efeito imediato no processamento.

## What Changes

- **BREAKING**: remove `PlexConfigStore` e a escrita/leitura do `.env` em
  runtime; configuração operacional passa a vir do SQLite.
- Nova tabela SQLite `AppSetting` (`Key`, `Value`, `UpdatedAt`).
- Porta `IAppSettingsStore` e repositório `SqliteAppSettingsStore`.
- Seed único no primeiro start: copia valores do ambiente ou usa padrões
  apenas quando `AppSetting` está vazia; nunca re-semeia depois.
- `GET/PUT /api/settings` (JWT obrigatório) lê e grava:
  `PLEX_URL`, `PLEX_TOKEN`, `MEDIA_ROOT`, `SYNC_INTERVAL_MINUTES`,
  `SYNC_CATALOG_INTERVAL_MINUTES`.
- `GET /api/settings` não devolve o token; devolve `plex_token_set`.
  `PUT` só altera o token quando o campo é enviado explicitamente.
- Removidos `POST /api/plex/save` e `GET/POST /config`; a página usa só
  `/api/settings`.
- Consumidores resolvem valores do banco a cada uso:
  `PlexRequestHandler`, `ImageProxyService`, `PlexAutoSkipBackgroundService`,
  `SyncBackgroundService`, `CatalogSyncBackgroundService`, endpoints de
  bibliotecas/séries.
- Validação centralizada em `UpdateAppSettingsUseCase`, antes de qualquer
  escrita atômica.
- Página `/settings` carrega e salva via API; remove `localStorage`.
- Variáveis exclusivas de bootstrap (`JWT_SECRET`, portas, certificado,
  `ASPNETCORE_ENVIRONMENT`, conexão SQLite) permanecem no ambiente.

## Capabilities

- **New Capabilities**: `settings-management`
- **Modified Capabilities**: `config-env`, `backend-architecture`

## Impact

- Backend: `src/PlexAutoplay.Application`, `src/PlexAutoplay.Infrastructure`,
  `src/PlexAutoplay.Web`; removidos `IPlexConfigStore`/`PlexConfigStore`,
  `PlexSaveEndpoint` e `ConfigEndpoints`.
- Banco: migração 7 com tabela `AppSetting`; `journal_mode=WAL` e
  `busy_timeout`.
- Frontend: `vue/plex-frontend/src/views/Pages/SettingsView.vue`.
- Testes: `ArchitectureTests` e teste unitário do store/seed.
- Sem novas dependências.
