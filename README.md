# Plex‑Autoplay  <img src="https://img.shields.io/badge/Plex%20Autoplay-100%25%20Free-%23E5A00D?style=for-the-badge&logo=plex" alt="Plex Autoplay badge">

> **Dashboard web para gerenciar marcadores de intro/créditos no Plex via API oficial** – gera `custom.json` para o **PlexAutoSkip** (embutido) que envia comandos de *seek* aos players compatíveis.

---

## 🎯  Visão geral
| Funcionalidade | Status |
|----------------|--------|
| **Descoberta de bibliotecas** | ✅ Consulta `GET /library/sections` no Plex e apresenta cards com capa, título e tipo. |
| **Geração de marcadores** | ✅ Scanner manual via UI: roda detecção (ffprobe silence/logo) e grava marcadores no **custom.json** (formato PlexAutoSkip). |
| **Edição manual de marcadores** | ✅ UI para criar/editar/remover marcadores de intro e créditos por episódio. |
| **PlexAutoSkip embutido** | ✅ Lê `custom.json` e serve WebSocket na porta 8081 para players compatíveis. |
| **Interface Web** | ✅ Dashboard único (auth + CSRF): config Plex, bibliotecas, scan manual, logs tempo real. |
| **Infraestrutura** | ✅ Container único (supervisord): Flask + Redis + PlexAutoSkip. |

> ⚠️ **Auto-skip/auto-next server-side NÃO está implementado** (código removido). O skip funciona apenas via **PlexAutoSkip** nos players compatíveis.

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
# edite .env → PLEX_URL=...  PLEX_TOKEN=...  SETUP_TOKEN=seu-token-secreto

# 3️⃣  Build + up
./start.sh          # ou: docker compose build && docker compose up -d

# 4️⃣  Acesse o dashboard
# HTTP  : http://localhost:8080
# HTTPS : https://localhost:8080   (certificado auto‑assinado – aceite o aviso)

# 5️⃣  Crie o usuário admin → Em ⚙ Configuração → salve URL/Token do Plex
#     → Em ▤ Bibliotecas → veja os cards das bibliotecas do Plex
#     → POST /api/plex/scan { "section_key":"1" } → job_id
#     → GET  /api/plex/scan/<job_id> → quando status=completed, marcadores já estão no custom.json
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

## 🛡  Primeiro acesso – token pré‑definido (`SETUP_TOKEN`)

Por padrão o sistema gera um token aleatório e o grava em `config/dashboard-setup-token`.
Para ambientes automatizados (CI/CD, Portainer, Ansible, etc.) você pode **definir o token antes de subir o container**:

```dotenv
# .env  (ou direto no docker‑compose.yml / GitHub Actions secrets)
SETUP_TOKEN=meu-token-super-seguro-123
```

* Com `SETUP_TOKEN` presente, a tela **“Criar administrador”** já aceita esse valor.
* Se a variável **não** estiver definida, o comportamento antigo (geração automática + arquivo) permanece.

---

## ⚙️  Configuração (via UI ou `.env`)

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `PLEX_URL` | URL base do PMS (ex.: `http://192.168.1.50:32400`) | `http://localhost:32400` |
| `PLEX_TOKEN` | Token admin (obrigatório) | – |
| `SETUP_TOKEN` | Token de *primeiro acesso* (opcional) | *gerado automaticamente* |
| `WEBHOOK_SECRET` | Segredo HMAC para validar `X-Plex-Signature` (opcional) | vazio |
| `WEB_PORT` | Porta do dashboard (interno) | `8080` |
| `DASHBOARD_HTTPS` | Ativar HTTPS auto‑assinado | `true` |
| `DASHBOARD_COOKIE_SECURE` | Cookie `Secure` no HTTPS | `true` |
| `DASHBOARD_TLS_HOSTS` | Hosts para certificado auto‑assinado | `localhost,plex-autoplay,host.docker.internal` |
| `LOG_LEVEL` | Nível de log (`DEBUG`, `INFO`, `WARN`, `ERROR`) | `INFO` |
| `PLEX_AUTOPLAY_CUSTOM_JSON_PATH` | Caminho do `custom.json` do PlexAutoSkip (opcional) | `/config/plexautoskip/custom.json` |
| `PLEX_AUTOPLAY_CONFIG_DIR` | Diretório de config do PlexAutoSkip (opcional) | `/config/plexautoskip` |

> **Configurações do PlexAutoSkip** (geradas automaticamente no `config.ini` via `start_autoskip.sh`):  
> `unwatched = False`, `first-episode-series = Always`, `start = 0` (ver `start_autoskip.sh` para detalhes).

Todas as variáveis podem ser alteradas **pela UI** (⚙ Configuração) – o arquivo `config/app-config.json` é atualizado automaticamente e tem prioridade sobre o `.env`.

---

## 🐳  Docker – detalhes do container único

```text
+---------------------------+
|  supervisord (PID 1)      |
|  ├─ redis-server :6379    |
|  ├─ python -m plex_autoplay (Flask + Waitress/Cheroot) |
|  └─ python /opt/plexautoskip/main.py (PlexAutoSkip) |
+---------------------------+
```

