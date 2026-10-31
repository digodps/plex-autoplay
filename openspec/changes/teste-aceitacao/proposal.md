# Proposal: teste-aceitacao

## Why

O projeto possui testes E2E Playwright com Plex mockado, mas não possui um teste de aceitação que valide a stack real iniciada pelo usuário. Hoje é possível subir o container com `scripts/start.sh` e descobrir falhas apenas navegando manualmente ou executando testes que dependem de um ambiente paralelo. Isso deixa lacunas de configuração, autenticação, persistência, integração com Plex e exposição de endpoints.

## What Changes

- Criar a skill `teste-aceitacao`, invocável como `/teste-aceitacao`.
- Iniciar a aplicação real com `scripts/start.sh` antes de qualquer verificação.
- Exercitar as funções do sistema uma a uma: autenticação, status, métricas, configuração, tema, CRUD de marcadores, bibliotecas, séries, episódios, scan/cancelamento de job, SSE/status, proxy de imagens e WebSocket do PlexAutoSkip.
- Acessar as funções de interface com Playwright em navegador real (Chromium): login, dashboard, configurações, bibliotecas, séries, editor de marcadores e sincronização, reproduzindo o que o usuário faz no navegador.
- Manter `curl` apenas para contratos sem interface: status, métricas, configuração, tema, CRUD de marcadores, Plex, scan/SSE e WebSocket.
- Considerar a tarefa **REPROVADA** se o start falhar, se qualquer verificação obrigatória retornar status inesperado, corpo inválido ou erro de conexão, ou se qualquer passo do navegador real falhar.
- Exibir relatório final com `PASS`, `SKIP` e `FAIL`; sair com código diferente de zero quando houver falha.

## Capabilities

### New Capabilities

- `teste-aceitacao`: execução automatizada de aceitação ponta a ponta sobre a stack real, com reprovação em qualquer erro.

### Modified Capabilities

Nenhuma. A mudança adiciona ferramenta de validação e não altera comportamento de endpoints existentes.

## Impact

- `scripts/acceptance-test.sh`: executor de aceitação (contratos via `curl` + interface via Playwright).
- `tests/e2e/acceptance.config.js`: configuração Playwright contra a stack real.
- `tests/e2e/tests/acceptance-real.spec.js`: fluxos de navegador real do usuário.
- `.agent-source/skills/teste-aceitacao/SKILL.md`: instruções da skill.
- `.agents`, `.claude`, `.continue`, `.opencode`, `.github`, `.dsh`: cópias sincronizadas da skill.
- `openspec/changes/teste-aceitacao/`: artefatos de planejamento.
- O teste grava e remove um marcador temporário no SQLite; não altera dados permanentes além do job de scan iniciado e cancelado.
