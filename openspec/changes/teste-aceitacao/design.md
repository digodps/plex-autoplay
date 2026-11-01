# Design: teste-aceitacao

## Context

A aplicação sobe como container único (.NET 10 + Vue SPA + SQLite) via `docker compose`. `scripts/start.sh` compila, publica, sobe o container e aguarda `https://localhost:8443/login` responder. Os testes E2E existentes usam `docker-compose.test.yml` com Plex mockado, portanto não validam a configuração real do usuário nem o caminho oficial de start.

A autenticação é feita por `POST /auth/login` com `ADMIN_PASSWORD`; a resposta devolve `token` JWT e define o cookie `auth_token`. Rotas `/api/*` exigem `Authorization: Bearer <token>` ou o cookie. O PlexAutoSkip expõe WebSocket em `wss://localhost:8081/ws`.

## Goals / Non-Goals

**Goals:**

- Validar a stack real iniciada por `scripts/start.sh`.
2. Criar `tests/e2e/acceptance.config.js` e `tests/e2e/tests/acceptance-real.spec.js`.
3. Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo de uso.
4. Executar `python3 scripts/sync-agent-configs.py`.
4. Validar as cópias em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.
5. Validar a suíte de navegador: `(cd tests/e2e && npx playwright install chromium && npx playwright test --config=acceptance.config.js)`.
6. Rollback: remover o script, a config e o spec de aceitação; nenhuma alteração de runtime é necessária.
