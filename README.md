# Plex‑Autoplay  <img src="https://img.shields.io/badge/Plex%20Autoplay-100%25%20Free-%23E5A00D?style=for-the-badge&logo=plex" alt="Plex Autoplay badge">

> **Automatiza “Pular introdução” e “Próximo episódio” diretamente na API do Plex** – sem watchers de sistema de arquivos, sem `markers.json` locais, 100 % orientado à API oficial do Plex Media Server.

---  

## 🎯  Visão geral
| Funcionalidade | Como funciona agora |
|----------------|---------------------|
| **Descoberta de bibliotecas** | Consulta `GET /library/sections` no Plex e apresenta *cards* com capa, título e tipo (Série / Filme). |
| **Geração de marcadores** | Para cada episódio, roda a detecção (silêncio, logo, AniDB…) e grava **markers nativos** (`intro` / `credits`) via `Episode.addMarker`. |
| **Execução contínua** | *Polling* configurável (padrão 5 min) **ou** *webhook* `library.new / library.updated` para processamento quase em tempo real. |
| **Reprodução** | O cliente Plex (Web, iOS, Android, TV, etc.) já exibe os botões **“Pular intro” / “Pular créditos”** – nenhum código extra no player é necessário. |
| **Interface Web** | Dashboard único (autenticação + CSRF) para: <br>• Configurar `PLEX_URL` / `PLEX_TOKEN` <br>• Ajustar comportamento do Autoplay <br>• Disparar/acompanhar varreduras com **botão “Cancelar”** que interrompe o job quase instantaneamente <br>• Log em tempo real dentro do card de progresso (expandível com ▼/▶) <br>• Visualizar bibliotecas do Plex como cards |
| **Infraestrutura** | Um único container Docker que roda **app (Flask + Waitress/Cheroot) + Redis** via **supervisord** – sem dependências externas. |

---  

## 📦  Requisitos
| Item | Versão mínima |
|------|---------------|
| Docker Engine | 24.x |
| Docker Compose (plugin) | 2.x |
| Plex Media Server | ≥ 1.22 (API de *markers* nativa) |
| Conta Plex com **Owner / Admin** (para gerar token com permissão de escrita de markers) | – |

---  

## 🚀  Início rápido (TL;DR)

```bash
# 1️⃣  Clone o repositório
git clone https://github.com/seu-usuario/plex-autoplay.git
cd plex-autoplay

# 2️⃣  Crie o arquivo .env com suas credenciais (veja “Obtendo o token do Plex”)
cp .env.example .env
# edite .env → PLEX_URL=...  PLEX_TOKEN=...  SETUP_TOKEN=seu-token-secreto

# 3️⃣  Build + up  (redis sobe junto via supervisord)
./start.sh          # ou: docker compose build && docker compose up -d

# 4️⃣  Acesse o dashboard
# HTTP  : http://localhost:8080
# HTTPS : https://localhost:8080   (certificado auto‑assinado – aceite o aviso)

# 5️⃣  Crie o usuário admin – o token de *setup* já é o valor de SETUP_TOKEN
#    → Em ⚙ Configuração → salve URL/Token do Plex
#    → Em ▤ Bibliotecas → veja os cards das bibliotecas do Plex
#    → POST /api/plex/scan { "section_key":"1" } → job_id
#    → GET  /api/plex/scan/<job_id> → quando status=completed, marcadores já estão no Plex
```

---  

## 🔑  Obtendo o **Plex Token** (Owner / Admin)

1. Abra o **Plex Web App** em um navegador.  
2. Faça login com a conta **Owner** do servidor (ou uma conta *Admin* convidada).  
3. Vá em **Configurações → Conta → Token de autenticação** (ou acesse diretamente `https://plex.tv/claim`).  
4. Copie o token (`XXXXXXXXXXXXXXXXXXXX`).  
5. Cole no arquivo `.env` na variável `PLEX_TOKEN`.  

> **Importante:** o token **precisa ter permissão de escrita** (`addMarker` / `removeMarker`). Tokens de contas *restritas* (Home, Managed Users) **não funcionam**.

---  

## 🛡  Primeiro acesso – token pré‑definido (`SETUP_TOKEN`)

