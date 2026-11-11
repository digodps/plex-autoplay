# Proposal: Popular Catálogo de Séries Local via CatalogSyncUseCase

## Why

O `CatalogSyncUseCase` já existe e popula corretamente bibliotecas e séries do Plex no banco local, mas nunca é chamado nem registrado no DI. Com isso, o catálogo de séries permanece vazio no banco local e `GET /api/series/{sectionKey}` retorna vazio, impedindo a navegação do usuário pelas séries da biblioteca.

## What Changes

- Registrar `CatalogSyncUseCase` no container de DI (ServiceCollectionExtensions).
- Expor endpoint `POST /api/sync/catalog` que executa `CatalogSyncUseCase.RunAsync()` e retorna o resultado da sincronização (contagens de bibliotecas e séries inseridas/atualizadas/removidas).

## Capabilities

### New Capabilities
- `catalog-sync-popular-series`: endpoint de disparo manual de sincronização do catálogo de séries para o banco local.

### Modified Capabilities
- None

## Impact

- Endpoint de API backend (`POST /api/sync/catalog`).
- Registro de `CatalogSyncUseCase` no DI.
- Banco de dados local (tabelas de bibliotecas e séries são preenchidas pelo `CatalogSyncUseCase` existente).
- Sem alteração no frontend (o endpoint é um serviço de backend; consumo pode ser feito por outra change).
