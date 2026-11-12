# Proposal

## Why

Quando o usuário abre uma biblioteca no frontend, pode acontecer de o catálogo de séries ainda não ter sido sincronizado (o `CatalogSyncBackgroundService` roda a cada 5 min com delay inicial). Atualmente o usuário vê uma lista vazia ("Nenhuma série encontrada") sem saber por quê, achando que não há séries ou que há erro.

## What Changes

1. **Mensagem informativa de catálogo não processado**: Quando a biblioteca não tem séries no banco local (totalSize = 0), exibir uma mensagem clara explicando que o catálogo ainda não foi sincronizado e sugerir aguardar a sincronização automática ou forçar manualmente.

2. **Botão "Forçar sincronização"**: Adicionar botão na UI da biblioteca (na view de séries) que dispara o endpoint `/api/sync/catalog` para sincronizar o catálogo imediatamente. Mostrar feedback de carregamento e resultado.

## Escopo

- **Frontend**: `SeriesView.vue` - adicionar estado de sincronização, mensagem informativa e botão de forçar sync
- **Frontend**: `useSeriesCatalog.js` - adicionar função `forceSyncCatalog()` que chama `/api/sync/catalog`
- **Backend**: Endpoint `/api/sync/catalog` já existe (em `SyncEndpoints.cs`), apenas será consumido pelo frontend