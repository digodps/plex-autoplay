# Design

## Context

O sistema já possui o `CatalogSyncBackgroundService` (sincronização periódica de bibliotecas e séries do Plex para o SQLite) e o endpoint `POST /api/sync/catalog` que dispara a sincronização sob demanda. A view `SeriesView.vue` exibe as séries de uma biblioteca usando o composable `useSeriesCatalog.js`, que lê do banco local via `/api/series/{sectionKey}`. Quando o catálogo ainda não foi sincronizado, a lista fica vazia sem explicação.

## Goals / Non-Goals

**Goals:**
- Exibir mensagem informativa quando o catálogo de uma biblioteca não foi processado (0 séries no banco).
- Adicionar botão "Forçar sincronização" que chama `POST /api/sync/catalog`.
- Feedback visual de carregamento durante a sincronização forçada.
- Mensagem de sucesso/erro após a sincronização.

**Non-Goals:**
- Alterar a lógica de sincronização de backend (endpoint `/api/sync/catalog` já existe e é reutilizado).
- Persistir data/hora do último sync por biblioteca no banco (não é necessário para o MVP).
- CRUD completo; apenas navegação/interação visual.

## Decisions

### 1. Detectar catálogo não processado via totalSize == 0

**Decisão:** No frontend, quando `seriesTotalSize === 0` e não há erro de conexão, exibir a mensagem informativa. Isso indica que o catálogo ainda não foi sincronizado (ou a biblioteca está vazia no Plex).
**Rationale:** É o sinal mais confiável disponível no endpoint existente `/api/series/{sectionKey}` que lê do banco local.

### 2. Reutilizar endpoint `POST /api/sync/catalog`

**Decisão:** Chamar o endpoint existente `POST /api/sync/catalog` que já executa `CatalogSyncUseCase.RunAsync`.
**Rationale:** Não requer alteração de backend; apenas consumo do endpoint existente.

### 3. Estado de sincronização no frontend

**Decisão:** Adicionar estados `syncLoading`, `syncMessage`, `syncType` no composable `useSeriesCatalog.js` e no `SeriesView.vue`.
**Rationale:** Permite feedback visual claro durante e após a sincronização forçada.