---
name: teste-aceitacao
description: Teste de aceitação ponta a ponta: inicia container via start.sh, exercita cada função do sistema e reprova em qualquer erro.
allowed-tools: Bash(*)
license: MIT
metadata:
  author: plex-autoplay
  version: "1.0"
---

# Skill `teste-aceitacao` (`/teste-aceitacao`)

Teste de aceitação ponta a ponta: inicia container via start.sh, exercita cada função do sistema e reprova em qualquer erro.

## Uso

```bash
./scripts/acceptance-test.sh
```

Execute a partir da raiz do repositório. O script:

1. Valida `.env` e executa `./scripts/start.sh`.
2. Obtém o token JWT em `POST /auth/login` e verifica o cookie `auth_token`.
3. Chama cada função do sistema, uma a uma, com `curl` e `curl -k`.
4. Mostra `PASS`, `SKIP` e `FAIL` por verificação.
5. Retorna `RESULTADO: APROVADO` somente com `FAIL=0`; qualquer falha retorna `RESULTADO: REPROVADO` e código de saída `1`.

## Funções verificadas

| Área | Verificações |
|------|--------------|
| Start | `scripts/start.sh`, aplicação respondendo em `https://localhost:8443/login` |
| Auth | `GET /auth/login`, senha incorreta `401`, login correto `200`, token JWT, cookie `auth_token`, `401` sem token |
| Monitoramento | `/api/status`, `/metrics`, `/api/observability/summary` |
| Configuração | `GET/POST /config` |
| Tema | `GET /api/theme`, `PUT /api/theme` válido `204`, inválido `400` |
| Marcadores | listar, criar `201`, duplicado `409`, consultar, editar `200`, inexistente `404`, remover `200` e confirmar remoção |
| Plex | bibliotecas, episódios por biblioteca, catálogo de séries, episódios por série, proxy de imagem |
| Scan | iniciar job, status, SSE, cancelar; job `failed` reprova |
| SPA | `/login`, `/dashboard`, `/libraries`, `/settings`, `/sync` |
| WebSocket | handshake `101` em `https://localhost:8081/ws` |

## Regra de reprovação

- Não declare a tarefa concluída sem a linha `RESULTADO: APROVADO` e código de saída `0`.
- `FAIL > 0`, timeout de start, erro de conexão, status HTTP inesperado, job com `status = failed` ou `errors > 0` significam **REPROVADO**.
- `SKIP` de dados dependentes do Plex (sem bibliotecas, séries ou thumb) não reprova.
- Em caso de falha, mostre ao usuário a linha `FAIL`, os logs relevantes e o comando de diagnóstico `./scripts/logs.sh --no-follow --tail 100`.

## Pré-requisitos

- Docker disponível e `.env` com `PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD` e `MEDIA_ROOT`.
- Plex acessível com token de escrita para validar bibliotecas e séries.
- O certificado é autoassinado; o script já usa `curl -k`.

## Observações

- O script é idempotente: `scripts/start.sh` recompila, publica e sobe o container.
- O container permanece em execução ao final; use `./scripts/stop.sh` para parar.
- O marcador temporário é removido pelo `trap` mesmo em caso de falha.

## Fonte

- `.agent-source/skills/teste-aceitacao/SKILL.md`
- Material OpenSpec anexado ao final desta skill.
- Após edição, execute `python3 scripts/sync-agent-configs.py`.

## OpenSpec Material

## proposal.md
# Proposal: teste-aceitacao

## Why

O projeto possui testes E2E Playwright com Plex mockado, mas não possui um teste de aceitação que valide a stack real iniciada pelo usuário. Hoje é possível subir o container com `scripts/start.sh` e descobrir falhas apenas navegando manualmente ou executando testes que dependem de um ambiente paralelo. Isso deixa lacunas de configuração, autenticação, persistência, integração com Plex e exposição de endpoints.

## What Changes

- Criar a skill `teste-aceitacao`, invocável como `/teste-aceitacao`.
- Iniciar a aplicação real com `scripts/start.sh` antes de qualquer verificação.
- Exercitar as funções do sistema uma a uma: autenticação, status, métricas, configuração, tema, CRUD de marcadores, bibliotecas, séries, episódios, scan/cancelamento de job, SSE/status, proxy de imagens, rotas SPA e WebSocket do PlexAutoSkip.
- Reproduzir cada chamada com `curl` usando o token JWT obtido no login.
- Considerar a tarefa **REPROVADA** se o start falhar ou se qualquer verificação obrigatória retornar status inesperado, corpo inválido ou erro de conexão.
- Exibir relatório final com `PASS`, `SKIP` e `FAIL`; sair com código diferente de zero quando houver falha.

## Capabilities

### New Capabilities

- `teste-aceitacao`: execução automatizada de aceitação ponta a ponta sobre a stack real, com reprovação em qualquer erro.

### Modified Capabilities

Nenhuma. A mudança adiciona ferramenta de validação e não altera comportamento de endpoints existentes.

## Impact

