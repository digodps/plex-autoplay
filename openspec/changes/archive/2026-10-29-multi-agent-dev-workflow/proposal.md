# Proposal

## Why

Atualmente, o ciclo entre planejar uma mudança no OpenSpec e sua implementação completa exige coordenação manual: criação de branch isolado, divisão manual de tarefas entre backend (.NET) e frontend (Vue 3), validação de lint e padrões Clean Code/SOLID, subida de containers para execução de testes unitários e E2E Playwright, organização de múltiplos commits e criação de Pull Request.

Esta proposta cria uma skill e fluxo automatizado (`/dev-spec`), orquestrando três agentes em paralelo e sequencial com portões estritos de qualidade para entregar a mudança pronta em um Pull Request para revisão final do desenvolvedor.

## What Changes

- Criação do comando e skill `/dev-spec` (`.agents/skills/dev-spec/SKILL.md` e `.agents/workflows/dev-spec.md`).
- Orquestração de 3 agentes especializados:
  - **Agente 1 (Backend - Orquestrador)**: Setup de branch isolado via `scripts/setup-spec.py`, implementação de tarefas C#/.NET 10, execução de code review (.NET) e correção de pendências.
  - **Agente 2 (Frontend - Subagente)**: Execução paralela em background para implementar tarefas Vue 3 / Pinia / CSS, execução de code review (ESLint/Prettier) e auto-correção de estilo.
  - **Agente 3 (Tester - Subagente)**: Execução sequencial após 100% do código implementado e limpo. Validação de `.env`, execução de testes unitários e testes E2E Playwright dentro do container via Distrobox (Fedora), garantindo 100% de sucesso.
- Integração de entrega contínua:
  - Atualização do `tasks.md` marcando tarefas concluídas.
  - Geração de commits convencionais mapeados por task via `scripts/commit-skill.py` (sem `Co-authored-by`).
  - Push para o remote origin e criação automatizada de Pull Request com `gh pr create` para validação humana e merge.

## Capabilities

### New Capabilities
- `multi-agent-dev`: Orquestração multi-agente para implementação paralela de mudanças OpenSpec, portões de code-review com auto-correção, validação 100% de testes em container Distrobox e publicação automatizada de PR.

### Modified Capabilities
<!-- Nenhuma capacidade existente tem seus requisitos funcionais alterados -->

## Impact

- Novos arquivos em `.agents/skills/dev-spec/SKILL.md` e `.agents/workflows/dev-spec.md`.
- Uso integrado dos scripts existentes em `scripts/` (`setup-spec.py`, `code-review-skill.py`, `commit-skill.py`, `run-e2e-tests.sh`).
- Dependência do GitHub CLI (`gh`) para a etapa final de abertura de Pull Request.
