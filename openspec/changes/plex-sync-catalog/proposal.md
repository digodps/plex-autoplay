# Proposal

## Why

Hoje o sistema consulta a API do Plex a cada listagem de bibliotecas e catálogo de séries, e não persiste bibliotecas nem séries no banco. Isso torna cada busca dependente da disponibilidade e latência do servidor Plex, e não há histórico local do que já foi sincronizado. A mudança cria um catálogo local (bibliotecas de tipo série + séries com episódios e marcadores) consultável pelo banco, com sincronização incremental que remove do banco o que não existe mais no Plex.

## What Changes

- Novo serviço de sincronização de catálogo, separado do `SyncBackgroundService` de detecção de marcadores.
- Persistência de bibliotecas do tipo `show` no SQLite (tabela `Library`).
- Persistência de séries (rating key, título, summary, thumb, biblioteca) no SQLite (tabela `Series`).
- Endpoints de catálogo (`/api/series/{sectionKey}` com paginação e busca) passam a ler do banco em vez da API do Plex.
- Sincronização incremental: bibliotecas/séries novas são inseridas; bibliotecas/séries que sumiram do Plex são removidas do banco (hard delete).
- Reaproveita `FfmpegMarkerDetector` e `IMarkerRepository` para associar marcadores existentes às séries do catálogo.

## Capabilities

### New Capabilities
- `plex-sync-catalog`: sincronização incremental de bibliotecas e séries do Plex para o banco local, com detecção de adições e remoções (hard delete), e serviço separado do scanner de marcadores.

### Modified Capabilities
- `plex-libraries`: o requisito de mapear biblioteca para mídia local agora também grava a biblioteca no banco, não apenas lê da API.
- `series-catalog`: o endpoint de listagem de séries passa a consultar o banco (paginação e busca em SQL) em vez de chamar `GetSeriesAsync` da API do Plex a cada requisição.

## Impact

- Código novo: entidades `Library` e `Series` em `PlexAutoplay.Domain/Entities`; repositórios SQLite em `PlexAutoplay.Infrastructure/Persistence/Repositories`; serviço de sincronização de catálogo em `PlexAutoplay.Infrastructure/Sync` (novo, separado do `SyncBackgroundService` existente).
- Endpoints alterados: `SeriesEndpoints` (leitura do banco), possivelmente `PlexEndpoints` (remoção ou ajuste do scan de catálogo).
- Bancos existentes: migração aditiva para criar tabelas `Library` e `Series` (sem quebrar a tabela `Markers` existente).
- Nenhuma mudança na detecção de marcadores via ffprobe nem no `custom.json`.
