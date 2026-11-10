# Deficiências Resolvidas — Goldfish Review (EGM)

## Revisão A — Arquitetura, Dependências e Integração

| # | Achado | Classificação | Justificativa |
|---|--------|---------------|---------------|
| A1 | `.env` contém variáveis não consumidas pela aplicação nem pelo compose: `OTEL_EXPORTER`, `OTEL_ENDPOINT`, `OTEL_SERVICE_NAME`, `ALLOWED_HOSTS`, `HTTPS_PORT`, `WS_PORT`, `SYNC_INTERVAL_MINUTES`, `AUTO_SKIP_AUTHORIZED`, `CERT_PATH`, `CERT_PASSWORD`, `CERT_BASE64`, `SETUP_TOKEN`, `TEST_MASTER_PASSWORD`, `LOG_RETENTION_DAYS`. | **Aceito** | Remover do `.env` (manter apenas no `.env.example` se documentação). Compose não as referencia; app lê com defaults seguros. |
| A2 | `scripts/start.sh` valida `MEDIA_ROOT` (legado) em vez de `MEDIA_ROOTS`. | **Aceito** | App prefere `MEDIA_ROOTS`; `MEDIA_ROOT` é fallback. Corrigir validação para `MEDIA_ROOTS`. |
| A3 | Compose `environment:` hardcodeia valores (`WEB_PORT`, `LOG_LEVEL`, `DISABLE_HTTP`, `SQLITE_CONNECTION_STRING`, `APP_CONFIG_PATH`) que duplicam `.env`. | **Parcial** | Hardcode no compose garante runtime previsível; `.env` continua como seed de defaults. Sem conflito prático. Manter como está. |
| A4 | `GIT_COMMIT`, `IMAGE_NAME`, `CONTAINER_NAME`, `WEB_HOST_PORT` são interpolados apenas no compose (build args, image name, container name, port mapping). | **Aceito** | Manter — são variáveis de infraestrutura, não de aplicação. |

## Revisão S — SOLID e Coesão

| # | Achado | Classificação | Justificativa |
|---|--------|---------------|---------------|
| S1 | Variáveis espalhadas entre `.env`, `docker-compose.yml`, `scripts/start.sh` sem fonte única de verdade. | **Aceito** | Consolidar conjunto mínimo obrigatório. |
| S2 | Duas chaves para mídia: `MEDIA_ROOT` (legado) e `MEDIA_ROOTS` (preferida). | **Aceito** | Preferir `MEDIA_ROOTS` em validação; manter `MEDIA_ROOT` apenas como fallback silencioso na aplicação. |

## Revisão C — Clean Code, Escopo e Testabilidade

| # | Achado | Classificação | Justificativa |
|---|--------|---------------|---------------|
| C1 | `start.sh` valida conjunto rígido sem distinguir obrigatório vs opcional. | **Aceito** | Reduzir validação a estritamente obrigatório: `PLEX_URL`, `PLEX_TOKEN`, `MEDIA_ROOTS`, `JWT_SECRET`, `ADMIN_PASSWORD`. |
| C2 | `README.md` documenta `ALLOWED_HOSTS` como configuração, mas aplicação nunca a lê. | **Parcial** | Remover do `.env`; manter no `.env.example` e README como opcional/documentado. |
| C3 | `HTTPS_PORT` e `WS_PORT` aparecem apenas em comentários no `KestrelSetup.cs` e não são lidos em tempo de execução. | **Aceito** | Remover do `.env` (não usados). |
| C4 | Variáveis opcionais com defaults seguros (`OTEL_*`, `CERT_*`, `SETUP_TOKEN`, `TEST_MASTER_PASSWORD`, `LOG_RETENTION_DAYS`, `SYNC_INTERVAL_MINUTES`, `AUTO_SKIP_AUTHORIZED`) poluem o `.env` sem necessidade de configuração explícita. | **Aceito** | Remover do `.env`; defaults no código já cobrem. |

## Ações Consolidadas (Aceitas)
1. `.env` → manter apenas: `PLEX_URL`, `PLEX_TOKEN`, `MEDIA_ROOTS`, `JWT_SECRET`, `ADMIN_PASSWORD`, `WEB_PORT`, `DISABLE_HTTP`, `SQLITE_CONNECTION_STRING`, `APP_CONFIG_PATH`, `LOG_LEVEL`.
2. `docker-compose.yml` → `env_file` continua carregando `.env`; `environment:` hardcoded inalterado.
3. `scripts/start.sh` → validar apenas 5 variáveis obrigatórias; remover `MEDIA_ROOT` da validação; aceitar `MEDIA_ROOTS` como obrigatória.
4. `.env.example` / `README.md` → manter variáveis opcionais/documentadas se necessário.