# Change Proposal — unificar-sincronizacao

## Why

O sistema roda hoje dois `BackgroundService` independentes com intervalos e
lógicas duplicadas:

- `SyncBackgroundService` varre bibliotecas de séries e detecta marcadores
  (ffprobe) a cada `sync_interval_minutes` (default 5).
- `CatalogSyncBackgroundService` sincroniza bibliotecas e séries do Plex a cada
  `sync_catalog_interval_minutes` (default 30).

Esses dois processos não têm ordem garantida: a varredura de marcadores pode
rodar antes do catálogo estar atualizado, processando episódios que já não
existem ou ignorando séries recém-adicionadas. Além disso, o endpoint do botão
**Sincronizar** (`POST /api/plex/scan`) duplica a lógica de varredura de
marcadores em vez de reutilizá-la, e nunca sincroniza o catálogo.

## What Changes

- Unifica os dois serviços em **um único** `BackgroundService` de
  sincronização que executa **duas etapas encadeadas por ciclo**: (1) catálogo
  do Plex (bibliotecas + séries) e, em seguida, (2) marcadores de episódios.
- Substitui os dois intervalos (`sync_interval_minutes` e
  `sync_catalog_interval_minutes`) por **um único intervalo** de ciclo,
  aplicado ao conjunto das duas etapas.
- O botão **Sincronizar** passa a forçar uma execução completa (catálogo +
  marcadores) do mesmo serviço, via endpoint dedicado, com `jobId` e progresso
  SSE preservados (incluindo a etapa corrente no log).
- Remove `SyncBackgroundService` e `CatalogSyncBackgroundService`, extraindo a
  lógica compartilhada para um caso de uso/serviço de orquestração único.
- Atualiza as specs `plex-sync`, `plex-sync-catalog` e `settings-management`.

## Capabilities

### New Capabilities

- `synchronization`: orquestração única de sincronização em duas etapas
  encadeadas (catálogo → marcadores), agendada e executável sob demanda.

### Modified Capabilities

- `plex-sync`: a varredura de marcadores deixa de ser um serviço independente e
  passa a ser a etapa 2 do processo único de sincronização.
- `plex-sync-catalog`: a sincronização de catálogo passa a ser a etapa 1 do
  processo único de sincronização.
- `settings-management`: dois intervalos são substituídos por um único
  intervalo de sincronização.

## Impact

- Backend: remoção de `SyncBackgroundService` e `CatalogSyncBackgroundService`;
  novo serviço orquestrador em `PlexAutoplay.Infrastructure/Sync`; novo endpoint
  de sincronização forçada; `POST /api/plex/scan` passa a delegar ao orquestrador;
  DI em `ServiceCollectionExtensions`; `AppSettingsSnapshot` e
  `UpdateAppSettingsUseCase` com intervalo único.
- Frontend: `SyncView.vue` chama o novo endpoint; `SettingsView.vue` exibe um
  único campo de intervalo; `dashboard`/cards de sync ajustados se referirem a
  dois intervalos.
- Banco: as settings persistidas passam a ter uma chave de intervalo; migração
  ou leitura tolerante a valor antigo.
- Sem novas dependências.
