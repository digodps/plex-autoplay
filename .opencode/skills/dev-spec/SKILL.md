---
name: dev-spec
description: Orquestração de desenvolvimento multi-agente para OpenSpec (Backend .NET, Frontend Vue 3 e Tester em Distrobox com 100% de sucesso, commits semânticos e PR).
allowed-tools: Bash(*)
license: MIT
metadata:
  author: plex-autoplay
  version: "1.0"
---

# Multi-Agent Development Workflow (`/dev-spec`)

> 📖 **Guia de Desenvolvimento Obrigatório**: Antes de iniciar qualquer implementação, consulte o **[`DEVELOPMENT_GUIDE.md`](../../../DEVELOPMENT_GUIDE.md)**. Ele consolida todas as regras da skill de Code Review (`code-review-skill.py`) para evitar erros comuns.

Esta skill orquestra a implementação ponta a ponta de uma mudança OpenSpec, integrando os seguintes **agentes** e **skills de apoio**:

| Agente | Skill de Apoio |
|--------|----------------|
| Agente 1 (Backend) | `dev-spec.py`, `code-review-skill.py`, [`DEVELOPMENT_GUIDE.md`](../../../DEVELOPMENT_GUIDE.md) |
| Agente 2 (Frontend) | `dev-spec.py`, `code-review-skill.py`, [`DEVELOPMENT_GUIDE.md`](../../../DEVELOPMENT_GUIDE.md) |
| Agente 3 (Tester) | `dev-spec.py`, `run-e2e-tests.sh` |
| Human (Developer) | `commit-skill.py`, `code-review-skill.py --fix` |

A implementação ponta a ponta divide-se entre **três agentes especializados**:

1. **Agente 1 (Backend - Orquestrador Principal)**:
   - Configura o branch isolado `feature/<change>-<span>` via `scripts/setup-spec.py`.
   - Implementa endpoints, DTOs, regras de domínio e persistência em `src/**`.
   - Executa `dotnet format PlexAutoplay.slnx` e validação Clean Code / SOLID.
   - Corrige 100% das violações encontradas antes de avançar.
   - Spawna e monitora o Agente 2 e posteriormente o Agente 3.

2. **Agente 2 (Frontend - Subagente em Background)**:
   - Executa concorrentemente com o Agente 1.
   - Implementa componentes, views, composables e Pinia stores em `vue/plex-frontend/**`.
   - Executa `npx eslint --fix` e `npx prettier --write`.
   - Corrige 100% das violações de estilo/código antes de sinalizar conclusão.

3. **Agente 3 (Tester - Subagente Sequencial)**:
   - Acionado apenas após Backend e Frontend estarem 100% implementados e limpos no code review.
   - **Validação Automatizada**:
     - Valida variáveis obrigatórias do `.env` na raiz.
     - Roda testes unitários dentro do container Distrobox.
     - Sobe a stack de testes e executa E2E Playwright.
   - **Validação Manual (Sanity Check)**:
     - Após o sucesso dos testes automatizados, o Agente 3 deve orientar o desenvolvedor a abrir a aplicação no navegador dentro do Distrobox para validar visualmente o que foi solicitado.
   - Exige **100% de aprovação (0 falhas)** em ambos os modos. Se houver falha, diagnostica, orienta o reparo e retesta.

---

## Contrato de Divisão de Diretrios (Zero Conflito)

Para garantir execução paralela sem conflitos de git, todos os agentes devem seguir rigorrosamente o **[DEVELOPMENT_GUIDE.md](../../../DEVELOPMENT_GUIDE.md)** durante a implementacao:

- **Backend**: restrito a `src/**`, `tests/unit/**` e `PlexAutoplay.slnx`.
- **Frontend**: restrito a `vue/plex-frontend/**`.
- **Contrato de API**: derivado estritamente do `design.md` e `specs/` da change antes de escrever o codigo.
- **Tasks**: o Agente Orquestrador centraliza a atualizacao do `openspec/changes/<change>/tasks.md`.

---

## Finalização e Entrega Contínua

Ao atingir 100% dos testes aprovados, execute a fase de finalizacao:

1. **Auto-validação final**:
   - Rode `python scripts/code-review-skill.py` localmente para garantir zero achados.
   - Verifique o **[Checklist de Definition of Done](../../../DEVELOPMENT_GUIDE.md#5-checklist-de-definition-of-done-por-task)** do DEVELOPMENT_GUIDE.md.

2. Agrupa alterações em commits semânticos por tarefa usando `scripts/commit-skill.py` (sem `Co-authored-by`).
3. Faz push para o branch remoto.
4. Cria o Pull Request via GitHub CLI e devolve o link para validacao humana e merge pelo desenvolvedor.
