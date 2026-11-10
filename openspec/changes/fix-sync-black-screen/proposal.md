# Proposal

## Why

Na tela de Sincronização (`/sync`), ao clicar em "Iniciar Escaneamento", o fluxo:
1. Chama `POST /api/plex/scan` e recebe um `job_id`.
2. Conecta SSE em `/api/sync/log/stream/{jobId}` para atualizar status/logs em tempo real.

Hoje a tela fica preta após iniciar o escaneamento: o status não sai de `idle`, o console de logs não é preenchido e a UI deixa de responder visualmente. Possíveis causas:
- Falha silenciosa no `EventSource` (SSE) sem tratamento de `onerror`.
- Erro `401` não tratado no `apiFetch`, redirecionando para `/login` e descarregando a shell.
- Exceção JavaScript não capturada quebrando o componente Vue (`SyncView`).
- Problema de tema/CSS (classes dark/light) ocultando o conteúdo.
- Transição `page` do `router-view` travando na saída/entrada da página.

O usuário relata que a tela fica preta ao mandar sincronizar. É necessário corrigir para que o escaneamento seja iniciado, o status atinja `running`, os logs apareçam via SSE e o tema/menus continuem funcionando.

## What Changes

- Investigar e corrigir a causa da tela preta no `SyncView.vue` e no fluxo de scan/SSE.
- Garantir tratamento de erro do `EventSource` (onerror) com feedback ao usuário.
- Garantir que erros 401/HTTP no `apiFetch` não deixem a tela branca/preta silenciosamente.
- Validar que o tema (light/dark toggle) continua aplicado nos componentes de sincronização.
- Validar que menus/navegação continuam funcionando após o escaneamento.

## Capabilities

### New Capabilities
- None (bug fix).

### Modified Capabilities
- `sincronizacao`: UI de escaneamento deve iniciar, atualizar status e logs via SSE sem deixar tela preta.

## Impact

**Frontend (`vue/plex-frontend`):**
- `src/views/Sync/SyncView.vue` (iniciar scan, conectar SSE, tratar erros).
- `src/api.js` (`apiFetch`, `redirectToLoginPreservingRoute`, tratamento de 401).
- Possíveis ajustes de tema em `src/styles` e componentes de tela.

**Backend (`src/PlexAutoplay.Web`):**
- `src/PlexAutoplay.Web/Endpoints/PlexEndpoints.cs` (`/api/plex/scan`).
- `src/PlexAutoplay.Web/Endpoints/SyncEndpoints.cs` (`/api/sync/log/stream` SSE).

**Tests:**
- Adicionar/cobrir fluxo de scan e SSE nos testes frontend.

**Docs:**
- Nenhum.
