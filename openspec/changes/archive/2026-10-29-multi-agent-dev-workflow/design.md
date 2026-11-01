# Design

## Context

<<<<<<< HEAD
O projeto Plex Autoplay possui stack híbrida com backend em .NET 10 (C#) e frontend em Vue 3 (Vite + Pinia). O ambiente de desenvolvimento roda sobre Bazzite (sistema operacional imutável baseado em Fedora), onde comandos de Docker e build são executados.
=======
O projeto Plex Autoplay possui stack híbrida com backend em .NET 10 (C#) e frontend em Vue 3 (Vite + Pinia). Os comandos de Docker e build são executados diretamente na máquina do desenvolvedor.
>>>>>>> origin/main

O repositório já dispõe de scripts utilitários em `scripts/`:
- `setup-spec.py`: cria branch `feature/<change>-<span>` limpo a partir de `origin/main`.
- `code-review-skill.py`: avalia formatação, Clean Code, SOLID e OWASP.
- `commit-skill.py`: agrupa alterações por tarefa em commits semânticos com emojis.
- `docker-compose.test.yml` / `run-e2e-tests.sh`: orquestram suíte de testes E2E com mock do Plex.

A skill `/dev-spec` integrará essas capacidades em um protocolo automatizado multi-agente.

## Goals / Non-Goals

**Goals:**
- Coordenar a implementação de mudanças OpenSpec via três agentes (Backend, Frontend e Tester).
- Permitir execução concorrente de Backend e Frontend para maximizar velocidade de entrega sem sobreposição de arquivos.
- Garantir portão estrito de Code Review prévio com auto-correção de violações antes de submeter código aos testes.
<<<<<<< HEAD
- Executar testes unitários e testes E2E Playwright de forma confiável com o `.env` validado, exigindo 100% de sucesso.
=======
- Executar testes unitários e testes E2E Playwright de forma confiável na máquina do dev com o `.env` validado, exigindo 100% de sucesso.
>>>>>>> origin/main
- Gerar commits semânticos padronizados por tarefa e abrir o Pull Request via GitHub CLI (`gh`).

**Non-Goals:**
- Realizar o merge do Pull Request de forma autônoma (o merge é decisão e ação humana do desenvolvedor).
- Modificar arquitetura interna do app ou das suítes de teste existentes.

## Decisions

### 1. Modelo de Agentes (Orquestrador + 2 Subagentes)
- **Decisão**: O Agente Principal (Orquestrador) assume a implementação do Backend (.NET), dispara o Agente 2 (Frontend) como subagente em background via tool `subagent`, e após a conclusão de ambos, dispara o Agente 3 (Tester) como subagente dedicado.
- **Alternativa rejeitada**: Agente único sequencial (demora o dobro do tempo) ou múltiplos agentes sem orquestrador central (risco de perda de sincronização de estado no `tasks.md`).

### 2. Separação de Escopo de Arquivos
- **Decisão**: Divisão rígida de diretórios:
  - Agente Backend: `src/**`, `PlexAutoplay.slnx`
  - Agente Frontend: `vue/plex-frontend/**`
  - O contrato da API é derivado estritamente do `design.md` e `specs/` da change antes de iniciar o código.
- **Alternativa rejeitada**: Ambos os agentes editando os mesmos arquivos de documentação ou configuração simultaneamente.

### 3. Portão de Qualidade Pré-Teste (Self Code Review)
- **Decisão**: Cada agente roda lint e validação de sua stack:
  - Backend: `dotnet format PlexAutoplay.slnx` e regras Clean Code/SOLID.
  - Frontend: `npx eslint --fix` e `npx prettier --write`.
  - Quaisquer erros devem ser sanados antes do Agente 3 (Tester) ser acionado.

<<<<<<< HEAD
### 4. Execução de Testes
- **Decisão**: 
=======
### 4. Execução de Testes na Máquina do Dev
- **Decisão**: Os testes rodam diretamente na máquina do desenvolvedor:
>>>>>>> origin/main
  - Unitários: `dotnet test tests/unit/PlexAutoplay.UnitTests.csproj`
  - E2E: `docker compose -f docker-compose.test.yml up --build --abort-on-container-exit --exit-code-from executor`
- **Mitigação para .env**: O Agente Tester valida a presença das variáveis (`PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`, `MEDIA_ROOT`) antes do compose.

### 5. Finalização, Commits e PR
- **Decisão**: Utilizar `scripts/commit-skill.py` para criar commits lógicos agrupados por tarefa (sem `Co-authored-by`), fazer push para `origin` e abrir PR via `gh pr create`.
- **Alternativa rejeitada**: Commit único desestruturado ("squash commit") ou criação manual de PR.

## Risks / Trade-offs

- [Concorrência em tasks.md] → O Agente Orquestrador centraliza a marcação das caixas `[x]` no `tasks.md` ao consolidar o status de ambos os agentes.
- [Diferenças de porta/rede no E2E local] → O `docker-compose.test.yml` já encapsula a rede bridge `plex-test-network` e mock do Plex, garantindo determinismo sem depender do host.
- [Falhas intermitentes no Playwright] → O Agente 3 analisa o log/artefato, corrige o setup ou código e reexecuta a suíte até 100% verde.
