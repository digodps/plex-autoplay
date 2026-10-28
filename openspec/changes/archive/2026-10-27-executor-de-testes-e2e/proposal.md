# Proposal

## Why

Hoje não há um executor único que suba o container da aplicação com os dados do `.env`, rode os testes e2e (Playwright) contra ele, capture artefatos de falha (screenshots, traces, report e logs do container) e notifique o desenvolvedor localmente quando algo der errado. O desenvolvedor precisa fazer esse ciclo manualmente (`start.sh`, depois `playwright test`, depois `stop.sh`), o que é propenso a esquecer de derrubar o container e dificulta a análise de falhas.

## What Changes

- Novo script de executor de testes (`scripts/run-e2e-tests.sh` na raiz do projeto) que:
  - Valida existência e preenchimento do `.env` (variáveis obrigatórias: `PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`, `MEDIA_ROOT`)
  - Sube a aplicação via `docker compose -f docker-compose.test.yml up --build`
  - Aguarda a aplicação ficar disponível
  - Roda os testes e2e (Playwright)
  - Em caso de falha, captura screenshots, traces, `playwright-report/`, `test-results/` e `docker logs` da aplicação em `./test-artifacts/`
  - Sempre derruba o container ao final (`docker compose down`), mesmo se houver falha
  - Sai com código diferente de zero se algum grupo de testes falhar
- Novo `docker-compose.test.yml` com serviço `executor` (Node + Playwright + acesso ao socket do Docker) e serviço `plex-autoplay`, isolado do `docker-compose.yml` de dev
- Protótipo de GitHub Actions (`.github/workflows/test.yml`) que chama o mesmo compose de teste, injetando segredos via GitHub Secrets, com Plex mockado para CI
- Foco inicial apenas em testes e2e; testes unitários .NET ficam para uma mudança posterior

## Capabilities

### New Capabilities

- `test-executor`: Executor de testes que orquestra a validação do `.env`, subida do container da aplicação com dados do `.env`, execução dos testes e2e dentro do Docker, captura de artefatos de falha (screenshots, traces, report, logs) e notificação local com caminho dos artefatos. Suporta execução local e protótipo para GitHub Actions com Plex mockado.

### Modified Capabilities

- `e2e-integration`: Não há alteração nos requisitos dos testes e2e em si. O executor é um novo componente que orquestra a execução desses testes; não muda o comportamento especificado em `openspec/specs/e2e-integration/spec.md`.

## Impact

- Novos arquivos: `scripts/run-e2e-tests.sh`, `docker-compose.test.yml`, `.github/workflows/test.yml` (protótipo)
- Nenhum arquivo existente é modificado
- O executor depende do Docker Engine e Docker Compose disponíveis no host
- Na CI do Git, o Plex será mockado (não há Plex real disponível)
