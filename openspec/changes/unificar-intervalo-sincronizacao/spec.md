# Spec Delta — unificar-intervalo-sincronizacao

## Change
Unificar a configuração de sincronização em um único intervalo e executar o processamento em duas etapas ordenadas dentro desse intervalo: etapa 1 busca bibliotecas no Plex (catálogo), etapa 2 busca marcadores nos diretórios de mídia.

## Motivação
Atualmente há dois intervalos independentes (`sync_interval_minutes` = 5 para marcadores e `sync_catalog_interval_minutes` = 30 para catálogo) e dois `BackgroundService` separados (`SyncBackgroundService` e `CatalogSyncBackgroundService`) rodando sem ordem garantida. O spec `synchronization` exige um único processo de duas etapas (catálogo → marcadores) governado por um único intervalo.

## Requisitos

### R1 — Único intervalo de configuração
O sistema SHALL expor exatamente um campo `Intervalo de Sincronização (minutos)` em `/settings`, com valor válido entre 1 e 1440 minutos (default 5). Esse intervalo governa as duas etapas do ciclo.

### R2 — Duas etapas ordenadas por ciclo
A cada intervalo, o processo SHALL executar, em ordem:
- Etapa 1: buscar bibliotecas no Plex (sincronização de catálogo Plex → banco).
- Etapa 2: buscar marcadores nos diretórios de mídia (varredura de episódios → detecção → banco).

### R3 — Reutilização do mesmo processo
O mesmo processo de duas etapas SHALL ser reutilizado tanto pelo agendamento periódico quanto pelo comando de sincronização forçada (`POST /api/sync/run`).

### R4 — Falha na etapa 1 impede etapa 2
Se a etapa 1 falhar, a etapa 2 não inicia e o job termina em `failed`.

### R5 — Progresso por etapa
Os logs/status SHALL informar claramente a etapa corrente (catálogo ou marcadores) e as contagens cumulativas de processados/pulados/erros.