Por padrão o sistema gera um token aleatório e o grava em `config/dashboard-setup-token`.  
Para ambientes automatizados (CI/CD, Portainer, Ansible, etc.) você pode **definir o token antes de subir o container**:

```dotenv
# .env  (ou direto no docker‑compose.yml / GitHub Actions secrets)
SETUP_TOKEN=meu-token-super-seguro-123
```

* Com `SETUP_TOKEN` presente, a tela **“Criar administrador”** já aceita esse valor – não é necessário copiar do log.  
* Se a variável **não** estiver definida, o comportamento antigo (geração automática + arquivo) permanece.

---  

## ⚙️  Configuração (via UI ou `.env`)

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `PLEX_URL` | URL base do PMS (ex.: `http://192.168.1.50:32400`) | `http://localhost:32400` |
| `PLEX_TOKEN` | Token admin (obrigatório) | – |
| `SETUP_TOKEN` | Token de *primeiro acesso* (opcional) | *gerado automaticamente* |
| `AUTO_SKIP_INTRO` | Pular intro automaticamente | `true` |
| `AUTO_NEXT_EPISODE` | Avançar para próximo episódio automaticamente | `true` |
| `NEXT_EPISODE_THRESHOLD_SECONDS` | Segundos antes do fim para disparar “próximo” | `15` |
| `POLL_INTERVAL_SECONDS` | Intervalo de *polling* de sessões ativas | `2` |
| `DEFAULT_INTRO_SKIP_SECONDS` | Segundos iniciais a pular quando não houver marker | `0` |
| `SCAN_INTERVAL_MINUTES` | Intervalo do *polling* de varredura completa | `5` |
| `WEBHOOK_SECRET` | Segredo HMAC para validar `X-Plex-Signature` (opcional) | vazio |
| `WORKER_BACKEND` | `sync` (thread) ou `rq` (Redis) | `rq` |
| `REDIS_URL` | URL do Redis interno (não mude) | `redis://127.0.0.1:6379/0` |
| `ENABLE_FS_WATCHER` | Manter watcher legado (apenas migração) | `false` |
| `WEB_PORT` | Porta do dashboard (interno) | `8080` |
| `DASHBOARD_HTTPS` | Ativar HTTPS auto‑assinado | `true` |
| `DASHBOARD_COOKIE_SECURE` | Cookie `Secure` no HTTPS | `true` |
| `DASHBOARD_TLS_HOSTS` | Hosts para certificado auto‑assinado | `localhost,plex-autoplay,host.docker.internal` |
| `LOG_LEVEL` | Nível de log (`DEBUG`, `INFO`, `WARN`, `ERROR`) | `INFO` |

Todas as variáveis podem ser alteradas **pela UI** (⚙ Configuração) – o arquivo `config/app-config.json` é atualizado automaticamente e tem prioridade sobre o `.env`.

---  

## 🐳  Docker – detalhes do container único

```text
+---------------------------+
|  supervisord (PID 1)      |
|  ├─ redis-server :6379    |
|  └─ python -m plex_autoplay (Flask + Waitress/Cheroot) |
+---------------------------+
```

* `supervisord.conf` garante restart automático de ambos.  
* `REDIS_URL` e `WORKER_BACKEND=rq` já vêm prontos – o worker RQ consome jobs de varredura (`process_episode`).  
* Nenhum volume de mídia é necessário; o container só monta o **named volume** `plex-autoplay-config:/config` (persistência de `app-config.json`, chaves de sessão, certificado TLS).

---  

## 📚  Endpoints da API (autenticados – CSRF)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET`  | `/api/plex/libraries` | Lista seções do Plex (title, key, type, thumb_url). |
| `POST` | `/api/plex/scan` | Inicia varredura completa de uma seção (`{ "section_key": "1" }`). Retorna `job_id`. |
| `GET`  | `/api/plex/scan/<job_id>` | Status do job (`queued|running|completed|failed|cancelled`, processed, skipped, errors). |
| `POST` | `/api/plex/scan/<job_id>/cancel` | Solicita cancelamento do job em execução; o worker interrompe quase imediatamente. |
| `POST` | `/plex/webhook` | Recebe webhook `library.new / library.updated` do Plex (valida `X-Plex-Signature` com `WEBHOOK_SECRET`). |
| `GET`  | `/api/status` | Status do último job (inclui `job_id` quando há job ativo) + métricas de CPU/Mem/Rede. |
| `GET`  | `/config` / `POST /config` | UI de configuração (também expõe JSON via `GET /api/config` se necessário). |
| `GET`  | `/libraries` | Página “Bibliotecas” – renderiza cards do Plex + botão “Escanear”. |
| `GET`  | `/` | Dashboard – visão geral + último job. |

