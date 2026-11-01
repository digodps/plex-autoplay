# Slash command `/dev-spec` — OpenCode

Skill de desenvolvimento multi-agente para OpenSpec (Backend .NET, Frontend Vue 3, Tester, commits semânticos e PR).

> 📖 **Guia de Desenvolvimento Obrigatório**: Consulte o **[DEVELOPMENT_GUIDE.md](../../DEVELOPMENT_GUIDE.md)** antes de iniciar - ele contém todas as regras baseadas no Code Review para evitar retrabalho.

## Como usar no OpenCode
```
/dev-spec [<change-name>] [--dry-run]
```

## Comportamento

Ponto único de implementação: `scripts/dev-spec.py`.
1. Cria branch isolado (via `/dev-spec`).
2. Concorrência: Backend (.NET) e Frontend (Vue 3) desenvolvem em paralelo.
3. Portão de Code Review obrigatório com auto-correção.
4. Agente 3 (Tester) roda 100% dos testes unitários e E2E.
5. Agrupa commits por task via `scripts/commit-skill.py` e abre PR via `gh pr create`.
