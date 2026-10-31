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
- [x] 2.9 Implementar verificações de SPA e WebSocket: rotas `/login`, `/dashboard`, `/libraries`, `/settings`, `/sync` e handshake `101` em `wss://localhost:8081/ws`.
- [x] 2.10 Exibir relatório `PASS/SKIP/FAIL` e sair com código `1` quando `FAIL > 0`.

## 3. Skill e sincronização

- [x] 3.1 Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo, interpretação do relatório e regra de reprovação.
- [x] 3.2 Executar `python3 scripts/sync-agent-configs.py`.
- [x] 3.3 Validar que a skill existe em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.

## 4. Verificação

- [x] 4.1 Executar `bash -n scripts/acceptance-test.sh` e confirmar sintaxe válida.
- [x] 4.2 Executar `python3 scripts/create-skill.py teste-aceitacao --force` e confirmar sincronização sem erros.
- [ ] 4.3 Executar `./scripts/acceptance-test.sh` em ambiente com `.env` e Plex configurados e exigir `RESULTADO: APROVADO`.

> Execução de 2026-10-31: `PASS=35 SKIP=1 FAIL=2` → `RESULTADO: REPROVADO`. Falhas reais encontradas:
> - `GET /library/6` retorna `502`; o Plex responde `404` em `/library/sections/6/items` (a API do Plex expõe `/all`).
> - `POST /api/plex/scan` conclui com `errors=3`, mesmo problema de endpoint de episódios.
>
> O executor está funcionando; a reprovação é do sistema testado.
