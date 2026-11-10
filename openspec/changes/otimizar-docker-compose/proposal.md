# Proposal: Otimizar Docker Compose e Variáveis de Ambiente

## Why
O arquivo `.env` e o `docker-compose.yml` contêm diversas variáveis de ambiente obsoletas, especulativas ou não utilizadas pela aplicação ou pelo compose (como `OTEL_*`, `ALLOWED_HOSTS`, `HTTPS_PORT`, `WS_PORT`, `SYNC_INTERVAL_MINUTES`, etc.). Além disso, o script `start.sh` valida um conjunto rígido de variáveis, exigindo campos que podem ser opcionais ou legados. Precisamos limpar o que não é usado, manter apenas o que a aplicação e o compose utilizam, e ajustar o script de start para validar apenas o estritamente obrigatório.

## Changes
1. Remover variáveis não utilizadas do `.env` e do `infra/docker-compose.yml`.
2. Consolidar as variáveis essenciais usadas pela aplicação (`PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`, `MEDIA_ROOTS`, `WEB_PORT`, etc.).
3. Ajustar `scripts/start.sh` para validar apenas as variáveis verdadeiramente obrigatórias, sem falhar em opcionais.

## Capabilities
### Modified Capabilities
- `docker-compose`: remoção de variáveis órfãs/não usadas.
- `start-script`: validação restrita aos parâmetros mandatórios.
