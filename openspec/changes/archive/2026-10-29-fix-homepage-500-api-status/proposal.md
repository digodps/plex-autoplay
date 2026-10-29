# Proposal

## Why

A página inicial (dashboard `/dashboard`) consulta `GET /api/status` ao montar e a cada 30s via polling para exibir a saúde do serviço, o último job de sincronização e as métricas de processo no card de status. Esse endpoint está retornando HTTP 500 Internal Server Error, o que impede o dashboard de mostrar a saúde do serviço e gera erro nos cards da página inicial. É preciso restaurar o funcionamento confiável do endpoint de status para que o dashboard exiba as informações de saúde sem falha.

## What Changes

- Investigar a causa raiz do HTTP 500 em `GET /api/status` (`MonitoringEndpoints`, `ScanJobStore` e métricas de processo).
- Corrigir o endpoint para retornar HTTP 200 com informações do serviço, último job de sync e métricas de processo.
- Adicionar tratamento de exceções no endpoint para que nunca retorne 500 por falhas transitórias (degradação graciosa).
- Adicionar teste unitário para `GET /api/status` (status 200, formato da resposta, comportamento quando não há jobs).
- Verificar se o teste e2e existente de `GET /api/status` (`tests/e2e/tests/libraries.spec.js`) passa após a correção.

## Capabilities

### New Capabilities

- `api-status`: Contrato do endpoint de saúde do serviço `GET /api/status`. Define que o endpoint deve retornar HTTP 200 com dados do serviço (nome, versão), último job de sincronização e métricas de processo, e deve tratar erros internos sem expor 500 ao cliente.

### Modified Capabilities

- Nenhuma — não há mudança de requisito de especificação existente; trata-se de correção de comportamento falho do endpoint já especificado por `telemetry` (métricas de processo).

## Impact

- Código afetado: `src/PlexAutoplay.Web/Endpoints/MonitoringEndpoints.cs` (endpoint `/api/status`), `src/PlexAutoplay.Web/Services/ScanJobStore.cs` (leitura do último job).
- Frontend: `vue/plex-frontend/src/api.js` (`fetchStatus`), `vue/plex-frontend/src/stores/dashboard.js`, `vue/plex-frontend/src/views/DashboardView.vue` (consumem o endpoint; nenhuma mudança obrigatória, pois já tratam erro parcial).
- Testes: `tests/unit/` (novo teste unitário do endpoint), `tests/e2e/tests/libraries.spec.js` (teste e2e existente de `/api/status`).
- Especs: novo spec `specs/api-status/spec.md`.
