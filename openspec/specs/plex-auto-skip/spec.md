# plex-auto-skip Specification

## Purpose
Defines the behavior for a .NET WebSocket service that reads `custom.json` and serves seek commands to compatible Plex players, replacing the Python PlexAutoSkip component.

## Requirements

### Requirement: Serve WebSocket on port 8081
O sistema SHALL expor um endpoint WebSocket sobre TLS (WSS) na porta 8081 para players compatíveis com o protocolo Plex Companion.

#### Scenario: WebSocket connection accepted
- **WHEN** a compatible Plex player connects to `wss://localhost:8081`
- **THEN** the system establishes a secure WebSocket connection over TLS

#### Scenario: Plain WS connection rejected
- **WHEN** um cliente tenta se conectar via WS (texto puro, sem TLS)
- **THEN** o sistema não aceita a conexão

### Requirement: Read custom.json for markers
O sistema SHALL ler os dados de marcador de custom.json (o mesmo arquivo gravado pelo serviço de detecção de marcadores) para determinar os pontos de pular.

#### Scenario: custom.json loaded at startup
- **WHEN** o serviço PlexAutoSkip inicia
- **THEN** o sistema carrega todos os marcadores de custom.json na memória

#### Scenario: custom.json updated during runtime
- **WHEN** custom.json é modificado pelo serviço de varredura de marcadores
- **THEN** o sistema recolhe os marcadores atualizados sem exigir reinicialização

### Requirement: Serve seek commands to players
O sistema SHALL responder a mensagens de controle do player com comandos de seek que pulem segmentos de intro ou créditos com base nos marcadores do episódio em reprodução.

#### Scenario: Player requests skip for intro
- **WHEN** um player conectado sinaliza reprodução de um episódio com marcador de intro (0ms a 90000ms)
- **THEN** o sistema envia um comando de seek para pular para 90000ms

#### Scenario: Player requests skip for credits
- **WHEN** um player conectado sinaliza reprodução de um episódio com marcador de créditos (85% até o final)
- **THEN** o sistema envia um comando de seek para pular para o final dos créditos

#### Scenario: Player incompatible with WebSocket protocol
- **WHEN** um Plex Web App ou cliente iOS tenta se conectar
- **THEN** o sistema não consegue servir comandos de skip (limitação conhecida do protocolo Plex Companion descontinuado)

#### Scenario: WebSocket interprets player control messages
- **WHEN** uma mensagem de controle do protocolo Plex Companion é recebida via WebSocket
- **THEN** o sistema interpreta a mensagem e responde com o comando de seek apropriado com base nos marcadores do episódio
