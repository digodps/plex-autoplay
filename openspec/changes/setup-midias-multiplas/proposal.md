# Change Proposal — setup-midias-multiplas

## Why

O primeiro acesso (`/setup`) só cria a senha do operador; URL do Plex, token e diretório de mídia precisam ser configurados depois, manualmente, em `/settings`. Além disso o sistema aceita **um único** `MEDIA_ROOT`, e o operador precisa digitar o caminho à mão — impossível quando o container monta bibliotecas em pontos diferentes (`/media/Animes`, `/media/Filmes`, `/mnt/nas/Series`). O scan também só resolve caminhos contra essa única raiz.

Existe ainda um bug de transporte: `PlexApiClient` envia URIs relativas e `PlexRequestHandler` as reescreve com `new Uri(baseUri, relative)`. Caminho relativo começando com `/` **descarta o path prefix** de `PLEX_URL` (ex.: `https://host/plex`) e a URL final aponta para o host errado.

## What Changes

- **Wizard de primeiro acesso** passa a coletar, na mesma tela: token de setup, nova senha, URL do Plex, token do Plex e diretórios de mídia. O Plex é validado (`GET /identity`) e os diretórios precisam existir antes de concluir; falha não cria credencial.
- **Múltiplos diretórios de mídia**: `MEDIA_ROOT` singular vira lista `MEDIA_ROOTS` persistida no SQLite (JSON no `AppSetting`), com seed retrocompatível de `MEDIA_ROOT`/env.
- **Seletor de diretório nativo**: botão "Procurar…" abre navegador de diretórios do **servidor** (`GET /api/filesystem/directories`), porque o browser não enxerga o filesystem do container. Lista apenas diretórios, com navegação por pai/filho.
- **Ajustes** (SettingsView) usa o mesmo campo de lista + seletor, com add/remover.
- **Processamento usa todas as raízes**: `IPathResolver` recebe a lista e testa cada raiz (sonda de sufixo) antes do fallback; scan manual, scan agendado e listagens passam a lista completa.
- **Fix `PlexApiClient`**: cliente monta URL absoluta a partir de `PLEX_URL` (preservando path prefix); `PlexRequestHandler` deixa de reescrever `RequestUri` e só injeta `X-Plex-Token` e `Accept`.

## Capabilities

### New Capabilities

- `setup-inicial`: primeiro acesso coleta credenciais do operador e configuração do Plex/mídia em um único fluxo validado.
- `media-roots`: lista de raízes de mídia, validação, persistência e navegador de diretórios do servidor.
- `plex-api-client`: contrato de transporte do cliente Plex com URL absoluta.

### Modified Capabilities

- `plex-scan-real`: resolução de caminho local passa a considerar todas as raízes configuradas.
- `settings-management`: `MEDIA_ROOT` singular substituído por `MEDIA_ROOTS`.

## Impact

**Backend (`src/`)**

- `Application/AppSettings.cs`, `AppSettingKeys`/`AppSettingDefaults`/`AppSettingsSnapshot` (`MediaRoots`).
- `Application/UpdateAppSettingsUseCase.cs` (validação de lista).
- `Application/MediaPathResolver.cs` (múltiplas raízes).
- `Domain/Ports` `IPathResolver` e `Infrastructure/Platform/PathResolver.cs`.
- `Application/UseCases/CompleteSetupUseCase.cs` (validação Plex + mídia).
- `Web/Endpoints/AuthEndpoints.cs` (payload do setup), `SettingsEndpoints.cs`, novo `FileSystemEndpoints.cs`, `PlexEndpoints.cs`/`SeriesEndpoints.cs` (uso da lista).
- `Infrastructure/PlexProxy/PlexApiClient*.cs`, `PlexRequestHandler.cs`.
- `Infrastructure/Persistence/Repositories/SqliteAppSettingsStore.cs` (seed/migração).

**Frontend (`vue/plex-frontend/src`)**

- `views/Authentication/SetupView.vue` (wizard), `views/Pages/SettingsView.vue`.
- Novos `components/MediaRootsField.vue`, `components/DirectoryPicker.vue`.
- `api.js` (chamadas de setup/settings/filesystem).

**Fora de escopo**

- Alterar montagem de volumes no `docker-compose.yml` (operador continua montando o que quiser; a UI só descobre caminhos existentes).
- Migração automática de dados existentes além do seed retrocompatível de `MEDIA_ROOT`.
