# Design

## Context

O projeto já possui `docker-compose.yml` (serviço `plex-autoplay`, .NET 10, portas 443/8081, variáveis injetadas do `.env`), `start.sh`/`stop.sh` (valida `.env`, sobe/desce o container), e testes e2e Playwright em `tests/e2e/` (pressupõem container já rodando). Não há executor único que encapsule esse ciclo inteiro dentro do Docker, nem captura de artefatos de falha nem notificação local. Veja `proposal.md` para a motivação completa.

## Goals / Non-Goals

**Goals:**
- Executor rodando dentro de um container Docker com acesso ao socket do Docker (`/var/run/docker.sock`) para subir a aplicação
- Compose separado (`docker-compose.test.yml`), isolado do compose de dev
- Validação de `.env` antes de qualquer execução
- Captura de screenshots, traces, `playwright-report/`, `test-results/` e `docker logs` em caso de falha
- `docker compose down` sempre executado no final (mesmo em falha)
- Exit code diferente de zero se qualquer grupo de testes falhar
- Execução local primeiro, com protótipo para GitHub Actions
- Plex mockado na CI do Git

**Non-Goals:**
- Testes unitários .NET (ficam para uma mudança posterior)
- Notificação externa (Slack, Discord, GitHub Issue) — apenas notificação local (caminho no terminal)
- Plex real na CI (será mockado)

## Decisions

### Executor como serviço Docker com socket montado
O executor é um container próprio (Node + Playwright + Docker CLI) que monta `/var/run/docker.sock` para rodar `docker compose -f docker-compose.yml up --build` da aplicação. Alternativa considerada: executar o script no host e apenas subir a aplicação no Docker. Rejeitada porque o requisito é "tudo dentro do Docker".

### Compose de teste separado (`docker-compose.test.yml`)
Evita colidir com volumes, portas e estado do compose de dev (`docker-compose.yml`). Permite rodar executor e aplicação isoladamente.

### Captura de artefatos em volume montado no host
A pasta `./test-artifacts/` é montada como volume no container do executor para que o desenvolvedor acesse os artefatos localmente após a falha.

### Plex mockado na CI
Na GitHub Actions, um container simples responde às rotas consumidas pelos testes e2e, evitando a necessidade de um Plex Media Server real.

## Risks / Trade-offs

[Docker-in-Docker via socket] → O container do executor precisa de privilégios para montar `/var/run/docker.sock`. Em ambientes onde o socket não está disponível (ex. alguns runners de CI restritos), o executor não consegue subir a aplicação. Mitigação: documentar pré-requisito de Docker Engine no host.

[Plex mockado na CI] → O mock pode não reproduzir todos os comportamentos do Plex real. Mitigação: manter os e2e contra Plex real como execução local primária; o mock serve como protótipo de CI.

[Volumes persistentes] → O container da aplicação usa o volume `plex-data`. Re-execuções podem reusar dados antigos. Mitigação: o `docker compose down` libera o container, mas o volume persiste; usar `--remove-volumes` quando necessário.

[.env com credenciais sensíveis] → O `.env` contém `PLEX_TOKEN` e `JWT_SECRET`. Não deve ser commitado. O executor deve ler apenas do arquivo local existente.
