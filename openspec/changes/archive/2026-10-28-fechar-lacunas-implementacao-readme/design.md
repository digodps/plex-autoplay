# Design

## Context

O app é um container único .NET 10 (ASP.NET Core Web API + Vue SPA + SQLite) rodando em HTTPS-only. Já existem: autenticação JWT stateless, descoberta de bibliotecas (`GET /library/sections`), proxy de imagens com token server-side, repositório SQLite de marcadores, detecção ffprobe (`FfmpegMarkerDetector`), PlexAutoSkipService que lê `custom.json` (porém o WebSocket `/ws` só faz *echo*), OpenTelemetry básico e testes E2E com Playwright + mock Plex.

O que falta, segundo a proposta: scanner real (listar episódios, mapear `MediaPart.file` + `MEDIA_ROOT`, rodar ffprobe, gravar `custom.json` + SQLite), `SyncBackgroundService` real (hoje *stub*), API de marcadores gravando em `custom.json`, WebSocket interpretando mensagens do player e enviando *seek*, endpoints `/api/status`, `/config`, `/library/<section_key>`, CSRF em rotas mutantes, métricas Prometheus reais e consumo das variáveis de ambiente documentadas no README.

## Goals / Non-Goals

**Goals:**
- Fluxo ponta a ponta funcional: biblioteca Plex → episódios → detecção ffprobe → `custom.json` + SQLite → PlexAutoSkip envia *seek* ao player.
- Endpoints ausentes (`/api/status`, `/config`, `/library/<section_key>`) implementados.
- CSRF protection em rotas mutantes.
- Métricas Prometheus reais e `/api/status` com métricas de processo.
- Todas as variáveis de ambiente do README consumidas exclusivamente via env vars (sem `app-config.json`).

**Non-Goals:**
- Auto-skip server-side sem PlexAutoSkip (o skip continua via PlexAutoSkip nos players compatíveis; iOS/Web permanecem sem suporte, conforme limitações já documentadas).
- Suporte a múltiplos usuários além do admin único.
- Alterar a arquitetura de container único ou adicionar banco externo (Redis, Postgres).

## Decisions

### 1. Scanner real como BackgroundService + endpoints manuais
O `SyncBackgroundService` deixa de ser *stub* e orquestra: lista bibliotecas de séries → para cada, lista episódios → para cada episódio sem marcadores, resolve caminho local via `MEDIA_ROOT` + `MediaPart.file` → roda `FfmpegMarkerDetector` → grava em SQLite e reescreve `custom.json`. O endpoint `POST /api/plex/scan` dispara o mesmo fluxo sob demanda (job assíncrono com SSE).

**Alternativa considerada:** criar um uso de caso separado (`ScanLibrariesUseCase`) chamado tanto pelo background service quanto pelo endpoint. **Decisão:** manter a lógica no `SyncBackgroundService` e expor via endpoint, reusando `ScanLibrariesUseCase` e `SaveMarkerUseCase` que já existem na camada Application. Isso evita duplicar orquestração.

### 2. Gravação de custom.json na camada de infraestrutura
Criar um serviço `CustomJsonWriterService` (ou estender `PlexAutoSkipService`) responsável por ler, reescrever e atomicamente gravar `custom.json` no formato PlexAutoSkip (keyed por rating key, array de {kind, start_ms, end_ms}). Tanto o scanner quanto a API de marcadores (POST/PUT/DELETE) usam esse serviço.

**Alternativa considerada:** gravar diretamente nos controllers da Web. **Decisão:** manter na infraestrutura para separar comportamento de transporte e permitir reuso entre scanner, background service e API.

### 3. WebSocket PlexAutoSkip interpreta mensagens do player
O endpoint `/ws` (na porta 8081, WSS) passa de *echo* para: aceitar conexão, ler mensagens de texto do player, identificar rating key e estado de reprodução, e responder com comando de *seek* (p. ex., pular para `end_ms` do intro ou para o final dos créditos) quando houver marcador aplicável.

