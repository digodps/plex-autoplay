# Design: Remover Controle de HTTPS

## Visão Geral
A aplicação `plex-autoplay` deixará de gerenciar TLS. Irá expor apenas **HTTP** na porta **80** dentro do container. O operador é responsável por terminar TLS na borda (reverse proxy, load balancer cloud, tunnel, etc.).

## Arquitetura Atual → Nova

| Componente | Atual | Nova |
|---|---|---|
| Listener Kestrel | HTTPS (443) + WSS (8081) + HTTP opcional (8080 se DISABLE_HTTP=false) | HTTP (80) apenas |
| Certificado | Auto-assinado / CERT_PATH / CERT_BASE64 | **Nenhum** (removido) |
| Variáveis de ambiente TLS | `CERT_PATH`, `CERT_BASE64`, `CERT_PASSWORD`, `HTTPS_PORT`, `DISABLE_HTTP`, `WS_PORT` | **Removidas** |
| Config endpoint `/config` | Retorna `https_port`, `disable_http` | **Removidos** |
| Auth cookie `Secure` | `!disableHttp` (true em HTTPS) | `false` (HTTP) |
| Dockerfile `EXPOSE` | `80` (mas `ASPNETCORE_URLS=http://+:80` + DISABLE_HTTP=false) | `80` com `ASPNETCORE_URLS=http://+:80` |
| `docker-compose.yml` ports | `"${WEB_HOST_PORT:-8080}:8081"` | `"8080:80"` |
| `start.sh` health check | `curl -sk https://localhost:8443/login` | `curl -s http://localhost:8080/login` |
| E2E acceptance | `https://localhost:8443` + `ignoreHTTPSErrors` | `http://localhost:8080` sem ignore |

## Mudanças por Arquivo

### 1. `src/PlexAutoplay.Web/Program.cs`
- **Remover** linhas 33-39 (Certificate & Kestrel section)
- **Remover** `using System.Security.Cryptography.X509Certificates;`
- **Adicionar** direto após `builder.Host.UseSerilog();`:
```csharp
builder.WebHost.UseUrls("http://+:80");
```
- **Remover** parâmetro `disableHttp` do `app.MapAuthEndpoints(disableHttp)` → `app.MapAuthEndpoints()`

### 2. `src/PlexAutoplay.Web/Configuration/ServerCertificateProvider.cs`
- **Deletar arquivo inteiro**

### 3. `src/PlexAutoplay.Web/Configuration/KestrelSetup.cs`
- **Deletar arquivo inteiro**

### 4. `src/PlexAutoplay.Web/Endpoints/ConfigEndpoints.cs`
- **Remover** linhas 18-19 do objeto de retorno:
```csharp
https_port = Environment.GetEnvironmentVariable("HTTPS_PORT") ?? "443",
disable_http = Environment.GetEnvironmentVariable("DISABLE_HTTP") ?? "true",
```
- **Remover** `ws_port` se não usado em outro lugar (verificar) — manter por enquanto para compatibilidade com WebSocket PlexAutoSkip

### 5. `src/PlexAutoplay.Web/Endpoints/AuthEndpoints.cs`
- **Mudar** assinatura: `MapAuthEndpoints(this IEndpointRouteBuilder app, bool disableHttp = false)` → `MapAuthEndpoints(this IEndpointRouteBuilder app)`
- **Remover** parâmetro `disableHttp` da chamada em `Program.cs`
- **Mudar** linha 83: `var cookieSecure = !disableHttp;` → `var cookieSecure = false;`
- **Remover** comentário linha 82 sobre HTTPS→HTTP downgrade

### 6. `src/PlexAutoplay.Web/Properties/launchSettings.json`
- **Substituir** perfil "https" por "http":
```json
"http": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": false,
  "applicationUrl": "http://localhost:80",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

### 7. `infra/Dockerfile`
- **Manter** `EXPOSE 80`
- **Manter** `ENV ASPNETCORE_URLS=http://+:80`
- **Remover** `DISABLE_HTTP=false`, `WEB_PORT=80`, `HTTPS_PORT` (não existia), `LOG_LEVEL` (pode manter como default)

### 8. `infra/docker-compose.yml`
- **Ports**: `"8080:80"` (em vez de `"${WEB_HOST_PORT:-8080}:8081"`)
- **Environment**: remover `WEB_PORT`, `DISABLE_HTTP`, `WS_PORT` (se não usado), adicionar apenas variáveis essenciais
- **Manter** `MEDIA_ROOTS`, `SQLITE_CONNECTION_STRING`, `APP_CONFIG_PATH`, `LOG_LEVEL`

