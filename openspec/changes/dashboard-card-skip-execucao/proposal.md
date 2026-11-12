# Proposal: Visibilidade de Skip e Sessões Ativas no Dashboard

## Why
Atualmente, o sistema executa o auto-skip em segundo plano (`PlexAutoSkipBackgroundService`), mas não fornece feedback visual em tempo real no Dashboard sobre quais sessões estão ativas, quando o skip vai ser executado (posição atual vs marcador) e quando o skip já foi executado. Usuários precisam de um meio (card no Dashboard + página de detalhes) para validar se o skip ocorreu e monitorar o estado das sessões.

## What Changes
- **Backend (.NET)**:
  - `ISkipHistoryStore` / `SkipHistoryTracker` — registra histórico de skips executados recentemente (timestamp, cliente, tipo de marcador, posição).
  - Endpoint `GET /api/sessions/skip-status` — retorna lista de sessões ativas no Plex, se estão em cima de um marcador (quando o skip vai ser executado) e o histórico de skips recentes (quando o skip executou).
- **Frontend (Vue)**:
  - `useDashboardCards.js` — adiciona card "Sessões Ativas & Skips" no Dashboard, mostrando contagem de sessões ativas e último skip executado.
  - Nova View `SessionsView.vue` (ou componente modal/detalhado) acessível ao clicar no card, listando sessões ativas, progresso, contagem regressiva/posição para skip, e histórico de skips executados.
  - Rota `/sessions` adicionada ao router.

## Impact
- Backend: Novo serviço de histórico de skips e endpoint `/api/sessions/skip-status`.
- Frontend: Novo card no dashboard, nova view de sessões/skips, rota frontend.
- Sem breaking changes.
