# Deficiências Resolvidas — EGM Goldfish Review (A/S/C)

Change: `icone-marca-discreto`
Data: 2026-11-08

## A — Arquitetura, Dependências, Integração

1. **A-1** Nenhuma dependência nova introduzida. ✅ **Aceito** — apenas edita SVG inline existente.
2. **A-2** Contratos de API, rotas, stores inalterados. ✅ **Aceito**.
3. **A-3** `favicon.svg` atualizado mantém mesmo caminho `/favicon.svg`, sem quebrar `<link rel="icon">` em `index.html`. ✅ **Aceito**.

## S — SOLID, Coesão

1. **S-1** Responsabilidade única mantida: `AppLogo.vue` só renderiza a marca; favicon é asset estático. ✅ **Aceito**.
2. **S-2** Não há lógica duplicada: marca definida em 2 arquivos (componente + asset), mas são contextos diferentes (runtime vs browser tab). Necessidade de sincronizar desenho é risco conhecido. **Parcial** — mitigado por especificação compartilhada no design.md. Mantido.

## C — Clean Code, Escopo, Testabilidade

1. **C-1** Escopo minimalista: 2 arquivos, sem lógica. ✅ **Aceito**.
2. **C-2** `currentColor` elimina magic numbers de cor e adapta tema automaticamente. ✅ **Aceito**.
3. **C-3** Tarefas verificáveis via `npm run build` + inspeção visual (Playwright). ✅ **Aceito**.
4. **C-4** Favicon usa cinza fixo `#8B949E` — hardcoded color em asset estático é padrão da web. ✅ **Aceito**.

---

**Resumo**: Todos os achados **aceitos**. Nenhum exige mudança de requisito. Artefatos prontos para commit + worktree.