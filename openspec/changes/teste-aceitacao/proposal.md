# Proposal: teste-aceitacao

## Why

O projeto possui testes E2E Playwright com Plex mockado, mas não possui um teste de aceitação que valide a stack real iniciada pelo usuário. Hoje é possível subir o container com `scripts/start.sh` e descobrir falhas apenas navegando manualmente ou executando testes que dependem de um ambiente paralelo. Isso deixa lacunas de configuração, autenticação, persistência, integração com Plex e exposição de endpoints.

## What Changes

- Criar a skill `teste-aceitacao`, invocável como `/teste-aceitacao`.
- Iniciar a aplicação real com `scripts/start.sh` antes de qualquer verificação.
- `scripts/acceptance-test.sh`: executor de aceitação (contratos via `curl` + interface via Playwright).
- `tests/e2e/acceptance.config.js`: configuração Playwright contra a stack real.
- `tests/e2e/tests/acceptance-real.spec.js`: fluxos de navegador real do usuário.
