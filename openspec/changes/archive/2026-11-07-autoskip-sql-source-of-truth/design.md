# Design

## Context

Hoje o domínio de marcadores (`intro`/`credits`, chaveados por `RatingKey`) vive duplicado: o `IMarkerRepository` (SQLite) é source of truth para gravação, leitura por API/scan/sync, enquanto o `IPlexAutoSkipService` (`PlexAutoSkipService`) lê exclusivamente de `custom.json` com `FileSystemWatcher`. A gravação espelha para o JSON em `MarkerEndpoints`, `PlexEndpoints` (scan) e `SyncBackgroundService`, e `DELETE /api/markers/{ratingKey}/{kind}` não atualiza o JSON (inconsistência latente). Veja `proposal.md - Why` para a motivação.

## Goals / Non-Goals

**Goals:**
- SQLite é a única fonte de verdade para marcadores consumidos pelo autoskip (WebSocket `/ws`).
- Remover completamente `custom.json`, `ICustomJsonWriter`, `CustomJsonWriterService`, `FileSystemWatcher` e a variável `CUSTOM_JSON_PATH` do fluxo de autoskip.
- Tornar `DELETE /api/markers/{ratingKey}/{kind}` consistente (remove apenas do SQLite; não há mais JSON para sincronizar).
- Consultar o SQLite a cada mensagem de `play` do WebSocket (simples, consistente, sem cache desalinhado entre instâncias).

**Non-Goals:**
- Manter `custom.json` como export/backup ou fallback de leitura.
- Migrar dados existentes de `custom.json` para SQLite (os marcadores já gravados no SQL continuam válidos; o JSON órfão pode ser removido manualmente pelos operadores).
- Alterar o esquema da tabela `Markers` (RatingKey, Kind, StartMs, EndMs, Origin já existe).

## Decisions

**D1 — Source of truth do autoskip é o SQLite, consultado por mensagem.**
O `PlexAutoSkipService` recebe `IMarkerRepository` via DI e chama `GetMarkersForEpisode(ratingKey)` a cada mensagem `play` do WebSocket. SQLite responde a leitura por PK em microssegundos; o custo por mensagem de playback é desprezível e elimina qualquer risco de cache desalinhado entre instâncias do app.

*Alternativa considerada:* cache em memória carregado ao iniciar com notificação interna de mudança. Rejeitado por adicionar complexidade (sinalização entre camadas, coordenação entre instâncias) sem benefício mensurável dado o tamanho típico da tabela Markers.

**D2 — Remoção total do `custom.json` e de `ICustomJsonWriter`.**
Como não há consumidores externos confirmados do arquivo (apenas este app lê/escreve), a porta `ICustomJsonWriter` e a implementação `CustomJsonWriterService` são removidas, junto com `CUSTOM_JSON_PATH` e o `FileSystemWatcher` do `PlexAutoSkipService`. As chamadas `customJsonWriter.UpsertMarkers` saem de `MarkerEndpoints` (POST/PUT), `PlexEndpoints` (scan) e `SyncBackgroundService`.

**D3 — Nenhuma mudança no esquema `Markers`.**
A tabela já tem `(RatingKey, Kind, StartMs, EndMs, Origin)` com PK `(RatingKey, Kind)`, suficiente para atender o autoskip.

## Risks / Trade-offs

- [Risco] Leitura do SQLite a cada mensagem de `play` do WebSocket aumenta leituras no banco. → Mitigada: leitura por PK em SQLite é barata; o volume de mensagens de playback é limitado pelos jogadores conectados.
- [Risco] Clientes/operadores que dependem do `custom.json` como arquivo de saída perdem essa funcionalidade. → Mitigada: decisão explícita do usuário (eliminar); documentar remoção no `README.md`.
- [Trade-off] Não há migração automática do conteúdo existente do `custom.json` para o SQL. → Aceito: o SQL já é preenchido pelo scan/sync/API; o JSON órfão não afeta o comportamento do sistema.
- [Trade-off] `DELETE` deixa de gravar no JSON, mas como o JSON deixa de existir, não há regressão.

## Open Questions

Nenhuma bloqueadora. Todas as decisões de arquitetura foram tomadas com base nas respostas do usuário (não consumidores externos, eliminar JSON, consulta SQL por mensagem).
