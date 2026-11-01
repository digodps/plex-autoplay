# Tasks: teste-aceitacao

## 1. Artefatos OpenSpec

- [x] 1.1 Criar a change `teste-aceitacao` com `proposal.md`, `design.md` e `tasks.md`.
- [x] 1.2 Marcar `skip_specs: true` no `.openspec.yaml`, pois a mudança é de ferramenta/processo e não altera specs de comportamento.

## 2. Executor de aceitação

- [x] 2.1 Criar `scripts/acceptance-test.sh` que valida `.env`, executa `scripts/start.sh` e aborta com `REPROVADO` se o start falhar.
- [x] 2.2 Implementar login: `GET /auth/login`, senha incorreta `401`, senha correta `200`, token JWT e cookie `auth_token`.
- [x] 2.3 Implementar verificações de proteção: `GET /api/status` sem token deve retornar `401`.
- [x] 2.4 Implementar verificações de monitoramento: `/api/status`, `/metrics`, `/api/observability/summary`.
- [x] 2.5 Implementar verificações de configuração e tema: `GET/POST /config`, `GET/PUT /api/theme` (válido `204`, inválido `400`).
- [x] 2.6 Implementar CRUD de marcadores: listar, criar `201`, duplicado `409`, consultar, editar `200`, remover `200` e confirmar remoção.
- [x] 2.7 Implementar integração Plex: bibliotecas, episódios por biblioteca, séries, episódios por série e proxy de imagem quando houver dados.
- [x] 2.8 Implementar scan: iniciar job, consultar status, cancelar job e tratar job concluído antes do cancelamento.
- [x] 2.9 Implementar verificação de WebSocket: handshake `101` em `wss://localhost:8081/ws`.
- [x] 2.10 Exibir relatório `PASS/SKIP/FAIL` e sair com código `1` quando `FAIL > 0`.

## 3. Interface em navegador real (Playwright)

- [x] 3.1 Criar `tests/e2e/acceptance.config.js` apontando para `ACCEPTANCE_BASE_URL` (padrão `https://localhost:8443`), com `ignoreHTTPSErrors` e `workers: 1`.
- [x] 3.2 Criar `tests/e2e/tests/acceptance-real.spec.js` com login (senha incorreta e correta + cookie `auth_token`), dashboard, configurações, bibliotecas/séries/editor de marcadores e sincronização via UI.
- [x] 3.3 Marcar `SKIP` quando o Plex não tiver bibliotecas ou séries; reprovar em erro de UI ou job `failed`.
- [x] 3.4 Integrar a execução do Playwright ao `scripts/acceptance-test.sh`, convertendo cada resultado em `PASS`/`SKIP`/`FAIL`.
- [x] 3.5 Reprovar com instrução explícita quando o Chromium do Playwright não estiver instalado.

## 4. Skill e sincronização

- [x] 4.1 Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo, interpretação do relatório e regra de reprovação.
- [x] 4.2 Executar `python3 scripts/sync-agent-configs.py`.
- [x] 4.3 Validar que a skill existe em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.

## 5. Verificação

- [x] 5.1 Executar `bash -n scripts/acceptance-test.sh` e confirmar sintaxe válida.
- [x] 5.2 Executar `python3 scripts/create-skill.py teste-aceitacao --force` e confirmar sincronização sem erros.
- [x] 5.3 Executar `node --check` em `acceptance.config.js` e no spec de aceitação.
- [x] 5.4 Instalar o Chromium: `(cd tests/e2e && npm install && npx playwright install chromium)`.
- [x] 5.5 Executar `./scripts/acceptance-test.sh` em ambiente com `.env` e Plex configurados e exigir `RESULTADO: APROVADO`.

> Execução final de 2026-11-01: `PASS=40 SKIP=0 FAIL=0` → `RESULTADO: APROVADO`.
> Correções aplicadas para chegar ao aprovado:
> - `GET /library/{sectionKey}` passou a consultar `/library/sections/{sectionKey}/all?type=4` (o Plex devolvia 404 em `/items`).
> - Parsing tolera `MediaContainer` sem `Metadata` (bibliotecas vazias) sem lançar exceção.
> - `SqliteSeriesRepository` sempre define `@Query` e conta o total com o mesmo filtro (evitava `500`).
> - Login devolve o token no corpo e o cookie `auth_token` é legível pelo frontend (HttpOnly bloqueava o guarda do Vue).
> - `apiFetch` não redireciona para `/login` quando já está nele (evitava loop de redirect).
> - Clique na série navega via `router.push`; bundle do `wwwroot` regenerado a partir de `vue/plex-frontend`.
