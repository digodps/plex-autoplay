# Tasks: Otimizar Docker Compose e Variáveis de Ambiente

- [x] 1.1 Limpar `.env` (remover variáveis não utilizadas: OTEL_*, ALLOWED_HOSTS, HTTPS_PORT, WS_PORT, SYNC_INTERVAL_MINUTES).
- [x] 1.2 Atualizar `infra/docker-compose.yml` e `infra/.env.example` para refletir apenas as variáveis ativas.
- [x] 1.3 Ajustar `scripts/start.sh` para validar apenas as variáveis estritamente obrigatórias (`PLEX_URL`, `PLEX_TOKEN`, `MEDIA_ROOTS`, `JWT_SECRET`, `ADMIN_PASSWORD`).
- [x] 1.4 Validar build e execução com `./scripts/start.sh --build` e testes.
- [x] 1.5 Validar `openspec validate otimizar-docker-compose --strict`.