- `scripts/acceptance-test.sh`: novo executor de aceitação.
- `.agent-source/skills/teste-aceitacao/SKILL.md`: instruções da skill.
- `.agents`, `.claude`, `.continue`, `.opencode`, `.github`, `.dsh`: cópias sincronizadas da skill.
- `openspec/changes/teste-aceitacao/`: artefatos de planejamento.
- O teste grava e remove um marcador temporário no SQLite; não altera dados permanentes além do job de scan iniciado e cancelado.


## design.md
# Design: teste-aceitacao

## Context

A aplicação sobe como container único (.NET 10 + Vue SPA + SQLite) via `docker compose`. `scripts/start.sh` compila, publica, sobe o container e aguarda `https://localhost:8443/login` responder. Os testes E2E existentes usam `docker-compose.test.yml` com Plex mockado, portanto não validam a configuração real do usuário nem o caminho oficial de start.

A autenticação é feita por `POST /auth/login` com `ADMIN_PASSWORD`; a resposta devolve `token` JWT e define o cookie `auth_token`. Rotas `/api/*` exigem `Authorization: Bearer <token>` ou o cookie. O PlexAutoSkip expõe WebSocket em `wss://localhost:8081/ws`.

## Goals / Non-Goals

**Goals:**

- Validar a stack real iniciada por `scripts/start.sh`.
- Exercitar cada função exposta pelo sistema com uma chamada dedicada.
- Reprovar a tarefa no primeiro erro crítico e continuar coletando os demais resultados quando possível.
- Ser executável localmente sem dependências além de `bash`, `curl`, `python3` e Docker.
- Limpar os dados temporários criados pelo próprio teste.

**Non-Goals:**

- Substituir os testes E2E Playwright com Plex mockado.
- Validar interface visual, acessibilidade ou navegador real.
- Testar cenários de carga, concorrência ou recuperação de desastre.
- Alterar endpoints, regras de negócio ou configuração da aplicação.

## Decisions

**1. Script shell único em `scripts/acceptance-test.sh`**

Rationale: o fluxo já é shell (`start.sh`, `stop.sh`, `logs.sh`) e `curl` cobre HTTP/HTTPS/SSE/WebSocket handshake sem adicionar dependências. Python 3 já é usado pelos scripts do repositório para ler JSON.

Alternativa considerada: nova suíte Playwright. Rejeitada para este escopo porque o teste precisa validar a stack real e não a interface; Playwright adicionaria tempo e complexidade sem cobrir melhor os endpoints.

**2. `start.sh` como única forma de subir a aplicação**

Rationale: é o caminho documentado no README e valida `.env`, build, publish e `docker compose` exatamente como o usuário faria.

Alternativa considerada: `docker compose up -d` direto. Rejeitada: pularia a validação de `.env`, compilação e publish que fazem parte da aceitação.

**3. Token JWT no header `Authorization`**

Rationale: evita depender de cookie jar para todas as chamadas e reproduz o contrato usado pelos clientes. O cookie também é verificado no login.

**4. Falha acumulada com código de saída não zero**

Rationale: o relatório mostra todos os problemas em uma execução, mas o resultado final é binário: aprovado somente com `FAIL=0`.

**5. Scan do Plex: iniciar e cancelar**

Rationale: uma varredura completa pode durar horas em bibliotecas grandes. O teste valida os endpoints `POST /api/plex/scan`, `GET /api/sync/status/{jobId}` e `POST /api/sync/cancel/{jobId}` sem bloquear a aceitação. Se o job terminar antes do cancelamento, o status final é verificado e aceito somente se não for `failed`.

**6. Dados dependentes do Plex**

Rationale: bibliotecas, séries, episódios e imagens só existem se o Plex estiver configurado e acessível. Quando a listagem de bibliotecas retorna vazia, as verificações dependentes são marcadas como `SKIP`, não como falha. Erros HTTP ou jobs `failed` continuam reprovando.

## Risks / Trade-offs

[O teste exige Plex real configurado no `.env`] → Mitigar: falhas de conexão e respostas 502 reprovam com mensagem apontando `PLEX_URL`/`PLEX_TOKEN`; dados ausentes viram `SKIP`.

[O scan pode consumir CPU e I/O] → Mitigar: iniciar e cancelar imediatamente; não aguardar varredura completa.

[O container pode já estar em execução] → Mitigar: `start.sh` é idempotente (`docker compose up -d`) e recompila/publica antes de subir.

[O teste grava marcador temporário no volume `/data`] → Mitigar: usa `RatingKey` único com timestamp e remove o marcador ao final, inclusive em caso de falha.

[Certificado autoassinado] → Mitigar: todas as chamadas usam `curl -k`.

## Migration Plan

1. Criar `scripts/acceptance-test.sh` com as verificações e o relatório.
2. Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo de uso.
3. Executar `python3 scripts/sync-agent-configs.py`.
4. Validar as cópias em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.
5. Rollback: remover o script e a skill; nenhuma alteração de runtime é necessária.

## Open Questions

Nenhuma.


## tasks.md
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

- [ ] 4.1 Executar `bash -n scripts/acceptance-test.sh` e confirmar sintaxe válida.
- [ ] 4.2 Executar `python3 scripts/create-skill.py teste-aceitacao --force` e confirmar sincronização sem erros.
- [ ] 4.3 Executar `./scripts/acceptance-test.sh` em ambiente com `.env` e Plex configurados e exigir `RESULTADO: APROVADO`.

