# Tasks

## 1. Corrigir container.spec.js (caminhos dos scripts)

- [x] 1.1 Atualizar caminhos de `./start.sh`, `./stop.sh`, `./logs.sh` para `scripts/start.sh`, `scripts/stop.sh`, `scripts/logs.sh` — verificar: `npx playwright test tests/e2e/tests/container.spec.js` não falha por "arquivo não encontrado"

## 2. Corrigir libraries.spec.js (seletores de UI)

- [x] 2.1 Atualizar seletores [data-testid="library-card"] para corresponder à aplicação .NET/Vue — verificar: teste de bibliotecas passa quando há bibliotecas no Plex

## 3. Corrigir markers.spec.js (formulário e endpoint)

- [x] 3.1 Atualizar seletores de formulário de criação de marcador para a aplicação .NET/Vue — verificar: teste de criação de marcador passa
- [x] 3.2 Verificar endpoint GET /api/markers na aplicação .NET — verificar: retorna 200 com array de marcadores

## 4. Corrigir images.spec.js (proxy de imagens)

- [x] 4.1 Investigar por que proxy de imagens retorna HTTP 500 — verificar: endpoint /api/images/thumb/{rating_key} não retorna 500

## 5. Verificação final end-to-end

- [x] 5.1 Rodar todos os 14 testes e2e com Plex real acessível — verificar: todos passam (14/14) com seletores/caminhos corrigidos para a stack .NET/Vue
- [x] 5.2 Confirmar que `docker compose -f docker-compose.test.yml down --remove-orphans` derruba o container após os testes — verificar: nenhum container permanece

## Workflow follow-up

- Archive a change após revisão dos testes corrigidos
- Verifique que o archived result mantém os testes funcionando
