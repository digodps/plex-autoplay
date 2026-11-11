# Deficiências resolvidas — catalog-sync-popular-series

Goldfish Review (A/S/C) executado sobre requisitos/artefatos da change.
Worktree: `/home/hermes/plex-autoplay-catalog-sync` (branch `feature/spec-catalog-sync-popular-series`).

## A — Arquitetura, dependências e integração

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| A1 | `design.md` diz `AddSingleton`, mas `ServiceCollectionExtensions.cs:85` usa `AddTransient`. | Aceito | `AddTransient` correto (evita captive dependency de `IPlexApiClient` transient). Ação: corrigir `design.md`. |
| A2 | Endpoint chama `RunAsync(CancellationToken.None)`, ignora `HttpContext.RequestAborted`. | Aceito | Ação: passar `context.RequestAborted` (requisição longa cancelável). |
| A3 | Endpoint mapeia falha do Plex para 502 e sucesso para 200 com contagens, conforme spec. | Aceito | Nenhuma alteração. Implementação atende ao spec delta. |

## S — SOLID e coesão

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| S1 | `POST /api/sync/catalog` em `SyncEndpoints.cs`. | Rejeitado | `SyncEndpoints` é a fronteira de endpoints de sincronização (`/api/sync/*`); catálogo é um estágio do ciclo de sync. Coesão mantida; sem nova classe. |
| S2 | `CatalogSyncUseCase` registrado como Transient. | Aceito | Alinha com `IPlexApiClient` transient; sem captive dependency. |
| S3 | `RunAsync` usa `ct.ThrowIfCancellationRequested()` em cada iteração. | Aceito | Cancelamento cooperativo correto, agora alimentado por `RequestAborted`. |
| S4 | `CatalogSyncUseCase.cs:55-81` tem branches idênticos (só diferem contadores). | Rejeitado | Fora de escopo: `design.md` Non-Goal "não alterar `CatalogSyncUseCase`". Backlog. |

## C — Clean Code, escopo e testabilidade

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| C1 | `design.md` afirma "use case completo e testado". | Aceito | Ação: ajustar afirmação; cobertura real é via QA E2E. |
| C2 | Sem teste automatizado de DI/endpoint. | Parcial | Coberto por QA E2E Playwright que chama `POST /api/sync/catalog` e `GET /api/series/{key}`. Teste unitário de DI exigiria `WebApplicationBuilder` (custo de infra desproporcional). Backlog. |
| C3 | Contrato do endpoint em camelCase; irmãos usam snake_case. | Rejeitado | Spec não impõe casing; nenhum consumidor no frontend. Sem churn. |
| C4 | Endpoint não faz try/catch em `RunAsync`. | Parcial | `RunAsync` retorna `CatalogSyncResult.Fail` para falha do Plex (502). Exceção inesperada → 500 é aceitável (erro de servidor). Backlog. |

**Achados que alteraram artefato:** A1/C1 (`design.md`). **Achados que alteraram código:** A2/C2 (token de cancelamento em `SyncEndpoints.cs`).
