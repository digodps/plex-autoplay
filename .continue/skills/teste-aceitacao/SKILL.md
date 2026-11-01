---
name: teste-aceitacao
- [x] 4.1 Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo, interpretação do relatório e regra de reprovação.
- [x] 4.2 Executar `python3 scripts/sync-agent-configs.py`.
- [x] 4.3 Validar que a skill existe em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.

## 5. Verificação

- [x] 5.1 Executar `bash -n scripts/acceptance-test.sh` e confirmar sintaxe válida.
- [x] 5.2 Executar `python3 scripts/create-skill.py teste-aceitacao --force` e confirmar sincronização sem erros.
- [x] 5.3 Executar `node --check` em `acceptance.config.js` e no spec de aceitação.
- [ ] 5.4 Instalar o Chromium: `(cd tests/e2e && npm install && npx playwright install chromium)`.
- [ ] 5.5 Executar `./scripts/acceptance-test.sh` em ambiente com `.env` e Plex configurados e exigir `RESULTADO: APROVADO`.

> Execução de 2026-10-31: `PASS=35 SKIP=1 FAIL=2` → `RESULTADO: REPROVADO`. Falhas reais encontradas:
> - `GET /library/6` retorna `502`; o Plex responde `404` em `/library/sections/6/items` (a API do Plex expõe `/all`).
> - `POST /api/plex/scan` conclui com `errors=3`, mesmo problema de endpoint de episódios.
>
> O executor está funcionando; a reprovação é do sistema testado.
