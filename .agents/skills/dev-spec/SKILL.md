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

Esta skill orquestra a implementação ponta a ponta de uma mudança OpenSpec dividindo o trabalho entre **três agentes especializados**:

1. **Agente 1 (Backend - Orquestrador Principal)**:
   - Configura o branch isolado `feature/<change>-<span>` via `scripts/setup-spec.py`.
   - Implementa endpoints, DTOs, regras de domínio e persistência em `src/**`.
   - Executa `dotnet format PlexAutoplay.slnx` e validação Clean Code / SOLID.
   - Corrige 100% das violações encontradas antes de avançar.
   - Spawna e monitora o Agente 2 e posteriormente o Agente 3.

2. **Agente 2 (Frontend + UX - Subagente em Background)**:
   - Atua também como especialista de UX.
   - Pesquisa padrões atuais em produtos equivalentes e documentação oficial antes de codar; registra decisões de fluxo, hierarquia visual, estados vazio/carregando/erro e critérios de usabilidade no `design.md` da change.
   - Planeja experiência moderna, minimalista e distinta, com foco na tarefa principal, poucos passos, navegação previsível e feedback imediato.
   - Valida responsividade, teclado, foco visível, contraste, semântica HTML e mensagens acessíveis.
   - Executa concorrentemente com o Agente 1.
   - Implementa componentes, views, composables e Pinia stores em `vue/plex-frontend/**`.
   - Executa `npx eslint --fix` e `npx prettier --write`.
   - Corrige 100% das violações de UX, acessibilidade, estilo e código antes de sinalizar conclusão.

3. **Agente 3 (Tester - Subagente Sequencial)**:
   - Acionado apenas após Backend e Frontend estarem 100% implementados e limpos no code review.
   - Valida variáveis obrigatórias do `.env` na raiz (`PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`, `MEDIA_ROOT`).
   - Roda testes unitários dentro do container Distrobox:
     ```bash
     distrobox enter fedora -- dotnet test tests/unit/PlexAutoplay.UnitTests.csproj
     ```
   - Sobe a stack de testes e executa E2E Playwright:
     ```bash
     distrobox enter fedora -- docker compose -f docker-compose.test.yml up --build --abort-on-container-exit --exit-code-from executor
     ```
   - Derruba a stack de testes:
     ```bash
     distrobox enter fedora -- docker compose -f docker-compose.test.yml down -v
     ```
   - Exige **100% de aprovação (0 falhas)**. Se houver falha, diagnostica, orienta o reparo e retesta.

---

## Contrato de Divisão de Diretórios (Zero Conflito)

Para garantir execução paralela sem conflitos de git:
- **Backend**: restrito a `src/**`, `tests/unit/**` e `PlexAutoplay.slnx`.
- **Frontend**: restrito a `vue/plex-frontend/**`.
- **Contrato de API**: derivado estritamente do `design.md` e `specs/` da change antes de escrever o código.
- **Tasks**: o Agente Orquestrador centraliza a atualização do `openspec/changes/<change>/tasks.md`.

---

## Finalização e Entrega Contínua

Ao atingir 100% dos testes aprovados:
1. Agrupa alterações em commits semânticos por tarefa usando `scripts/commit-skill.py` (sem `Co-authored-by`).
2. Faz push para o branch remoto:
   ```bash
   git push origin feature/<change>-<span>
   ```
3. Cria o Pull Request via GitHub CLI:
   ```bash
   gh pr create --title "feat(<change>): <título>" --body "<resumo>" --base main --head "feature/<change>-<span>"
   ```
4. Exibe a URL do PR para validação humana e merge pelo desenvolvedor.
