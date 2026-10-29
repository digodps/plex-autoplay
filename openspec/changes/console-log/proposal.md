# Proposal

## Why

A página de sincronização (`/sync`) recebe o progresso da varredura via SSE, mas só exibe três contadores (processados, pulados, erros). O usuário não consegue ver o que foi feito, nem o que fazer quando algo dá errado. É necessário um console de log legível, com linhas por evento, ícones que ajudam a interpretar a gravidade, e, em caso de erro, uma sugestão de ação para o usuário corrigir o problema. O console deve seguir o tema claro/escuro do aplicativo.

## What Changes

- Backend: estender o stream SSE `/api/sync/log/stream/{jobId}` para enviar linhas de log detalhadas (timestamp, mensagem, nível) além dos contadores, e gravar essas linhas durante a varredura (início, bibliotecas, episódios processados/pulados, erros com sugestão de correção).
- Frontend: reescrever `SyncView.vue` como um painel de console de log com linhas, ícones por nível (info, sucesso, pular, aviso, erro), timestamps, sugestão de ação em erros, auto-scroll, fundo translúcido que segue o tema (claro/escuro/sistema) e botão "Limpar console" para esconder as linhas da exibição (apenas no frontend; o histórico do job no backend é mantido).
- Estilo: adicionar variáveis/classes do painel de log em `style.css` (fundo vidro/translúcido, fonte monospace, borda sutil).

## Capabilities

### Modified Capabilities

- `plex-sync`: o comportamento de reportagem de progresso da varredura é estendido para incluir linhas de log detalhadas no stream SSE (timestamp, mensagem, nível, sugestão de correção em erro), além dos contadores existentes.

### New Capabilities

- (nenhum)

## Impact

- Backend: `PlexAutoplay.Domain.Ports.IScanJobStore`, `PlexAutoplay.Web.Services.ScanJobStore`, `PlexAutoplay.Web.Endpoints.SyncEndpoints`, `PlexAutoplay.Web.Endpoints.PlexEndpoints`, `PlexAutoplay.Infrastructure.Sync.SyncBackgroundService`.
- Frontend: `vue/plex-frontend/src/views/SyncView.vue`, `vue/plex-frontend/src/style.css`.
- APIs: `GET /api/sync/log/stream/{jobId}` (contrato SSE estendido com linhas de log).
