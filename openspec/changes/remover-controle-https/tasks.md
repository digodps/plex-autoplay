# Tasks: Remover Controle de HTTPS

## Fase 1: Backend — Remoção de Código TLS

- [ ] 1.1 Deletar `src/PlexAutoplay.Web/Configuration/ServerCertificateProvider.cs`
- [ ] 1.2 Deletar `src/PlexAutoplay.Web/Configuration/KestrelSetup.cs`
- [ ] 1.3 Editar `src/PlexAutoplay.Web/Program.cs`:
  - [ ] Remover `using System.Security.Cryptography.X509Certificates;`
  - [ ] Remover linhas 33-39 (seção "Certificate & Kestrel")
  - [ ] Adicionar `builder.WebHost.UseUrls("http://+:80");` após `builder.Host.UseSerilog();`
  - [ ] Mudar `app.MapAuthEndpoints(disableHttp)` → `app.MapAuthEndpoints()`
- [ ] 1.4 Editar `src/PlexAutoplay.Web/Endpoints/ConfigEndpoints.cs`:
  - [ ] Remover `https_port` e `disable_http` do objeto de retorno GET `/config`
- [ ] 1.5 Editar `src/PlexAutoplay.Web/Endpoints/AuthEndpoints.cs`:
  - [ ] Remover parâmetro `bool disableHttp = false` de `MapAuthEndpoints`
  - [ ] Mudar `var cookieSecure = !disableHttp;` → `var cookieSecure = false;`
  - [ ] Remover comentário linha 82 sobre HTTPS→HTTP downgrade
- [ ] 1.6 Editar `src/PlexAutoplay.Web/Properties/launchSettings.json`:
  - [ ] Substituir perfil "https" por "http" na porta 80

## Fase 2: Docker & Compose

- [ ] 2.1 Editar `infra/Dockerfile`:
  - [ ] Confirmar `EXPOSE 80` e `ENV ASPNETCORE_URLS=http://+:80`
  - [ ] Remover `DISABLE_HTTP=false`, `WEB_PORT=80` do ENV (manter apenas `ASPNETCORE_URLS`, `LOG_LEVEL`)
- [ ] 2.2 Editar `infra/docker-compose.yml`:
  - [ ] Ports: `"8080:80"` (era `"${WEB_HOST_PORT:-8080}:8081"`)
  - [ ] Environment: remover `WEB_PORT`, `DISABLE_HTTP`, `WS_PORT`
  - [ ] Manter: `MEDIA_ROOTS`, `SQLITE_CONNECTION_STRING`, `APP_CONFIG_PATH`, `LOG_LEVEL`
- [ ] 2.3 Editar `infra/docker-compose.test.yml`:
  - [ ] Ports: `"8080:80"` (era `"8443:80"`)
  - [ ] Environment: remover `WEB_PORT`, `ASPNETCORE_URLS`, `DISABLE_HTTP`, `CERT_PATH`, `CERT_PASSWORD`, `CERT_BASE64`, `HTTPS_PORT`, `OTEL_*`
  - [ ] Manter apenas variáveis essenciais (PLEX_URL, PLEX_TOKEN, MEDIA_ROOTS, JWT_SECRET, ADMIN_PASSWORD, SETUP_TOKEN, SQLITE_CONNECTION_STRING, APP_CONFIG_PATH, LOG_LEVEL)
- [ ] 2.4 Editar `infra/docker-compose.ghcr.yml`:
  - [ ] Ports: confirmar `"8080:80"`
  - [ ] Environment: remover `WEB_PORT`, `DISABLE_HTTP`

## Fase 3: Variáveis de Ambiente & Documentação

- [ ] 3.1 Editar `.env.example` (raiz):
  - [ ] Remover seção "--- Certificado TLS / HTTPS ---" (linhas 44-57)
  - [ ] Remover `WS_PORT` se não usado
- [ ] 3.2 Editar `infra/.env.example`:
  - [ ] Remover `DISABLE_HTTP=false` (linha 29)
- [ ] 3.3 Editar `README.md`:
  - [ ] Remover linha 50: `# HTTPS : https://localhost:8443`
  - [ ] Remover linhas 85-89 (tabela CERT_*, HTTPS_PORT, DISABLE_HTTP)
  - [ ] Remover linhas 115-118 (descrição HTTPS)
  - [ ] Atualizar exemplos de acesso para `http://localhost:8080`

## Fase 4: Scripts & Testes

- [ ] 4.1 Editar `scripts/start.sh`:
  - [ ] Linha 24: `LOGIN_URL="http://localhost:8080/login"`
  - [ ] Linhas 85-86: Atualizar echo porta para 8080
  - [ ] Linha 115: `curl -s -o /dev/null "$LOGIN_URL"` (remover `-k`)
  - [ ] Linhas 121-123: Atualizar mensagem para HTTP, remover "aceite o certificado auto-assinado"
- [ ] 4.2 Editar `scripts/acceptance-test.sh`:
  - [ ] Definir `BASE_URL="http://localhost:8080"` (ou ler do ambiente)
  - [ ] Passar para Playwright via `ACCEPTANCE_BASE_URL`
- [ ] 4.3 Editar `tests/e2e/acceptance.config.js`:
  - [ ] Linha 14: `baseURL: process.env.ACCEPTANCE_BASE_URL || "http://localhost:8080"`
  - [ ] Linha 15: Remover `ignoreHTTPSErrors: true`
- [ ] 4.4 Verificar `tests/e2e/playwright.config.js`:
  - [ ] Remover `ignoreHTTPSErrors: true` se não usado em outros testes

## Fase 5: Spec Principal (https-transporte)

- [ ] 5.1 Editar `openspec/specs/https-transporte/spec.md`:
  - [ ] Adicionar no topo: `# https-transporte Specification (OBSOLETO — substituído por remover-controle-https)`
  - [ ] Manter conteúdo para histórico

## Fase 6: Build, Lint, Validação

- [ ] 6.1 `dotnet build PlexAutoplay.slnx -c Release` — deve passar
- [ ] 6.2 `cd vue/plex-frontend && npm run build` — deve passar
- [ ] 6.3 `cd vue/plex-frontend && npm run lint` — deve passar
- [ ] 6.4 `docker compose -f infra/docker-compose.yml build` — deve passar
- [ ] 6.5 `openspec validate remover-controle-https --strict` — deve passar

## Fase 7: QA UI (Playwright)

- [ ] 7.1 Subir aplicação: `./scripts/start.sh --build`
- [ ] 7.2 Acessar `http://localhost:8080/login` — validar login funciona
- [ ] 7.3 Validar tema: toggle light/dark aplica corretamente em todos componentes
- [ ] 7.4 Validar menus: abertura, navegação, fechamento, estado ativo correto
- [ ] 7.5 Validar navegação principal (Dashboard, Séries, Bibliotecas, Marcadores, Configurações)
- [ ] 7.6 Screenshot evidence para relatório

## Fase 8: Commit & PR

- [ ] 8.1 `git add -A && git commit -m "feat(remover-controle-https): remove HTTPS/TLS control from application"`
- [ ] 8.2 Push branch e criar PR