# Proposal — unificar-intervalo-sincronizacao

## Por que esta mudança?
Simplificar a configuração de processamento, utilizando somente uma configuração: um único intervalo (`sync_interval_minutes`). Nesse intervalo, o sistema executa o processamento em duas etapas ordenadas: primeiro busca bibliotecas no Plex (catálogo), depois busca marcadores nos diretórios de mídia.

## O que muda?
- Remoção do campo/configuração legada de intervalo de catálogo (`sync_catalog_interval_minutes`).
- Frontend (`SettingsView.vue`): exibe apenas um campo `Intervalo de Sincronização (minutos)`.
- Backend: unificação dos serviços de sincronização em um ciclo periódico de duas etapas.
