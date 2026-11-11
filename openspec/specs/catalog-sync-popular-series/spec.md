# catalog-sync-popular-series Specification

## Purpose
Define o comportamento do endpoint de sincronização do catálogo de séries, expondo o `CatalogSyncUseCase` existente para popular o banco local com bibliotecas e séries do Plex.

## Requirements

### Requirement: Expor endpoint de sincronização de catálogo
O sistema SHALL expor um endpoint `POST /api/sync/catalog` que executa `CatalogSyncUseCase.RunAsync()` e retorna o resultado da sincronização com contagens de bibliotecas e séries (inseridas, atualizadas, removidas).

#### Scenario: Endpoint executa sincronização de catálogo com sucesso
- **WHEN** o cliente chama `POST /api/sync/catalog`
- **THEN** o sistema executa `CatalogSyncUseCase.RunAsync()` e retorna status 200 com contagens de bibliotecas e séries inseridas/atualizadas/removidas

#### Scenario: Plex server indisponível
- **WHEN** o servidor Plex está inacessível e `CatalogSyncUseCase.RunAsync()` retorna falha
- **THEN** o sistema retorna status 502 com mensagem de erro indicando que o servidor Plex está indisponível

#### Scenario: Catálogo de séries populado no banco local
- **WHEN** a sincronização de catálogo é executada com sucesso
- **THEN** o banco local contém registros de bibliotecas do tipo `show` e suas séries, permitindo que `GET /api/series/{sectionKey}` retorne dados não vazios
