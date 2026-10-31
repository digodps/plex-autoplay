# Plex‑Autoplay  <img src="https://img.shields.io/badge/Plex%20Autoplay-100%25%20Free-%23E5A00D?style=for-the-badge&logo=plex" alt="Plex Autoplay badge">

> **Dashboard web para gerenciar marcadores de intro/créditos no Plex via API oficial** – gera `SQLite marker database` para o **PlexAutoSkip** (embutido) que envia comandos de *seek* aos players compatíveis.

---

## 🎯  Visão geral

| Funcionalidade | Status |
|----------------|--------|
| **Descoberta de bibliotecas** | ✅ Consulta `GET /library/sections` no Plex e apresenta cards com capa, título e tipo. |
| **Geração de marcadores** | ✅ Scanner manual via UI: grava marcadores no `SQLite marker database` (formato PlexAutoSkip). |
| **Edição manual de marcadores** | ✅ UI para criar/editar/remover marcadores de intro e créditos por episódio. |
| **PlexAutoSkip embutido** | ✅ Lê `SQLite marker database` e serve WebSocket na porta 8081 para players compatíveis. |
| **Interface Web** | ✅ Dashboard único (auth + CSRF): config Plex, bibliotecas, scan manual, logs tempo real. |
| **Infraestrutura** | ✅ Container único (.NET 10): ASP.NET Core Web API + Vue SPA + SQLite. |
| **Testes E2E** | ✅ Executor de testes dentro do Docker com Playwright + captura de artefatos de falha. |
| **Observabilidade** | ✅ OpenTelemetry (traces + métricas Prometheus) + Serilog. |

> ⚠️ **Auto-skip/auto-next server-side NÃO está implementado**. O skip funciona apenas via **PlexAutoSkip** nos players compatíveis.

---

## 📦  Requisitos

| Item | Versão mínima |
|------|---------------|
| Docker Engine | 24.x |
| Docker Compose (plugin) | 2.x |
| Plex Media Server | ≥ 1.22 (API de *markers* nativa) |
| Conta Plex com **Owner / Admin** | – |

---

## 🚀  Início rápido (TL;DR)

```bash
# 1️⃣  Clone o repositório
git clone https://github.com/seu-usuario/plex-autoplay.git
cd plex-autoplay

# 2️⃣  Crie o arquivo .env com suas credenciais
cp .env.example .env
# edite .env → PLEX_URL=...  PLEX_TOKEN=...  JWT_SECRET=...  ADMIN_PASSWORD=...  MEDIA_ROOT=...

# 3️⃣  Build + up
./scripts/start.sh --detach

# 4️⃣  Acesse o dashboard
# HTTPS : https://localhost:8443   (certificado auto‑assinado – aceite o aviso)
# WebSocket (PlexAutoSkip): wss://localhost:8081

# 5️⃣  Crie o usuário admin → Em ⚙ Configuração → salve URL/Token do Plex
#     → Em ▤ Bibliotecas → veja os cards das bibliotecas do Plex
```

---

## 🔑  Obtendo o **Plex Token** (Owner / Admin)

1. Abra o **Plex Web App** em um navegador.
2. Faça login com a conta **Owner** do servidor (ou uma conta *Admin* convidada).
3. Vá em **Configurações → Conta → Token de autenticação** (ou acesse `https://plex.tv/claim`).
4. Copie o token (`XXXXXXXXXXXXXXXXXXXX`).
5. Cole no arquivo `.env` na variável `PLEX_TOKEN`.

> **Importante:** o token **precisa ter permissão de escrita** (`addMarker` / `removeMarker`). Tokens de contas *restritas* (Home, Managed Users) **não funcionam**.

---

## ⚙️  Configuração (via UI ou `.env`)

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `PLEX_URL` | URL base do PMS (ex.: `http://192.168.1.50:32400`) | – |
| `PLEX_TOKEN` | Token admin (obrigatório) | – |
| `JWT_SECRET` | Chave secreta para assinar tokens JWT | – |
| `ADMIN_PASSWORD` | Senha para login no painel administrativo | – |
| `WEB_PORT` | Porta do dashboard (interno) | 8080 |
| `WS_PORT` | Porta WebSocket do PlexAutoSkip (interno) | 8081 |
| `SYNC_INTERVAL_MINUTES` | Intervalo em minutos entre varreduras automáticas das bibliotecas | 5 |
| `APP_CONFIG_PATH` | Caminho do `app-config.json` | `/data/app-config.json` |
| `CERT_PATH` | Caminho para arquivo PFX no container | vazio |
| `CERT_PASSWORD` | Senha do arquivo PFX (opcional) | vazio |
| `CERT_BASE64` | Conteúdo base64 de um arquivo PFX (alternativa a `CERT_PATH`) | vazio |
| `HTTPS_PORT` | Porta HTTPS (padrão: 443) | 443 |
| `DISABLE_HTTP` | Desabilitar listener HTTP completamente | true |
| `ASPNETCORE_ENVIRONMENT` | Ambiente ASP.NET Core | Development |
| `LOG_LEVEL` | Nível de log (`DEBUG`, `INFO`, `WARN`, `ERROR`) | Information |
| `ALLOWED_HOSTS` | Hosts permitidos | * |
| `OTEL_EXPORTER` | Exportador de telemetria: `console` ou `otlp` | console |
| `OTEL_ENDPOINT` | Endpoint OTLP (apenas quando `OTEL_EXPORTER=otlp`) | vazio |
| `OTEL_SERVICE_NAME` | Nome do serviço para identificação nos traces/métricas | plex-autoplay |

