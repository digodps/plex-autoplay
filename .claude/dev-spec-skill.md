# Slash command `/dev-spec` — Claude Code

Skill de desenvolvimento multi-agente para OpenSpec (Backend .NET, Frontend Vue 3, Tester em Distrobox, commits semânticos e PR).

## Como usar no Claude Code
```
/dev-spec [<change-name>] [--dry-run]
```

## Comportamento

Ponto único de implementação: `scripts/dev-spec.py`.
1. Cria branch isolado via `scripts/setup-spec.py`.
2. Concorrência: Backend (.NET) e Frontend (Vue 3) desenvolvem em paralelo.
3. Portão de Code Review obrigatório com auto-correção.
4. Agente 3 (Tester) roda 100% dos testes unitários e E2E via Distrobox (Fedora).
5. Agrupa commits por task via `scripts/commit-skill.py` e abre PR via `gh pr create`.
