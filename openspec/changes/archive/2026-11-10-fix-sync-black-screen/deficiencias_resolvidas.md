# Deficiências resolvidas — fix-sync-black-screen

Goldfish Review (A/S/C) executado sobre requisitos/artefatos da change.

## A — Arquitetura, dependências e integração

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| A1 | `EventSource` no frontend faz `JSON.parse` sem `try/catch`; payload malformado derruba o componente e pode deixar a tela inerte/preta. | Aceito | Corrigir com try/catch no `onmessage` e estado de erro visível (RF3). |
| A2 | SSE no servidor roda `while` + `Task.Delay(1000)` sem limite de tempo nem detecção explícita de cliente desconectado além de `RequestAborted`. | Aceito (parcial) | Manter comportamento; garantir `onerror` fecha o `EventSource` no cliente e exibe feedback. |
| A3 | `redirectToLoginPreservingRoute` em `apiFetch` faz `window.location.href` (efeito colateral forte, não testável e pode descarregar a shell ao 401). | Aceito | Tratar 401 sem redirecionar silenciosamente quando já houver erro; manter UI funcional (RF4). |

## S — SOLID e coesão

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| S1 | `SyncView.vue` concentra lógica de scan, SSE, parsing e UI (baixa coesão). | Aceito | Refatorar mínimamente: isolar conexão SSE e tratamento de erro; não criar abstrações excessivas (ponytail). |
| S2 | Endpoint `/api/plex/scan` usa `Task.Run` misturando request com background job; acoplado a repositórios injetados. | Aceito (parcial) | Manter; garantir resposta imediata `{ job_id }` e logs via SSE. |

## C — Clean Code, escopo e testabilidade

| # | Achado | Classificação | Motivo / Ação |
|---|--------|---------------|---------------|
| C1 | `SyncView.vue` mistura variáveis de estado, callbacks SSE e template; difícil de testar unitariamente. | Aceito | Extrair lógica de conexão SSE em função separada dentro do componente; manter escopo da change (sem redesign). |
| C2 | Variáveis de tema (CSS classes `dark:`) inconsistentes entre componentes (alguns usam `text-gray-900` sem par dark). | Aceito | Padronizar classes da tela de sincronização com variáveis do design system (RF5). |

**Nenhum achado altera requisitos do spec.** Os achados aceitos são atendidos pelas tarefas em `tasks.md`. Registro em `deficiencias_resolvidas.md` concluído.
