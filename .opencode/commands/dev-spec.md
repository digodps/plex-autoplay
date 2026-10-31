---
description: "Executa a skill de desenvolvimento multi-agente (Backend .NET, Frontend Vue 3 e Tester em Distrobox com 100% testes e PR)"
---

Executa a skill `/dev-spec` para implementar e testar uma mudança OpenSpec com três agentes coordenados.

> 📖 **Guia Obrigatório**: Antes de qualquer implementação, consulte o **[DEVELOPMENT_GUIDE.md](../../DEVELOPMENT_GUIDE.md)** que consolida todas as regras da Code Review para evitar erros comuns.

Uso:
```
/dev-spec [<change-name>] [--dry-run]
```

Exemplos:
```
/dev-spec                                  # usa a mudança em andamento
/dev-spec infinite-scroll-series           # usa a mudança especificada
/dev-spec --dry-run                        # simula os passos sem alterar código ou git
```

O comando orquestra:
1. **Setup**: isolamento de branch `feature/<change>-<span>` via `scripts/setup-spec.py`.
2. **Desenvolvimento Paralelo**:
   - **Agente 1 (Backend)**: implementa C#/.NET em `src/**` e roda code-review (.NET) com auto-correção.
   - **Agente 2 (Frontend)**: implementa Vue 3 em `vue/plex-frontend/**` e roda code-review (ESLint/Prettier) com auto-correção.
3. **Consolidação**: marca tarefas como `[x]` no `tasks.md`.
4. **Agente 3 (Tester)**: roda testes unitários e E2E Playwright dentro do container via Distrobox (Fedora) exigindo 100% de sucesso.
5. **Entrega**: commita via `scripts/commit-skill.py`, faz push para origin e abre Pull Request no GitHub via `gh pr create`.
