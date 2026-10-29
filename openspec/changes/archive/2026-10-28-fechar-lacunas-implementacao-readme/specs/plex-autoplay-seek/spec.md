# plex-autoplay-seek Specification

## Purpose

Define o comportamento do servidor WebSocket (WSS na porta 8081) que interpreta mensagens do protocolo Plex Companion enviadas por players compatíveis e responde com comandos de seek para pular segmentos de intro e créditos com base nos marcadores armazenados em custom.json.

## ADDED Requirements

### Requirement: Servir WebSocket Secure na porta 8081
O sistema SHALL expor um endpoint WebSocket sobre TLS (WSS) na porta 8081 para players compatíveis com o protocolo Plex Companion.

#### Scenario: Conexão WSS aceita
- **WHEN** um player Plex compatível se conecta ao endpoint WSS na porta 8081
- **THEN** o sistema estabelece uma conexão WebSocket segura sobre TLS

#### Scenario: Conexão WS em texto puro rejeitada
- **WHEN** um cliente tenta se conectar via WS (texto puro, sem TLS)
- **THEN** o sistema não aceita a conexão

### Requirement: Ler marcadores de custom.json
O sistema SHALL ler os marcadores do arquivo custom.json na inicialização e recarregá-los quando o arquivo for modificado, para determinar os pontos de pular (skip) de cada episódio.

#### Scenario: custom.json carregado na inicialização
- **WHEN** o serviço PlexAutoSkip inicia
- **THEN** o sistema carrega todos os marcadores de custom.json na memória

#### Scenario: custom.json atualizado durante a execução
- **WHEN** o arquivo custom.json é modificado pelo serviço de varredura de marcadores
- **THEN** o sistema recolhe os marcadores atualizados sem exigir reinicialização

### Requirement: Enviar comandos de seek para pular intro e créditos
O sistema SHALL responder a mensagens de controle do player com comandos de seek que pulem os segmentos de intro ou créditos com base nos marcadores do episódio em reprodução.

#### Scenario: Player solicita pular intro
- **WHEN** um player conectado sinaliza reprodução de um episódio com marcador de intro (0ms a 90000ms)
- **THEN** o sistema envia um comando de seek para pular para 90000ms

#### Scenario: Player solicita pular créditos
- **WHEN** um player conectado sinaliza reprodução de um episódio com marcador de créditos (85% até o final)
- **THEN** o sistema envia um comando de seek para pular para o final dos créditos

#### Scenario: Player incompatível com o protocolo WebSocket
- **WHEN** um app Plex Web ou iOS tenta se conectar
- **THEN** o sistema não consegue servir comandos de skip (limitação conhecida do protocolo Plex Companion descontinuado)