**Alternativa considerada:** implementar o protocolo Plex Companion completo. **Decisão:** implementar o subset mínimo necessário (mensagem de play/seek com rating key + resposta de seek), documentando que iOS/Web não são suportados por limitação da Plex.

### 4. Endpoints ausentes
- `GET /api/status`: retorna último job de scan (do dicionário em memória, como já existe para scan) + métricas de processo lidas via `System.Diagnostics.Process` (CPU, RAM, rede, disco).
- `GET`/`POST /config`: lê/grava configurações expostas exclusivamente via variáveis de ambiente (POST apenas valida e retorna os valores atuais; não grava `app-config.json`, alinhado ao spec `config-env`).
- `GET /library/{section_key}`: consome a API do Plex para listar episódios da seção, retornando título, temporada, episódio e caminho de mídia resolvido.

### 5. CSRF protection
Adicionar middleware de CSRF (ou usar `Antiforgery` do ASP.NET Core) que exija token em rotas `POST`/`PUT`/`DELETE` sob `/api/`. O login (`POST /auth/login`) e o WebSocket permanecem sem CSRF (conforme já acontece hoje).

**Alternativa considerada:** cookie SameSite apenas. **Decisão:** CSRF token + SameSite, pois o README promete CSRF em rotas mutantes.

### 6. Métricas Prometheus reais
Substituir o endpoint `/metrics` hardcoded pela exportação real do OpenTelemetry Prometheus exporter, configurando `OTEL_EXPORTER` e `OTEL_ENDPOINT` no `OpenTelemetryService`. Manter o endpoint `/metrics` servindo a saída do exporter.

**Alternativa considerada:** biblioteca Prometheus.NET standalone. **Decisão:** reuso do OpenTelemetry já instalado, evitando nova dependência.

### 7. Variáveis de ambiente
Consumir no código .NET: `WS_PORT`, `LOG_LEVEL`, `ALLOWED_HOSTS`, `APP_CONFIG_PATH` (documentado como não lido), `OTEL_EXPORTER`, `OTEL_ENDPOINT`, `CUSTOM_JSON_PATH` (já consumido), `MEDIA_ROOT` (novo consumo para resolução de caminho). Manter prioridade: env var > default hardcoded > sem `app-config.json`.

## Risks / Trade-offs

- [Escaneamento de muitos episódios pode demorar] → o scan roda em background job com cancelamento; progresso via SSE; usuário pode cancelar.
- [ffprobe pode não estar disponível em todos os ambientes de execução] → o Dockerfile já instala ffprobe; documentar como pré-requisito para execução fora do container.
- [custom.json escrito por múltiplos concorrentes (scanner + API)] → usar gravação atômica (write to temp + rename) e lock simples para evitar corrupção.
- [CSRF pode quebrar clientes existentes que não enviam token] → documentar que rotas mutantes exigem CSRF; testes E2E devem ser atualizados para incluir o token.
- [WSS na 8081 vs ws:// documentado no README] → corrigir README para `wss://` e ajustar clientes Plex que se conectam via WSS.

## Migration Plan

- Implantar como nova imagem do container; volume `plex-data` preserva SQLite e `custom.json` existentes.
- Nenhuma migração de banco necessária (tabela `Markers` já existe).
- Rollback: voltar à imagem anterior; os specs e o código antigo não gravam `custom.json` via API, então arquivos gerados pelo novo scanner permanecem válidos para o PlexAutoSkip antigo (que só lê).

## Open Questions

- Qual o formato exato das mensagens de controle do protocolo Plex Companion que o WebSocket deve interpretar? (Pode ser resolvido durante a implementação sem alterar os specs.)
- O endpoint `/config` via POST deve apenas retornar valores atuais ou permitir alterar runtime? (Decisão: apenas leitura/validação, alinhado ao spec `config-env` que proíbe `app-config.json`.)