> Todas as rotas mutantes exigem sessão de admin + token CSRF (`X-CSRF-Token` header ou campo `csrf_token`).

---  

## 🔄  Fluxo de varredura (polling + webhook)

1. **Polling** (cron interno a cada `SCAN_INTERVAL_MINUTES`):  
   `PlexLibraryScanner.scan_section(section_key)` → para cada episódio sem markers → `MarkerGenerator.generate(file_path)` → `PlexManager.add_marker`.  
2. **Webhook** (opcional, near‑real‑time):  
   Plex POSTa em `/plex/webhook` → valida HMAC → extrai `ratingKey` → enfileira `process_episode(ratingKey)` no worker RQ.  
3. **Cancelamento**:  
   UI exibe botão **“Cancelar”** enquanto o job está `running`; ao clicar envia `POST /api/plex/scan/<job_id>/cancel` → o worker recebe sinal (`threading.Event`) e interrompe o loop de episódios imediatamente.  
4. **Logs em tempo real**:  
   O painel de progresso inclui uma área de log expansível (▼/▶) que recebe linhas via **Server‑Sent Events** (`/api/plex/scan/<job_id>/log/stream`).  
5. **Resultado**: markers `intro` / `credits` gravados no Plex → clientes mostram botões nativos.

---  

## 🛠  Desenvolvimento / Testes locais

```bash
# Subir stack em modo dev (logs no terminal)
docker compose up --build

# Rodar testes (pytest) dentro do container
docker compose run --rm plex-autoplay pytest -q

# Lint / type‑check
docker compose run --rm plex-autoplay ruff check .
docker compose run --rm plex-autoplay mypy plex_autoplay
```

*O código de detecção de marcadores (`marker_generator.py`) ainda é um *stub* – implemente a lógica real (ffprobe `silencedetect`, logo detection, AniDB, etc.) e rode o helper CLI:*

```bash
docker compose exec plex-autoplay python -m plex_autoplay.application.marker_generator /media/serie/ep01.mkv
```

---  

## 📂  Estrutura de pastas (resumo)

```
plex-autoplay/
├── Dockerfile                # python + redis + supervisor
├── supervisord.conf          # sobe redis + app
├── docker-compose.yml        # stack único (named volume)
├── start.sh / stop.sh / logs.sh
├── .env.example
├── config/                   # volume persistente (app-config.json, certs, session.key)
├── plex_autoplay/
│   ├── config.py            # Settings (dataclass)
│   ├── main.py              # entrypoint – apenas dashboard
│   ├── infrastructure/
│   │   ├── plex/client.py   # PlexManager (list_sections, list_episodes, add_marker…)
│   │   └── web/             # Flask app, rotas, templates, static
│   └── application/
│       ├── plex_library_scanner.py
│       └── marker_generator.py
└── tests/                    # pytest suite
```

---  

## 📖  Documentação adicional

| Arquivo | Conteúdo |
|---------|----------|
| `CONTRIBUTING.md` | Guia de contribuição, estilo de código, CI. |
| `openspec/changes/plex-api-markers-integration/` | Spec completa (proposal, design, tasks, spec). |
| `docs/specs/` | Especificações legadas (mantidas para histórico). |

---  

## 🛡  Segurança

* **HTTPS** auto‑assinado por padrão (`DASHBOARD_HTTPS=true`). Em produção, substitua os certificados em `config/` por um certificado válido (Let’s Encrypt, etc.) e defina `DASHBOARD_COOKIE_SECURE=true`.  
* **CSRF** em todas as rotas `POST/PUT/DELETE`.  
* **Webhook** protegido por HMAC (`WEBHOOK_SECRET`).  
* Token Plex **nunca** escrito nos logs (mascarado na UI).  
* Container roda como root apenas para permitir que `supervisord` inicie o Redis como `appuser`; o código da aplicação roda com privilégios mínimos.

