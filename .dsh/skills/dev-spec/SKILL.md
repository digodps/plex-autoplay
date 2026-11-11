---
name: dev-spec
description: Orquestração de desenvolvimento multi-agente para OpenSpec (Backend .NET, Frontend Vue 3 e Tester com 100% de sucesso, commits semânticos e PR).
allowed-tools: Bash(*)
license: MIT
metadata:
  author: plex-autoplay
  version: "1.0"
---

# Multi-Agent Development Workflow (`/dev-spec`)

> 📖 **Guia de Desenvolvimento Obrigatório**: Antes de iniciar qualquer implementação, consulte o **[`DEVELOPMENT_GUIDE.md`](../../../DEVELOPMENT_GUIDE.md)**. Ele consolida todas as regras da skill de Code Review (`code-review-skill.py`) para evitar erros comuns.

## 🚀 MCP Development Server (`plex-autoplay-mcp`)

Antes de qualquer implementação, o Agente Orquestrador (e os demais agentes) deve usar o MCP local em `mcp-server/server.py`:

| Tool / Resource | Uso |
|-----------------|-----|
| `get_best_practices` / resource `plex://guidelines` | Boas práticas e Definition of Done, sem reler o guia inteiro |
| `get_code_map` / resource `plex://codemap` | Mapa backend/frontend/tests/openspec para localizar arquivos |
| `validate_commit_msg` | Valida formato Conventional Commit com emoji antes do commit |

**Início obrigatório (idempotente)** — execute antes de spawnar os agentes:

```bash
pgrep -f "mcp-server/server.py" >/dev/null || nohup python3 mcp-server/server.py >/tmp/plex-autoplay-mcp.log 2>&1 &
```

**Validação**: `pgrep -f "mcp-server/server.py"` deve retornar um PID. Se não retornar, abortar e reportar falha ao usuário.

Esta skill orquestra a implementação ponta a ponta de uma mudança OpenSpec, integrando os seguintes **agentes** e **skills de apoio**:

| Agente | Skill de Apoio |
|--------|----------------|
| Agente 1 (Backend) | `dev-spec.py`, `code-review-skill.py`, MCP `plex-autoplay-mcp`, [`DEVELOPMENT_GUIDE.md`](../../../DEVELOPMENT_GUIDE.md) |
| Agente 2 (Frontend + UX) | `dev-spec.py`, `code-review-skill.py`, MCP `plex-autoplay-mcp`, pesquisa de UX, [`DEVELOPMENT_GUIDE.md`](../../../DEVELOPMENT_GUIDE.md) |
| Agente 3 (Tester) | `dev-spec.py`, `run-e2e-tests.sh`, Validação com Navegador Real |
| Human (Developer) | `commit-skill.py`, `code-review-skill.py --fix` |

A implementação ponta a ponta divide-se entre **três agentes especializados**:

1. **Agente 1 (Backend - Orquestrador Principal)**:
   - Garante o MCP rodando (comando idempotente acima) antes de spawnar agentes.
   - Configura o branch isolado `feature/<change>-<span>` (internal ao `/dev-spec`).
   - Implementa endpoints, DTOs, regras de domínio e persistência em `src/**`.
   - Consulta `get_code_map` antes de criar arquivos novos para respeitar a arquitetura existente.
   - Executa `dotnet format PlexAutoplay.slnx` e validação Clean Code / SOLID.
   - Corrige 100% das violações encontradas antes de avançar.
   - Spawna e monitora o Agente 2 e posteriormente o Agente 3.

2. **Agente 2 (Frontend + UX - Subagente em Background)**:
   - Atua também como especialista de UX.
   - Consulta `get_best_practices` (`plex://guidelines`) antes de codar e `get_code_map` para localizar views/stores existentes.
   - Pesquisa padrões atuais em produtos equivalentes e documentação oficial antes de codar; registra decisões de fluxo, hierarquia visual, estados vazio/carregando/erro e critérios de usabilidade no `design.md` da change.
   - Planeja experiência moderna, minimalista e distinta, com foco na tarefa principal, poucos passos, navegação previsível e feedback imediato.
   - Valida responsividade, teclado, foco visível, contraste, semântica HTML e mensagens acessíveis.
   - Executa concorrentemente com o Agente 1.
   - Implementa componentes, views, composables e Pinia stores em `vue/plex-frontend/**`.
   - Executa `npx eslint --fix` e `npx prettier --write`.
   - Corrige 100% das violações de UX, acessibilidade, estilo e código antes de sinalizar conclusão.

3. **Agente 3 (Tester - Subagente Sequencial)**:
   - Acionado apenas após Backend e Frontend estarem 100% implementados e limpos no code review.
   - **Validação Automatizada**:
     - Valida variáveis obrigatórias do `.env` na raiz.
     - Roda testes unitários dentro do container.
     - Sobe a stack de testes e executa E2E Playwright.
   - **Validação Manual (Sanity Check)**:
     - Após o sucesso dos testes automatizados, o Agente 3 deve orientar o desenvolvedor a abrir a aplicação no navegador para validar visualmente o que foi solicitado.
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

2. Agrupa alterações em commits semânticos por tarefa usando `scripts/commit-skill.py` (sem `Co-authored-by`). Valide cada mensagem com a tool `validate_commit_msg` do MCP antes de commitar.
3. Faz push para o branch remoto.
4. Cria o Pull Request via GitHub CLI e devolve o link para validacao humana e merge pelo desenvolvedor.
5. Encerra o MCP se ele foi iniciado por esta execução: `pkill -f "mcp-server/server.py"` (apenas quando não estava rodando antes).