### 9. `infra/docker-compose.test.yml`
- **Ports**: `"8080:80"` (em vez de `"8443:80"`)
- **Environment**: remover `WEB_PORT=80`, `ASPNETCORE_URLS=http://0.0.0.0:80`, `DISABLE_HTTP=false`, `CERT_PATH`, `CERT_PASSWORD`, `CERT_BASE64`, `HTTPS_PORT`, `OTEL_*`
- **Manter** apenas: `PLEX_URL`, `PLEX_TOKEN`, `MEDIA_ROOTS`, `JWT_SECRET`, `ADMIN_PASSWORD`, `SETUP_TOKEN`, `SQLITE_CONNECTION_STRING`, `APP_CONFIG_PATH`, `LOG_LEVEL`

### 10. `infra/docker-compose.ghcr.yml`
- **Ports**: `"8080:80"` (em vez de `"8080:80"` — já está correto)
- **Environment**: remover `WEB_PORT`, `DISABLE_HTTP` (já expõe 80)
- **Manter** variáveis essenciais

### 11. `.env.example` (raiz)
- **Remover** seção "--- Certificado TLS / HTTPS ---" (linhas 44-57)
- **Remover** `WS_PORT` se não usado (verificar)
- **Manter**: `PLEX_URL`, `PLEX_TOKEN`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`, `WEB_PORT` (para referência, mas não usado), `SYNC_INTERVAL_MINUTES`, `MEDIA_ROOT`, `APP_CONFIG_PATH`, `LOG_LEVEL`, `ALLOWED_HOSTS`, `OTEL_*`

### 12. `infra/.env.example`
- **Remover** `DISABLE_HTTP` (linha 29)
- **Manter** variáveis essenciais

### 13. `scripts/start.sh`
- **Linha 24**: `LOGIN_URL="http://localhost:8080/login"`
- **Linha 85-86**: Atualizar echo para mostrar porta 8080
- **Linha 115**: `curl -s -o /dev/null "$LOGIN_URL"` (remover `-k`)
- **Linhas 121-123**: Atualizar mensagem para HTTP, remover "aceite o certificado auto-assinado"

### 14. `scripts/acceptance-test.sh`
- **Definir** `BASE_URL="http://localhost:8080"` (ou ler do ambiente)
- **Passar** para Playwright via `ACCEPTANCE_BASE_URL`

### 15. `tests/e2e/acceptance.config.js`
- **Linha 14**: `baseURL: process.env.ACCEPTANCE_BASE_URL || "http://localhost:8080"`
- **Linha 15**: **Remover** `ignoreHTTPSErrors: true`

### 16. `tests/e2e/playwright.config.js`
- **Verificar** se `ignoreHTTPSErrors` é usado — se só no acceptance, remover

### 17. `README.md`
- **Remover** linha 50: `# HTTPS : https://localhost:8443`
- **Remover** linhas 85-89 (tabela variáveis CERT_*, HTTPS_PORT, DISABLE_HTTP)
- **Remover** linha 115-118 (descrição HTTPS)
- **Atualizar** exemplos de acesso para `http://localhost:8080`

### 18. `openspec/specs/https-transporte/spec.md`
- **Marcar como obsoleto** no topo: `# https-transporte Specification (OBSOLETO — substituído por remover-controle-https)`
- **Manter** conteúdo para histórico, não deletar
- **O delta spec** `remover-controle-https` define o novo comportamento (apenas HTTP)

## Variáveis de Ambiente — Resumo Final

### Obrigatórias (validade no start.sh)
- `PLEX_URL`
- `PLEX_TOKEN`
- `MEDIA_ROOTS`
- `JWT_SECRET`
- `ADMIN_PASSWORD`

### Opcionais (com defaults seguros no código/compose)
- `SYNC_INTERVAL_MINUTES` (default 5)
- `MEDIA_ROOT` (default /media)
- `APP_CONFIG_PATH` (default /data/app-config.json)
- `SQLITE_CONNECTION_STRING` (default Data Source=/data/app.db)
- `LOG_LEVEL` (default Information)
- `ALLOWED_HOSTS` (default *)
- `OTEL_EXPORTER` (default console)
- `OTEL_ENDPOINT` (default vazio)
- `OTEL_SERVICE_NAME` (default plex-autoplay)

### **Removidas**
- `CERT_PATH`, `CERT_BASE64`, `CERT_PASSWORD`
- `HTTPS_PORT`
- `DISABLE_HTTP`
- `WS_PORT` (se não usado pelo frontend)

## WebSocket PlexAutoSkip
- Continua em `ws://` (não WSS) na porta **8081** interno do container
- O `docker-compose.yml` **não expõe** a porta 8081 no host (apenas interno)
- Se usuário quiser WSS externo, configura no reverse proxy (upgrade WebSocket + TLS)

## Compatibilidade
- **Breaking change**: URLs mudam de `https://localhost:8443` → `http://localhost:8080`
- Cookies de auth: `Secure=false` (funciona em HTTP)
- Reverse proxy esperado: NGINX/Caddy/Traefik/Cloud LB termina TLS e encaminha HTTP para container porta 80