---  

## 📈  Observabilidade

* Logs estruturados (`JSON`‑ready) – `LOG_LEVEL` controla verbosidade.  
* Métricas de processo expostas em `/api/status` (CPU, RAM, rede, disco).  
* Métricas Prometheus opcionais (descomente `prometheus_client` no `requirements.txt` e adicione endpoint `/metrics`).

---  

## 📄  Licença

MIT – veja `LICENSE`.

---  

## 🤝  Suporte / Comunidade

* **Issues** → GitHub Issues (bugs, feature requests).  
* **Discussões** → GitHub Discussions.  
* **Discord** → (link opcional) para ajuda rápida.

---

## ⚙️  CI/CD – GitHub Actions (`.github/workflows/ci.yml`)

```yaml
name: CI / CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  # --------------------------------------------------------------
  # 1️⃣  Lint, type‑check & unit tests
  # --------------------------------------------------------------
  test:
    runs-on: ubuntu-latest
    timeout-minutes: 15
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Set up Python
        uses: actions/setup-python@v5
        with:
          python-version: "3.12"
          cache: "pip"

      - name: Install deps
        run: |
          python -m pip install --upgrade pip
          pip install -r requirements.txt
          pip install ruff mypy pytest pytest-cov

      - name: Lint (ruff)
        run: ruff check .

      - name: Type‑check (mypy)
        run: mypy plex_autoplay

      - name: Unit tests (pytest)
        run: pytest -q --cov=plex_autoplay --cov-report=term-missing

  # --------------------------------------------------------------
  # 2️⃣  Build & push Docker image (only on tags / main)
  # --------------------------------------------------------------
  docker:
    needs: test
    if: github.event_name == 'push' && (github.ref == 'refs/heads/main' || startsWith(github.ref, 'refs/tags/'))
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write   # needed for GHCR
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Log in to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata (tags, labels)
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=ref,event=pr
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}
            type=raw,value=latest,enable={{is_default_branch}}

      - name: Build & push
        uses: docker/build-push-action@v5
        with:
          context: .
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

  # --------------------------------------------------------------
  # 3️⃣  Deploy (example – render, k8s, Portainer, etc.)
  # --------------------------------------------------------------
  # deploy:
  #   needs: docker
  #   runs-on: ubuntu-latest
  #   steps:
  #     - name: Deploy to …
  #       run: |
  #         echo "Add your deploy step here (helm, kubectl, docker compose on remote, etc.)"
```

**O que o workflow faz**

| Stage | O que executa |
|-------|----------------|
| **test** | `ruff`, `mypy`, `pytest` (cobertura) – falha rápido se algo quebrar. |
| **docker** | Build multi‑arch (via Buildx) e push para **GHCR** (`ghcr.io/<owner>/plex-autoplay`) somente em `main` ou tags (`v0.4.0`). Usa cache de camadas do GitHub Actions para builds rápidos. |
| **deploy** *(comentado)* | Ponto de extensão – você pode chamar `helm upgrade`, `kubectl apply`, `docker compose` remoto via SSH, etc. |

> **Segredos necessários** (Settings → Secrets → Actions)  
> * `GITHUB_TOKEN` – já existe automaticamente.  
> * Se você for fazer push para outro registry (Docker Hub, AWS ECR, etc.) adicione `DOCKERHUB_USERNAME / DOCKERHUB_TOKEN` e troque o step *Log in to GHCR*.

---  

## 📦  Como usar a imagem publicada

```bash
docker pull ghcr.io/seu-usuario/plex-autoplay:latest   # ou v0.4.0
docker run -d \
  --name plex-autoplay \
  -p 8080:8080 \
  -v plex-autoplay-config:/config \
  -e PLEX_URL=http://seu-plex:32400 \
  -e PLEX_TOKEN=xxxxxxxxxxxxxxxxxxxx \
  -e SETUP_TOKEN=meu-token-seguro \
  ghcr.io/seu-usuario/plex-autoplay:latest
```

---  

> **Feito com ❤️ para a comunidade Plex.**  
> *Automatize, relaxe e aproveite seus episódios sem interrupções.* 🎬✨