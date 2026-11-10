# Tasks: Otimizar Docker Compose e Variáveis de Ambiente

- [ ] 1.1 Limpar `.env` (remover variáveis não utilizadas: OTEL_*, ALLOWED_HOSTS, HTTPS_PORT, WS_PORT, SYNC_INTERVAL_MINUTES).
- [ ] 1.2 Atualizar `infra/docker-compose.yml` e `infra/.env.example` (se existir) para refletir apenas as variáveis ativas.
- [ ] 1.3 Ajustar `scripts/start.sh` para validar apenas as variáveis estritamente obrigatórias (`PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD`, `MEDIA_ROOTS`).
- [ ] 1.4 Validar build e execução com `./scripts/start.sh --build` e testes.
- [ ] 1.5 Validar `openspec validate otimizar-docker-compose --strict`.
