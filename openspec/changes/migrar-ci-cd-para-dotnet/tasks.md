# Tasks

## 1. Stack de teste multiambiente (compose + script)

- [x] 1.1 Adicionar o serviço `plex-mock` ao `docker-compose.test.yml` (imagem `node:20-slim`, volume `./tests/e2e/mock-plex:/app`, comando `node server.js`, porta interna 32400, rede `plex-test-network`, `container_name: plex-mock`) — verificar: `docker compose -f docker-compose.test.yml config` valida o serviço
- [x] 1.2 Parametrizar o host da aplicação: `scripts/run-e2e-tests.sh` lê `APP_HOST` do ambiente com default `plex-autoplay.dns.podman`, e o compose repassa para `PLAYWRIGHT_BASE_URL` via `${APP_HOST}` — verificar: rodar `scripts/run-e2e-tests.sh` localmente (Podman) com o e2e passando, sem editar o script
- [x] 1.3 Conferir que `PLEX_URL` resolve `http://plex-mock:32400` (nome de serviço Compose) para a aplicação e o executor — verificar: localmente, `scripts/run-e2e-tests.sh` com `.env` apontando o mock e bibliotecas carregando no e2e

## 2. Workflow de CI

- [x] 2.1 Reescrever `.github/workflows/ci.yml`: job `build` (checkout v4, `actions/setup-dotnet` com SDK 10 e cache NuGet, `dotnet build PlexAutoplay.slnx`) nos triggers push main/develop e pull_request main — verificar: `actionlint` sem erros no arquivo
- [x] 2.2 Adicionar job `e2e` ao `ci.yml` no mesmo triggers: sobe `docker-compose.test.yml` com `APP_HOST=plex-autoplay-test`, executa `scripts/run-e2e-tests.sh`, e publica `test-artifacts/` com `actions/upload-artifact@v4` em `failure()` — verificar: `actionlint` sem erros e steps coerentes com o executor
- [x] 2.3 Remover `.github/workflows/test.yml` e o job `docker` legado do `ci.yml` — verificar: `git grep -l "test.yml" .github/` não retorna workflows ativos e `actionlint` passa em todos os workflows

## 3. Validação integrada

- [x] 3.1 Validar o stack completo localmente (Podman): `scripts/run-e2e-tests.sh` com `plex-mock` no Compose e `APP_HOST` default, e2e verde — verificar: saída do executor com todos os testes passando e `docker compose down` executado
- [ ] 3.2 Validar o pipeline no GitHub Actions: abrir PR com a change, conferir checks `build` e `e2e` verdes no PR — verificar: `gh pr checks` com os dois jobs de sucesso
