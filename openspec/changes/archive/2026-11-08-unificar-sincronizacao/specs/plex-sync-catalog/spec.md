# Spec Delta — plex-sync-catalog

## ADDED Requirements

### Requirement: Catálogo como primeira etapa da sincronização unificada

O sistema SHALL sincronizar bibliotecas e séries do Plex para o banco local
como **etapa 1** de um único processo de sincronização, executado antes da
etapa de varredura de marcadores.

#### Scenario: Etapa 1 executa antes dos marcadores

- **WHEN** um ciclo de sincronização ou um force sync inicia
- **THEN** bibliotecas de séries (`show`) são descobertas e persistidas antes de
  qualquer episódio ser varrido para marcadores

#### Scenario: Ordem garantida no force sync

- **WHEN** o botão **Sincronizar** é acionado
- **THEN** `POST /api/sync/run` executa etapa 1 (catálogo) completa e só então a
  etapa 2 (marcadores de episódios)

#### Scenario: Catálogo atualizado alimenta a etapa 2

- **WHEN** a etapa 1 insere uma biblioteca ou série nova
- **THEN** a etapa 2 do mesmo ciclo já considera as bibliotecas retornadas pela
  API do Plex para varrer episódios