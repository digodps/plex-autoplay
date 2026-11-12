# Spec Delta: Dashboard Card - Sessões Ativas e Histórico de Skip

## ADDED Requirements

### Requirement: Card "Sessões Ativas & Skips" no Dashboard
O Dashboard SHALL exibir um card "Sessões Ativas & Skips" ao lado dos cards existentes (Serviço, Última Sincronização, Bibliotecas).

#### Scenario: Card exibe contagem de sessões ativas
- **WHEN** há sessões ativas no Plex (estado "playing")
- **THEN** o card mostra o número de sessões ativas como valor principal

#### Scenario: Card exibe último skip executado
- **WHEN** houve pelo menos um skip executado recentemente
- **THEN** o card mostra o timestamp do último skip como valor secundário/breakdown

#### Scenario: Card é clicável
- **WHEN** o usuário clica no card
- **THEN** navega para rota `/sessions` (página de detalhes de sessões e skips)

### Requirement: Endpoint `/api/sessions/skip-status`
O backend SHALL expor `GET /api/sessions/skip-status` retornando sessões ativas e histórico de skips.

#### Scenario: Lista sessões ativas com progresso
- **WHEN** chamar GET `/api/sessions/skip-status`
- **THEN** resposta inclui array `activeSessions` com objetos:
  - `machineIdentifier` (string)
  - `product` (string)
  - `platform` (string)
  - `ratingKey` (int)
  - `positionMs` (int) — posição atual do playback
  - `durationMs` (int) — duração total do item
  - `nextMarker` (object|null) — próximo marcador de intro/créditos se houver:
    - `kind` (string: "intro" | "credits")
    - `startMs` (int)
    - `endMs` (int)
    - `msUntilSkip` (int) — diferença `startMs - positionMs` se `positionMs < startMs`, senão 0
  - `state` (string) — "playing" | "paused" | "buffering"

#### Scenario: Histórico de skips recentes
- **WHEN** chamar GET `/api/sessions/skip-status`
- **THEN** resposta inclui array `recentSkips` (últimos 50) com objetos:
  - `timestamp` (ISO 8601 string) — quando o skip foi executado
  - `machineIdentifier` (string)
  - `product` (string)
  - `ratingKey` (int)
  - `markerKind` (string: "intro" | "credits")
  - `skippedFromMs` (int) — posição antes do seek
  - `skippedToMs` (int) — posição depois do seek (endMs do marcador)

#### Scenario: Endpoint protegido
- **WHEN** chamar sem autenticação
- **THEN** retorna 401

### Requirement: Página de Detalhes `/sessions`
O frontend SHALL ter rota `/sessions` acessível via clique no card do Dashboard.

#### Scenario: Lista sessões ativas
- **WHEN** acessar `/sessions`
- **THEN** exibe tabela/lista de sessões ativas com:
  - Cliente (product + platform)
  - Episódio/Filme (ratingKey - opcional título se disponível)
  - Progresso visual (barra) com posição atual
  - Marcador próximo (se houver): tipo, tempo restante até skip
  - Estado (playing/paused/buffering)

#### Scenario: Histórico de skips
- **WHEN** acessar `/sessions`
- **THEN** exibe tabela/lista de skips recentes (últimos 50) com:
  - Data/hora do skip
  - Cliente
  - Tipo de marcador (intro/créditos)
  - De → Para (posição antes/depois)

#### Scenario: Auto-refresh
- **WHEN** na página `/sessions`
- **THEN** atualiza dados a cada 5 segundos (polling) enquanto aba ativa

#### Scenario: Tema light/dark
- **WHEN** alternar tema
- **THEN** cores da tabela, barras de progresso, badges respeitam tema

### Requirement: Registro de histórico de skip no Background Service
O `PlexAutoSkipBackgroundService` SHALL registrar cada skip executado no histórico.

#### Scenario: Skip executado
- **WHEN** o background service executa seek bem-sucedido
- **THEN** grava registro no histórico (timestamp, machineIdentifier, product, ratingKey, markerKind, skippedFromMs, skippedToMs)

#### Scenario: Histórico persiste entre restarts
- **WHEN** aplicação reinicia
- **THEN** histórico de skips permanece disponível (SQLite)

## REMOVED Requirements
Nenhum.