Todas as variáveis são lidas exclusivamente de variáveis de ambiente. O arquivo `app-config.json` não é usado para configuração (consulte o spec `config-env`).

---

## 🐳  Docker – detalhes do container

O container roda .NET 10 ASP.NET Core Web API + Vue SPA + SQLite em modo HTTPS-only.

```text
+--------------------------------------------------+
|  Container único (PID 1 = .NET 10)               |
|  ├─ Kestrel :443 (HTTPS API + Vue SPA)           |
|  ├─ Kestrel :8081 (WSS PlexAutoSkip)             |
|  ├─ OpenTelemetry (traces + métricas Prometheus) |
|  └─ SQLite (/data/app.db)                        |
+--------------------------------------------------+
```

- **HTTPS** na porta 443 do container (mapeada para 8443 no host em modo rootless).
- **WebSocket (PlexAutoSkip)** na porta 8081.
- **HTTP** desabilitado por padrão (`DISABLE_HTTP=true`).
- **Certificado auto-assinado** gerado automaticamente quando nenhum `CERT_PATH`/`CERT_BASE64` é fornecido.
- **Volumes**: `plex-data` (`/data` - SQLite, SQLite marker database, certificados) e bind mount `/media` (arquivos de mídia via `MEDIA_ROOT`).

---

## 📚  Endpoints da API (autenticados – CSRF)

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `POST` | `/auth/login` | Login com senha admin, retorna JWT | Não |
| `GET` | `/api/plex/libraries` | Lista bibliotecas de séries | Sim |
| `POST` | `/api/plex/scan` | Inicia escaneamento de marcadores | Sim |
| `GET` | `/api/sync/status/{jobId}` | Status do scan | Sim |
| `GET` | `/api/sync/log/stream/{jobId}` | Progresso via SSE | Sim |
| `POST` | `/api/sync/cancel/{jobId}` | Cancela scan em execução | Sim |
| `GET/POST/DELETE` | `/api/markers` | CRUD de marcadores | Sim |
| `GET` | `/api/images/thumb/{ratingKey}` | Proxy de imagens do Plex | Sim |
| `GET` | `/api/status` | Status do último job + métricas | Sim |
| `GET` | `/config` / `POST /config` | UI de configuração | Sim |
| `GET` | `/libraries` | Página “Bibliotecas” – renderiza cards do Plex | Sim |
| `GET` | `/library/<section_key>` | Visualização de biblioteca com séries e episódios | Sim |
| `GET` | `/` | Dashboard – visão geral + último job | Sim |
| `WS` | `/ws` | WebSocket PlexAutoSkip (porta 8081) | Não |

> Todas as rotas mutantes exigem sessão de admin + token CSRF.

---

## 🔄  Fluxo de varredura (manual via UI)

1. Usuário clica **“Escanear”** em uma biblioteca no dashboard.
2. `POST /api/plex/scan { "section_key": "1" }` → cria job assíncrono.
3. `PlexLibraryScanner` processa a seção e salva marcadores no `SQLite marker database`.
4. UI exibe progresso via **Server‑Sent Events** (`/api/sync/log/stream/{jobId}`).
5. Botão **“Cancelar”** interrompe o job quase instantaneamente.
6. **PlexAutoSkip** lê o `SQLite marker database` atualizado e envia comandos de *seek* aos players compatíveis.

---

## 🧪  Testes E2E (Playwright)

O projeto inclui um executor de testes e2e que roda **dentro do Docker**:

```bash
# Verificar que o Docker está disponível
docker --version && docker compose version

# Subir a aplicação
./scripts/start.sh --detach

# Rodar os testes e2e
cd tests/e2e
npx playwright test

# Ver artefatos de falha (se houver)
ls test-artifacts/
```

### Estrutura do executor

```
docker-compose.test.yml    # Compose separado para testes (executor + app)
Dockerfile.test-executor   # Imagem Node + Playwright + Docker CLI
scripts/run-e2e-tests.sh   # Orquestração: valida .env → compose up → playwright → captura falhas → compose down
tests/e2e/mock-plex/       # Servidor Plex mock para CI
```

