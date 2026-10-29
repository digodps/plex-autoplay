# Design

## Context

- `GET /api/status` é mapeado em `src/PlexAutoplay.Web/Endpoints/MonitoringEndpoints.cs` e registrado em `Program.cs` via `app.MapMonitoringEndpoints()`.
- O handler chama `scanJobs.GetLatest()` (loja em memória `ConcurrentDictionary` em `ScanJobStore.cs`) e métricas de `System.Diagnostics.Process.GetCurrentProcess()`, retornando um objeto anônimo via `Results.Ok`.
- O endpoint fica atrás de `JwtAuthMiddleware` e `CorrelationIdMiddleware` no pipeline de `Program.cs`, então exige JWT válido (cookie `auth_token`), igual aos demais endpoints da API.
- `ScanJob.UpdatedAt` é `DateTime` não-nulo; `GetLatest()` ordena por `UpdatedAt` descendente e retorna `null` quando não há jobs.
- Não há `try/catch` no handler de `/api/status` — uma exceção não tratada escapa para o pipeline do Kestrel como HTTP 500.
- Frontend `fetchStatus()` (`vue/plex-frontend/src/api.js`) lança erro em resposta não-ok; a store do dashboard usa `Promise.allSettled` e degrada graciosamente (marca `partialErrors.status` e exibe "Erro ao carregar" no card).
- Causa raiz suspeita (a confirmar na implementação): exceção não tratada no handler ao acessar métricas de processo ou ordenar jobs.

Veja a motivação em `proposal.md - Why` e os requisitos em `specs/api-status/spec.md`.

## Goals / Non-Goals

**Goals:**
- Restaurar `GET /api/status` para retornar HTTP 200 com nome/versão do serviço, último job de sincronização e métricas de processo.
- Garantir que o endpoint nunca retorne HTTP 500 por falhas internas (degradação graciosa com dados parciais).
- Adicionar teste unitário do endpoint (`tests/unit/`).
- Manter o teste e2e existente de `/api/status` (`tests/e2e/tests/libraries.spec.js`) passando.

**Non-Goals:**
- Redesenhar o formato da resposta (manter campos atuais).
- Alterar requisitos de autenticação ou middleware.
- Adicionar novas dependências NuGet/npm.
- Persistir jobs de sync (a loja permanece em memória).

## Decisions

1. **Envolver o corpo do handler de `/api/status` em `try/catch`; na exceção, logar (nível Error, com correlation ID) e retornar HTTP 200 com os campos disponíveis** (service/version sempre presentes; `lastJob` = null; métricas de processo zeradas) em vez de 500.
   - Alternativa considerada: retornar 503 em falha. Rejeitada porque o dashboard já degrada graciosamente em resposta não-ok e o teste e2e espera 200; 200 com dados parciais mantém os clientes funcionando.
2. **Manter `ScanJobStore` como fonte da verdade em memória; nenhuma mudança de persistência nesta correção.**
3. **Confirmar a causa raiz localmente (reproduzir o 500 / inspecionar logs) antes de aplicar a correção**, em vez de chutar a linha que lança.

## Risks / Trade-offs

- [Risco] A linha exata que lança não foi confirmada por inspeção estática. → Mitigação: reproduzir localmente e inspecionar logs antes de implementar; adicionar log estruturado da exceção no `catch`.
- [Risco] Engolir exceções pode mascarar problemas reais (ex.: loja mal registrada no DI). → Mitigação: logar em nível Error com correlation ID; teste unitário asserta HTTP 200.
- [Trade-off] Retornar 200 com dados parciais em vez de 500 exige que clientes tolerem campos null/zerados. → Mitigação: o frontend já trata `lastJob` null; o spec exige métricas de processo ≥ 0.

## Migration Plan

- Implantar a alteração do .NET API em `MonitoringEndpoints.cs`. Não há migração de banco (loja em memória).
- Rollback: reverter a alteração em `MonitoringEndpoints.cs`; o endpoint volta ao comportamento anterior (possivelmente 500).
- Verificação: `GET /api/status` retorna HTTP 200; teste e2e de `/api/status` passa; card de serviço do dashboard exibe dados sem erro.

## Open Questions

Nenhuma — a causa raiz será confirmada durante a implementação. Se exigir alteração de spec ou de design, revisar antes de codificar.
