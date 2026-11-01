---
description: "Workflow de desenvolvimento multi-agente com OpenSpec (Backend, Frontend, Tester, Code Review e PR)"
---

Workflow de desenvolvimento multi-agente integrado ao OpenSpec.

Uso:
```
/dev-spec [<change-name>]
```

## Etapas do Workflow

1. **Seleção e Isolamento da Mudança**:
   - Identifica a mudança ativa via `openspec list --json` ou usa o argumento fornecido.
   - O branch `feature/<change>-<span>` é criado automaticamente pelo `/dev-spec`.

2. **Divisão de Tarefas**:
   - Lê `openspec/changes/<change>/tasks.md`.
   - Separa as tarefas por domínio:
     - Tarefas de backend (.NET APIs, Domain, Infraestrutura, Dapper/SQLite).
     - Tarefas de frontend (Vue 3 views, components, stores Pinia).

3. **Execução Paralela**:
   - O **Agente 1 (Backend)** implementa os arquivos em `src/**`.
   - Em paralelo, o **Agente 2 (Frontend)** é spawnado via subagente em background e implementa os arquivos em `vue/plex-frontend/**`.

4. **Portão de Code Review (Auto-correção)**:
   - Backend roda `dotnet format PlexAutoplay.slnx` e validação Clean Code / SOLID.
   - Frontend roda `npx eslint --fix` e `npx prettier --write`.
   - Ambos resolvem todas as pendências antes de avançar.

5. **Consolidação de Tasks**:
   - Antes de concluir, garanta que o código segue o [`DEVELOPMENT_GUIDE.md`](../../../DEVELOPMENT_GUIDE.md).
   - Atualiza `tasks.md` marcando as tarefas concluídas `[x]`.

6. **Agente 3 (Tester em container)**:
