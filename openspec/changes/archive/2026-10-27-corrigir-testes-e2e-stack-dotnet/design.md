# Design

## Context

Os testes e2e (Playwright) em `tests/e2e/tests/` foram escritos para a stack antiga (Python/Flask). O projeto agora usa .NET 10 + Vue 3. Os seletores de UI, caminhos dos scripts e endpoints da API não correspondem mais à stack atual. Veja `proposal.md` para a motivação completa.

## Goals / Non-Goals

**Goals:**
- Corrigir `container.spec.js`: caminhos para `scripts/start.sh`, `scripts/stop.sh`, `scripts/logs.sh`
- Corrigir `libraries.spec.js`: seletores de UI para bibliotecas na aplicação .NET/Vue
- Corrigir `markers.spec.js`: formulário de criação de marcador e endpoint `GET /api/markers`
- Corrigir `images.spec.js`: proxy de imagens não retorna 500

**Non-Goals:**
- Alterar requisitos de comportamento do spec `e2e-integration`
- Adicionar novos testes e2e
- Modificar a aplicação .NET para atender aos testes (os testes devem se adaptar à aplicação)

## Decisions

### Atualizar caminhos nos testes
Alterar `./start.sh`, `./stop.sh`, `./logs.sh` para `scripts/start.sh`, `scripts/stop.sh`, `scripts/logs.sh` no `container.spec.js`.

### Usar seletores genéricos e baseURL configurável
Os testes e2e devem usar `PLAYWRIGHT_BASE_URL` e seletores que correspondam à aplicação .NET/Vue real, em vez de seletores da stack Flask antiga.

## Risks / Trade-offs

[Endpoints da API .NET podem não existir exatamente como esperado] → Se `GET /api/markers` retornar 500, o problema está na aplicação, não no teste. O teste deve apenas validar o contrato esperado.