### GitHub Actions

O workflow `.github/workflows/test.yml` roda o executor de testes com Plex mockado para CI/CD.

---

## 🧑‍💻  Desenvolvimento

### Pré-requisitos
- .NET 10 SDK
- Node.js 20+
- Docker Engine 24.x+
- Docker Compose 2.x

### Comandos do dia a dia

```bash
# Subir stack em modo dev (logs no terminal)
./scripts/start.sh

# Subir em segundo plano
./scripts/start.sh --detach

# Ver logs
./scripts/logs.sh --no-follow --tail 20

# Parar
./scripts/stop.sh

# Rodar testes e2e
cd tests/e2e
npx playwright test

# Pre-commit (valida .NET, Vue, YAML, JSON, GitHub Actions)
pre-commit run --all-files
```

### Estrutura de pastas

```
plex-autoplay/
├── src/
│   ├── PlexAutoplay.Web/          # .NET 10 ASP.NET Core Web API + Vue SPA
│   ├── PlexAutoplay.Application/  # Casos de uso (Application layer)
│   ├── PlexAutoplay.Domain/       # Entidades e portas (Domain layer)
│   └── PlexAutoplay.Infrastructure/  # Implementações (Infrastructure layer)
├── tests/
│   └── e2e/                       # Testes Playwright + mock Plex
├── scripts/
│   ├── start.sh                   # Sobe o container
│   ├── stop.sh                    # Para o container
│   ├── logs.sh                    # Visualiza logs
│   └── run-e2e-tests.sh           # Executor de testes e2e dentro do Docker
├── docker-compose.yml             # Stack de produção
├── docker-compose.test.yml        # Stack de testes (isolado)
├── Dockerfile                     # Imagem .NET 10
├── Dockerfile.test-executor       # Imagem Node + Playwright
├── .env / .env.example            # Variáveis de ambiente
├── openspec/                      # Especificações OpenSpec
└── README.md
```

---

## 🛡  Segurança

* **HTTPS** auto‑assinado por padrão (`DISABLE_HTTP=true`). Em produção, substitua os certificados por um certificado válido (Let’s Encrypt, etc.) e defina `DASHBOARD_COOKIE_SECURE=true`.
* **CSRF** em todas as rotas `POST/PUT/DELETE` sob `/api/` (cabeçalho `X-CSRF-TOKEN`).
* **Webhook** protegido por HMAC (`WEBHOOK_SECRET`).
* Token Plex **nunca** escrito nos logs (mascarado na UI).
* Container roda com privilégios mínimos.

---

## 📈  Observabilidade

* Logs estruturados (`JSON`‑ready) – `LOG_LEVEL` controla verbosidade.
* **OpenTelemetry**: traces distribuídos e métricas Prometheus.
* Métricas de processo expostas em `/api/status` (CPU, RAM, rede, disco).
* Endpoint `/metrics` (Prometheus).

---

## ⚠️  Limitações conhecidas – iOS / Web

> **Importante:** O PlexAutoSkip **não funciona** para controle remoto via **app iOS do Plex** nem via **Plex Web App** no navegador.

### Por que não funciona

O PlexAutoSkip depende do protocolo **Plex Companion** para enviar comandos de *seek* diretamente ao player. A partir de 2023, a Plex **descontinuou oficialmente** o protocolo Companion:

- O app **iOS** redesenhado não expõe mais o endpoint de controle remoto local.
- O **Plex Web App** no navegador também não expõe mais essa API.

### Clientes que **FUNCIONAM** com PlexAutoSkip

| Cliente | Status |
|---------|--------|
| **Plex Desktop App** (Windows/Mac/Linux) | ✅ Funciona |
| **Plex para Android TV / Google TV** | ✅ Funciona |
| **Plex para Apple TV** | ✅ Funciona |
| **Plex Home Theater** (legado) | ✅ Funciona |
| **Plex para Android (Mobile)** | ⚠️ Parcial |
| **Plex para iOS (iPhone/iPad)** | ❌ Não funciona |
| **Plex Web App** (navegador) | ❌ Não funciona |

### Alternativas para usuários iOS/Web

1. **Use o app Plex Desktop** – o skip automático funcionará.
2. **Use AirPlay** do iOS para Apple TV.
3. **Use Chromecast / Google Cast** do navegador/Android para TV.
4. **Plex Desktop App** no mesmo servidor – funciona como player controlável.

---

## 📄  Licença

MIT – veja `LICENSE`.

---

## 🤝  Suporte / Comunidade

* **Issues** → GitHub Issues (bugs, feature requests).
* **Discussões** → GitHub Discussions.
* **Discord** → (link opcional) para ajuda rápida.

---

> **Feito com ❤️ para a comunidade Plex.**
> *Gerencie seus marcadores, relaxe e aproveite seus episódios sem interrupções.* 🎬✨