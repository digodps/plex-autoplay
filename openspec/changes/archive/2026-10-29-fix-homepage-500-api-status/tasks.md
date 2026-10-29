# Tasks

## 1. Investigação da causa raiz

- [x] 1.1 Reproduzir o HTTP 500 localmente em `GET /api/status` e confirmar a linha que lança (inspecionar logs e exceção). Verification: consegue reproduzir o 500 e identificar o método/linha que lança.

## 2. Correção do backend e testes

- [x] 2.1 Envolver o handler de `GET /api/status` em `try/catch`, retornando HTTP 200 com dados parciais (service/version sempre presentes, `lastJob` null, métricas de processo zeradas) quando houver exceção. Verification: endpoint retorna 200 mesmo forçando exceção na leitura das métricas de processo.
- [x] 2.2 Adicionar log estruturado da exceção (nível Error com correlation ID) no bloco `catch`. Verification: exceção é gravada no log sem alterar o status HTTP 200.
- [x] 2.3 Adicionar teste unitário para `GET /api/status` (HTTP 200, formato da resposta, `lastJob` null quando não há jobs, métricas de processo ≥ 0) e executá-lo. Verification: `dotnet test` passa para o teste novo.
- [x] 2.4 Verificar o teste e2e existente de `GET /api/status` (`tests/e2e/tests/libraries.spec.js`) contra o endpoint corrigido. Verification: e2e de status passa ou compatibilidade confirmada.

## 3. Validação do plano de arquitetura

- [x] 3.1 Executar `openspec validate` para confirmar que a mudança está completa e consistente. Verification: comando passa sem erros.
- [x] 3.2 Executar build e suíte de testes do backend para confirmar ausência de regressões. Verification: `dotnet build` e `dotnet test` passam.

## Workflow follow-up

- Aplicar a mudança com `/opsx-apply` após revisão dos artifacts de planejamento.
- Arquivar a mudança após conclusão da implementação e revisão dos requisitos.
