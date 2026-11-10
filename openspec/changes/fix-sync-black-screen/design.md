# Design — fix-sync-black-screen

## Architecture & Integration

- **Frontend (`SyncView.vue`)**:
  - Tratar exceções no parsing de mensagens SSE (`JSON.parse` dentro de try/catch).
  - Tratar robustamente o evento `onerror` do `EventSource` para evitar estado indefinido ou crash do componente.
  - Assegurar que o `apiFetch` gerencie erros 401 ou de rede sem quebrar o DOM, exibindo mensagem amigável ou estado de erro no componente.
- **Backend (`PlexEndpoints.cs`, `SyncEndpoints.cs`)**:
  - Garantir que o endpoint `/api/plex/scan` retorne JSON válido `{ "job_id": "..." }`.
  - Garantir que o stream SSE em `/api/sync/log/stream/{jobId}` envie chunks corretos em UTF-8 com Content-Type `text/event-stream`.

## UI/UX & Theming
- Utilizar variáveis e classes consistentes com o design system do projeto (`bg-white dark:bg-gray-800`, `text-body dark:text-bodydark`, etc. em vez de misturar `text-gray-900` sem suporte em alguns contextos).
- Garantir que o tema light/dark seja propagado corretamente e que o fundo da página não fique preto incorretamente.
