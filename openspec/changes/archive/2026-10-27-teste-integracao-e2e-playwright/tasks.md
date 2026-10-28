# Tasks

## 1. Infraestrutura de testes e2e

- [x] 1.1 Criar `.env.example` com variáveis obrigatórias (PLEX_URL, PLEX_TOKEN, MEDIA_ROOT, JWT_SECRET, ADMIN_PASSWORD, SYNC_INTERVAL_MINUTES, CUSTOM_JSON_PATH, WEB_PORT, WS_PORT, ASPNETCORE_ENVIRONMENT) e verificar que o arquivo existe na raiz do projeto
- [x] 1.2 Criar scripts `start.sh`, `stop.sh`, `logs.sh` para gerenciar o container Docker e verificar que cada um é executável (`chmod +x`) e roda sem erro de sintaxe (`bash -n`)
- [x] 1.3 Adicionar Playwright ao projeto (npm init/playwright install) e verificar que `npx playwright --version` retorna a versão instalada
- [x] 1.4 Adicionar `.env` ao `.gitignore` para evitar commit de credenciais e verificar que `git check-ignore .env` retorna o arquivo

## 2. Configuração dos testes Playwright

- [x] 2.1 Criar `playwright.config.ts` (ou .js) com baseURL apontando para `http://localhost:8080` e timeout global de 5 minutos e verificar que `npx playwright show-config` não retorna erro
- [x] 2.2 Criar utilitário de leitura do `.env` nos testes (carrega variáveis para process.env) e verificar que as variáveis estão acessíveis nos testes
- [x] 2.3 Criar script de espera (health check) que aguarda a porta 8080 responder antes de iniciar os testes e verificar que o script retorna sucesso quando o container está up

## 3. Testes de autenticação JWT

- [x] 3.1 Criar teste e2e de login com senha correta (`POST /auth-login` ou página de login Vue) e verificar que o token JWT é armazenado (localStorage/cookie) e o usuário é redirecionado para o dashboard
- [x] 3.2 Criar teste e2e de login com senha incorreta e verificar que a aplicação retorna HTTP 401 e exibe mensagem de erro
- [x] 3.3 Criar teste e2e de acesso a endpoint protegido sem token e verificar que retorna HTTP 401

## 4. Testes de bibliotecas de séries

- [x] 4.1 Criar teste e2e que acessa `/libraries` autenticado e verifica que apenas bibliotecas de tipo "show" são exibidas
- [x] 4.2 Criar teste e2e para cenário de Plex indisponível e verificar que a aplicação exibe erro de servidor indisponível

## 5. Testes de marcadores (CRUD)

- [x] 5.1 Criar teste e2e de criação de marcador manual (rating_key, kind, start_ms, end_ms) e verificar que o marcador é persistido (aparece na listagem e/ou no custom.json)
- [x] 5.2 Criar teste e2e de remoção de marcador e verificar que o marcador é deletado da listagem e do custom.json
- [x] 5.3 Criar teste e2e de listagem de marcadores (`GET /api/markers`) e verificar que retorna os marcadores criados

## 6. Testes de proxy de imagens

- [x] 6.1 Criar teste e2e que requisita `GET /api/images/thumb/{rating_key}` e verifica que retorna imagem com Content-Type correto (image/jpeg ou image/png)
- [x] 6.2 Criar teste e2e que verifica que o cabeçalho/URL da requisição de imagem não expõe o valor de PLEX_TOKEN no navegador

## 7. Ciclo de vida do container (iniciar/parar/monitorar)

- [x] 7.1 Criar teste e2e (ou script de orquestração) que inicia o container com `./start.sh` e verifica que o container está rodando (`docker compose ps` ou health check)
- [x] 7.2 Criar teste e2e (ou script de orquestração) que para o container com `./stop.sh` e verifica que o container não está mais em execução
- [x] 7.3 Criar teste e2e (ou script de orquestração) que verifica os logs via `./logs.sh --no-follow --tail 10` e confirma que retorna saída sem erro

## 8. Execução completa da suíte e2e e limpeza

- [x] 8.1 Rodar toda a suíte Playwright (`npx playwright test`) contra o container em execução e verificar que todos os testes passam
- [x] 8.2 Executar `./stop.sh` após os testes e verificar que o container é parado e removido

## Workflow follow-up

- Archive a mudança `teste-integracao-e2e-playwright` após revisão dos requisitos de teste.
- Verificar que o archive foi concluído com sucesso.