* `supervisord.conf` garante restart automático dos três processos.
* **Redis** roda mas **não é usado** pelo código da aplicação atualmente (mantido para futuras filas).
* **PlexAutoSkip** roda embutido, lendo `/config/plexautoskip/custom.json` e servindo WebSocket na porta 8081.
* Nenhum volume de mídia é necessário; o container só monta o **named volume** `plex-autoplay-config:/config` (persistência de `app-config.json`, `custom.json`, certificado TLS, chaves de sessão).

---

## 📚  Endpoints da API (autenticados – CSRF)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET`  | `/api/plex/libraries` | Lista seções do Plex (title, key, type, thumb_url). |
| `POST` | `/api/plex/scan` | Inicia varredura completa de uma seção (`{ "section_key": "1" }`). Retorna `job_id`. |
| `GET`  | `/api/plex/scan/<job_id>` | Status do job (`queued|running|completed|failed|cancelled`, processed, skipped, errors). |
| `POST` | `/api/plex/scan/<job_id>/cancel` | Solicita cancelamento do job em execução; o worker interrompe quase imediatamente. |
| `POST` | `/plex/webhook` | Recebe webhook `library.new / library.updated` do Plex (valida `X-Plex-Signature` com `WEBHOOK_SECRET`). **Atualmente apenas loga o evento.** |
| `GET`  | `/api/sync/library/<section_key>/tree` | Retorna árvore de séries/episódios com marcadores mesclados (Plex + custom.json). |
| `POST` | `/api/sync/marker` | Salva/atualiza marcador (`{rating_key, kind, start_ms, end_ms, section_key}`) no custom.json. |
| `GET`  | `/api/status` | Status do último job + métricas de CPU/Mem/Rede. |
| `GET`  | `/config` / `POST /config` | UI de configuração. |
| `GET`  | `/libraries` | Página “Bibliotecas” – renderiza cards do Plex + botão “Escanear”. |
| `GET`  | `/library/<section_key>` | Visualização de biblioteca com séries, episódios e edição de marcadores. |
| `GET`  | `/` | Dashboard – visão geral + último job. |

> Todas as rotas mutantes exigem sessão de admin + token CSRF (`X-CSRF-Token` header ou campo `csrf_token`).

---

## 🔄  Fluxo de varredura (manual via UI)

1. Usuário clica **“Escanear”** em uma biblioteca no dashboard.
2. `POST /api/plex/scan { "section_key": "1" }` → cria job assíncrono.
3. `PlexLibraryScanner.scan_section(section_key)`:
   - Lista episódios via `PlexManager.list_episodes()`
   - Para cada episódio sem marcadores nativos: roda `MarkerGenerator.generate(file_path)` (ffprobe)
   - Salva marcadores no **custom.json** (formato PlexAutoSkip).
4. UI exibe progresso em tempo real via **Server‑Sent Events** (`/api/plex/scan/<job_id>/log/stream`).
5. Botão **“Cancelar”** interrompe o job quase instantaneamente.
6. **PlexAutoSkip** lê o `custom.json` atualizado e envia comandos de *seek* aos players compatíveis.

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

*O código de detecção de marcadores (`marker_generator.py`) é um *stub* – implemente a lógica real (ffprobe `silencedetect`, logo detection, AniDB, etc.) e rode o helper CLI:*

```bash
docker compose exec plex-autoplay python -m plex_autoplay.application.marker_generator /media/serie/ep01.mkv
```

---

## 📂  Estrutura de pastas (resumo)

```
plex-autoplay/
├── Dockerfile                # python + redis + supervisor
├── supervisord.conf          # sobe redis + app + plexautoskip
├── docker-compose.yml        # stack único (named volume)
├── start.sh / stop.sh / logs.sh
├── .env.example
├── config/                   # volume persistente (app-config.json, certs, session.key)
├── plex_autoplay/
│   ├── config.py            # Settings (dataclass)
│   ├── main.py              # entrypoint – apenas dashboard
│   ├── infrastructure/
│   │   ├── paths.py         # Paths do PlexAutoSkip (env overrides)
│   │   ├── plex/client.py   # PlexManager (list_sections, list_episodes, add_marker…)
│   │   ├── markers/
│   │   │   └── json_repository.py  # Leitura custom.json + marcadores nativos do Plex
│   │   └── web/             # Flask app, rotas, templates, static
│   └── application/
│       ├── manual_marker_service.py  # CRUD de marcadores manuais (UI)
│       ├── plex_library_scanner.py   # Scanner Plex → custom.json
│       └── marker_generator.py       # Detecção via ffprobe (stub)
└── tests/                    # pytest suite (config, PlexManager, metrics)
```

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
* Métricas Prometheus opcionais (adicione `prometheus_client` no `requirements.txt` e endpoint `/metrics`).

---

## ⚠️  Limitações conhecidas – iOS / Web

> **Importante:** O PlexAutoSkip **não funciona** para controle remoto via **app iOS do Plex** nem via **Plex Web App** no navegador.

### Por que não funciona

O PlexAutoSkip depende do protocolo **Plex Companion** para enviar comandos de *seek* diretamente ao player. A partir de 2023, a Plex **descontinuou oficialmente** o protocolo Companion:

- O app **iOS** redesenhado não expõe mais o endpoint de controle remoto local.
- O **Plex Web App** no navegador também não expõe mais essa API.
- A Plex confirmou oficialmente que o protocolo Companion foi descontinuado.

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