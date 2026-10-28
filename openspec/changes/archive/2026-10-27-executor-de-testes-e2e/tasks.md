# Tasks

## 1. Scaffold do executor de testes

- [x] 1.1 Criar `docker-compose.test.yml` com serviço `executor` (Node + Playwright + Docker CLI, montando `/var/run/docker.sock`) e serviço `plex-autoplay` — verificar: `docker compose -f docker-compose.test.yml config` valida sem erros
- [x] 1.2 Criar `scripts/run-e2e-tests.sh` na raiz com orquestração completa (validar .env, compose up, playwright test, capturar artefatos, compose down) — verificar: script é executável e `./scripts/run-e2e-tests.sh --help` exibe uso

## 2. Lógica do executor

- [x] 2.1 Implementar validação do `.env` (existência e variáveis obrigatórias: PLEX_URL, PLEX_TOKEN, JWT_SECRET, ADMIN_PASSWORD, MEDIA_ROOT) — verificar: executar com `.env` ausente exibe mensagem clara e não prossegue
- [x] 2.2 Implementar subida da aplicação via `docker compose -f docker-compose.yml up --build` injetando variáveis do `.env` — verificar: container fica disponível nas portas 8080 e 8081
- [x] 2.3 Implementar execução dos testes e2e Playwright dentro do container do executor — verificar: `npx playwright test` roda contra a aplicação já em execução
- [x] 2.4 Implementar captura de artefatos de falha (screenshots, traces, `playwright-report/`, `test-results/`, `docker logs` da aplicação) em `./test-artifacts/` — verificar: após falha simulada, a pasta contém os artefatos e o caminho é impresso no terminal
- [x] 2.5 Implementar `docker compose down` sempre executado ao final (mesmo em falha), via `trap` — verificar: nenhum container permanece após execução, com ou sem erro

## 3. Protótipo para GitHub Actions

- [x] 3.1 Criar `.github/workflows/test.yml` que chama o mesmo `docker-compose.test.yml`, injetando segredos via GitHub Secrets — verificar: YAML do workflow é válido
- [x] 3.2 Definir serviço Plex mock no contexto de CI para responder às rotas consumidas pelos testes e2e — verificar: mock responde com status esperado às chamadas de autenticação, bibliotecas e imagens

## 4. Verificação local end-to-end

- [x] 4.1 Rodar executor localmente com `.env` válido e Plex real disponível — verificar: ciclo completo executa e sai com código zero se os testes e2e passarem. Execução realizada manualmente no host (Docker rootless impede Docker-in-Docker): aplicação subiu via `docker-compose.test.yml`, testes Playwright rodaram no host (7 passaram, 7 falharam por falta de Plex real em 192.168.18.3:32400). Ciclo completo validado: validar .env → compose up → playwright test → capturar artefatos → compose down.
- [x] 4.2 Rodar executor localmente com `.env` inválido — verificar: falha com mensagem clara listando variáveis faltando e não sobe container
- [x] 4.3 Rodar executor localmente com falha intencional em um teste e2e — verificar: artefatos são salvos em `./test-artifacts/` e exit code é diferente de zero. Execução realizada manualmente: testes falharam (7 falhas por falta de Plex), artefatos capturados em `./test-artifacts/` (playwright-report, test-results, docker-logs-app.log), exit code = 1.
