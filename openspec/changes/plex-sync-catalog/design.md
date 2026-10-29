# Design

## Context

O sistema já possui: `SyncBackgroundService` (varredura periódica de marcadores via ffprobe), `SqliteMarkerRepository` (tabela `Markers` com RatingKey, Kind, StartMs, EndMs, Origin), `FfmpegMarkerDetector`, `CustomJsonWriterService` (custom.json), `IMarkerRepository`, `IPlexApiClient` (GetLibraries, GetEpisodes, GetSeries, GetSeriesEpisodes), `MediaPathResolver` (MEDIA_ROOT), endpoints `PlexEndpoints`, `SeriesEndpoints`, `SyncEndpoints`, `MarkerEndpoints`. Os specs `plex-libraries`, `series-catalog`, `plex-markers` e `plex-sync` já existem. Hoje nenhum dado de biblioteca ou série é persistido no banco; toda listagem consulta a API do Plex.

Veja `proposal.md` para a motivação (Why) e o escopo (What Changes).

## Goals / Non-Goals

**Goals:**
- Persistir bibliotecas do tipo `show` e séries (rating key, título, summary, thumb, biblioteca) no SQLite.
- Endpoints de catálogo (`/api/series/{sectionKey}` com paginação e busca) lerem do banco local via SQL, não da API do Plex.
- Sincronização incremental em serviço separado: inserir bibliotecas/séries novas, atualizar alteradas, remover (hard delete) o que não existe mais no Plex.
- Reaproveitar `FfmpegMarkerDetector`, `IMarkerRepository` e `custom.json` para detecção de marcadores (inalterada).
- Migração aditiva: novas tabelas sem alterar a tabela `Markers` existente.

**Non-Goals:**
- Alterar a detecção de marcadores por ffprobe nem o formato `custom.json`.
- Mover o `SyncBackgroundService` existente para o novo serviço; eles rodam de forma independente.
- Sincronizar episódios individualmente no banco além do catálogo de séries (episódios continuam sendo resolvidos via `MediaPathResolver` quando necessário).

## Decisions

### 1. Novo serviço de sincronização de catálogo, separado do `SyncBackgroundService`
**Decisão:** Criar `CatalogSyncBackgroundService` (ou similar) em `PlexAutoplay.Infrastructure/Sync`, registrado como `AddHostedService`, com intervalo configurável distinto do serviço de marcadores.
**Rationale:** O usuário pediu um serviço separado. O serviço de marcadores já tem seu próprio ciclo, progresso SSE e cancelamento; misturar ciclos diferentes (catálogo muda pouco, marcadores exigem ffprobe pesado) acoplaria dois escopos com frequências e falhas distintas.
**Alternativa considerada:** Usar o mesmo `SyncBackgroundService` para ambas as tarefas. Rejeitado porque acoplariam ciclos de execução e erros de um afetariam o outro.

### 2. Tabelas `Library` e `Series` no SQLite (mesmo banco do `Markers`)
**Decisão:** Criar tabelas `Library` (RatingKey PK, Title, Type, Agent, SyncedAt) e `Series` (RatingKey PK, LibraryRatingKey FK, Title, Summary, Thumb, SyncedAt) no mesmo arquivo SQLite (`SQLITE_CONNECTION_STRING` / `/data/app.db`).
**Rationale:** Simplicidade operacional — um único banco, uma connection string, migração aditiva via `CREATE TABLE IF NOT EXISTS`. Não requer novo serviço externo.
**Alternativa considerada:** Banco separado para catálogo. Rejeitado por adicionar complexidade de deploy e backup sem benefício claro.

### 3. Hard delete de bibliotecas e séries sumidas do Plex
**Decisão:** Remover fisicamente do banco as bibliotecas e séries que não aparecem mais na API do Plex.
**Rationale:** O usuário solicitou remoção. Evita acúmulo de dados órfãos e mantém o banco espelhando o estado atual do servidor Plex.
**Risco:** Marcadores já criados pelo usuário para episódios de uma série removida ficam órfãos na tabela `Markers` (sem série vinculada). Isso é aceitável porque `Markers` é indexada por RatingKey e continua funcional; a UI pode filtrar marcadores por séries existentes.

### 4. Leitura do catálogo no banco com paginação e busca em SQL
**Decisão:** `SeriesEndpoints` (`/api/series/{sectionKey}`) passa a consultar `Series` filtrado por `LibraryRatingKey`, com `LIMIT/OFFSET` para paginação e `LIKE` para busca por título.
**Rationale:** Remove a dependência da API do Plex para cada listagem, reduzindo latência e chamadas ao servidor.
**Risco:** O banco pode ficar desatualizado entre sincronizações. Mitigado pela execução periódica do `CatalogSyncBackgroundService`.

### 5. Fluxo de sincronização (diff)
Para cada ciclo:
1. `GetLibrariesAsync()` → filtrar `type == "show"` → upsert em `Library`; deletar bibliotecas no banco não retornadas pelo Plex (cascade delete de `Series` vinculadas).
2. Para cada biblioteca de séries: `GetSeriesAsync(sectionKey, ...)` → upsert em `Series`; deletar séries no banco para aquela biblioteca não retornadas pelo Plex.
3. Logar contadores (processados, inseridos, atualizados, removidos, erros) para observabilidade.

## Risks / Trade-offs

[Banco desatualizado entre sincronizações] → O catálogo reflete o estado do Plex apenas no último ciclo de sync; aumentar a frequência do `CatalogSyncBackgroundService` reduz a janela, mas consome mais chamadas à API do Plex.

[Rate limiting do servidor Plex] → Sincronizações muito frequentes de todas as bibliotecas podem atingir limites do Plex. Mitigar com intervalo configurável padrão razoável (ex.: 15–60 min) e backoff em caso de erro 429.

[Marcadores órfãos após hard delete] → Quando uma série é removida do banco, seus marcadores em `Markers` permanecem órfãos (indexados por RatingKey). Não quebram funcionalidade, mas acumulam dados. Pode ser abordado por limpeza opcional futura.

[Consistência entre `custom.json` e SQLite] → Ambos são gravados pelo scanner de marcadores existente, não afetado por esta mudança; sem novo risco de consistência introduzido aqui.

[Performance de `LIKE` em títulos para busca] → Para bibliotecas muito grandes, busca por título com `LIKE '%termo%'` pode ser lenta. Mitigar com índice em `Series(Title)` se necessário; não é requisito inicial.

## Open Questions

- Intervalo padrão de execução do `CatalogSyncBackgroundService` (15 min, 30 min, 60 min?) — pode ser ajustado por variável de ambiente sem alterar specs ou tasks.
- Se episódios devem ser listados do banco no futuro (não neste escopo).
