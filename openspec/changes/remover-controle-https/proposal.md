# Proposal: Remover Controle de HTTPS da Aplicação

## Resumo
Remover todo o controle de HTTPS/TLS/certificados da aplicação `plex-autoplay`. A aplicação passará a servir apenas **HTTP** na porta 80 (interno do container). O TLS/HTTPS será responsabilidade **exclusiva da infraestrutura do usuário** (reverse proxy: NGINX, Traefik, Caddy, cloud load balancers, etc.).

## Why
- **Simplicidade**: O código de certificado auto-assinado, CERT_PATH, CERT_BASE64, DISABLE_HTTP, KestrelSetup, ServerCertificateProvider adiciona ~300 linhas de complexidade.
- **Separação de responsabilidades**: TLS é preocupação de infraestrutura/rede, não de aplicação.
- **Flexibilidade**: Usuários em produção já usam reverse proxy (NGINX/Caddy/Traefik) ou cloud LB (ALB, Cloud Run, Azure Container Apps) que terminam TLS na borda.
- **Menos bugs**: Elimina falhas de handshake IPv6 TLS, certificados expirados, geração de cert auto-assinado, validação de PFX, etc.
- **Testes mais simples**: E2E roda em HTTP puro sem `ignoreHTTPSErrors` nem `curl -k`.

## Motivação
- **Simplicidade**: O código de certificado auto-assinado, CERT_PATH, CERT_BASE64, DISABLE_HTTP, KestrelSetup, ServerCertificateProvider adiciona ~300 linhas de complexidade.
- **Separação de responsabilidades**: TLS é preocupação de infraestrutura/rede, não de aplicação.
- **Flexibilidade**: Usuários em produção já usam reverse proxy (NGINX/Caddy/Traefik) ou cloud LB (ALB, Cloud Run, Azure Container Apps) que terminam TLS na borda.
- **Menos bugs**: Elimina falhas de handshake IPv6 TLS, certificados expirados, geração de cert auto-assinado, validação de PFX, etc.
- **Testes mais simples**: E2E roda em HTTP puro sem `ignoreHTTPSErrors` nem `curl -k`.

## Escopo da Remoção
### Código Backend (remover)
- `src/PlexAutoplay.Web/Configuration/ServerCertificateProvider.cs` — inteiro
- `src/PlexAutoplay.Web/Configuration/KestrelSetup.cs` — inteiro (substituir por configuração simples HTTP)
- `src/PlexAutoplay.Web/Program.cs` — remover seção "Certificate & Kestrel", chamar `builder.WebHost.UseUrls("http://+:80")` direto
- `src/PlexAutoplay.Web/Endpoints/ConfigEndpoints.cs` — remover campos `https_port`, `disable_http` do GET /config
- `src/PlexAutoplay.Web/Endpoints/AuthEndpoints.cs` — remover parâmetro `disableHttp` e lógica `cookieSecure = !disableHttp` → usar `false` (HTTP)
- `src/PlexAutoplay.Web/Properties/launchSettings.json` — remover perfil "https", deixar apenas "http" na porta 80

### Configuração / Ambiente (remover variáveis)
- `CERT_PATH`, `CERT_BASE64`, `CERT_PASSWORD`, `HTTPS_PORT`, `DISABLE_HTTP`, `WS_PORT`
- `.env.example` (raiz)
- `infra/.env.example`
- `infra/docker-compose.yml` (ajustar portas/env)
- `infra/docker-compose.test.yml` (ajustar portas/env, remover 8443)
- `infra/docker-compose.ghcr.yml` (ajustar portas/env)
- `infra/Dockerfile` — `EXPOSE 80`, `ASPNETCORE_URLS=http://+:80`, remover `DISABLE_HTTP`, `WEB_PORT`, `HTTPS_PORT`

### Scripts (ajustar)
- `scripts/start.sh` — `LOGIN_URL="http://localhost:8080/login"`, `curl -s` (sem `-k`), health check HTTP
- `scripts/acceptance-test.sh` — `BASE_URL` HTTP
- `tests/e2e/acceptance.config.js` — `baseURL: "http://localhost:8080"`, remover `ignoreHTTPSErrors`
- `tests/e2e/playwright.config.js` — remover `ignoreHTTPSErrors` se não usado em outros testes

### Documentação (atualizar)
- `README.md` — remover seção HTTPS, certificado auto-assinado, portas 8443/443, instruções de certificado
- `openspec/specs/https-transporte/spec.md` — **arquivar/descontinuar** (marcar como obsoleto, não deletar — manter histórico)

### Specs Principais (delta)
- Criar delta spec `remover-controle-https` que **remove** requirements do `https-transporte` e adiciona requirement de "Servir apenas HTTP"

## Fora de Escopo
- Não alterar WebSocket PlexAutoSkip: continuará em `ws://` (não WSS) na porta 8081 interno. Se usuário quiser WSS, faz no proxy reverso.
- Não alterar autenticação JWT (stateless, sem expiração) — continua igual.
- Não alterar `PLEX_URL` — continua aceitando `http://` ou `https://` do Plex Media Server externo.

## Critérios de Aceitação
1. Aplicação sobe em HTTP na porta 80 (interno) / 8080 (host via docker-compose)
2. `./scripts/start.sh` funciona sem `-k` no curl, exibe link `http://localhost:8080/login`
3. Testes E2E (acceptance) rodam contra HTTP sem `ignoreHTTPSErrors`
4. `docker compose up` sobe container saudável em HTTP
5. Build passa (`dotnet build`, `npm run build`, `docker build`)
6. Lint/format passa
7. `openspec validate remover-controle-https --strict` passa
8. QA UI: tema light/dark toggle funciona, menus abrem/navegam/fecham, estado ativo correto