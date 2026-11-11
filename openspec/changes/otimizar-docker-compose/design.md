# Design: Otimizar Docker Compose e Variáveis de Ambiente

## 1. Mapeamento de Uso de Variáveis
- **Utilizadas pela aplicação (.NET) e Compose:**
  - `PLEX_URL`: URL do Plex (seed / app)
  - `PLEX_TOKEN`: Token do Plex (seed / app)
  - `MEDIA_ROOTS` (com fallback legado `MEDIA_ROOT`): Diretórios de mídia
  - `JWT_SECRET`: Chave de autenticação JWT
  - `ADMIN_PASSWORD`: Senha do painel admin
  - `WEB_PORT`: Porta Kestrel
  - `DISABLE_HTTP`: Controle HTTP/HTTPS
  - `SQLITE_CONNECTION_STRING`: Banco SQLite
  - `APP_CONFIG_PATH`: Caminho do config.json
  - `LOG_LEVEL`: Nível de log
- **Removidas por desuso / irrelevância prática:**
  - `OTEL_EXPORTER`, `OTEL_ENDPOINT`, `OTEL_SERVICE_NAME`
  - `ALLOWED_HOSTS`
  - `HTTPS_PORT`, `WS_PORT`
  - `SYNC_INTERVAL_MINUTES`
  - `CERT_PATH`, `CERT_PASSWORD`, `CERT_BASE64` (podem ser opcionais, mas mantidos apenas se necessário; vamos simplificar removendo o lixo não utilizado)

## 2. Ajustes no `scripts/start.sh`
- Validar estritamente apenas: `PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`, e `MEDIA_ROOTS` (ou `MEDIA_ROOT`).
