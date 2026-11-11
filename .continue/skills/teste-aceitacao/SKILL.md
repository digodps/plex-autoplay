---
name: teste-aceitacao
description: Teste de aceitação ponta a ponta: inicia container via start.sh, exercita cada função do sistema (interface em navegador real via Playwright) e reprova em qualquer erro.
allowed-tools: Bash(*)
license: MIT
metadata:
  author: plex-autoplay
  version: "1.0"
---

# Skill `teste-aceitacao` (`/teste-aceitacao`)

Teste de aceitação ponta a ponta: inicia container via start.sh, exercita cada função do sistema (interface em navegador real via Playwright) e reprova em qualquer erro.

## Uso

```bash
./scripts/acceptance-test.sh
```

Execute a partir da raiz do repositório. O script:

1. Valida `.env` e executa `./scripts/start.sh`.
2. Obtém o token JWT em `POST /auth/login` e verifica o cookie `auth_token`.
3. Acessa as funções de interface com Playwright em navegador real (Chromium) contra `ACCEPTANCE_BASE_URL`.
4. Chama os contratos sem UI com `curl` e `curl -k`.
5. Mostra `PASS`, `SKIP` e `FAIL` por verificação.
6. Retorna `RESULTADO: APROVADO` somente com `FAIL=0`; qualquer falha retorna `RESULTADO: REPROVADO` e código de saída `1`.

## Funções verificadas

| Área | Verificações |
|------|--------------|
| Start | `scripts/start.sh`, aplicação respondendo em `https://localhost:8443/login` |
| Auth (API) | `GET /auth/login`, senha incorreta `401`, login correto `200`, token JWT, cookie `auth_token`, `401` sem token |
| Auth (navegador) | formulário de login, senha incorreta exibe erro, senha correta abre `/dashboard` e define cookie |
| Dashboard (navegador) | `h1` "Início", métricas visíveis, sem bloco de erro |
| Configurações (navegador) | salvar intervalo e ver mensagem de sucesso |
| Bibliotecas (navegador) | lista de bibliotecas, clique abre séries, clique na série abre o editor de marcadores |
| Sincronização (navegador) | iniciar scan pelo botão, status sai de `idle`, cancelar quando possível; `failed` reprova |
| Monitoramento | `/api/status`, `/metrics`, `/api/observability/summary` |
| Configuração | `GET/POST /config` |
| Tema | `GET /api/theme`, `PUT /api/theme` válido `204`, inválido `400` |
| Marcadores | listar, criar `201`, duplicado `409`, consultar, editar `200`, inexistente `404`, remover `200` e confirmar remoção |
| Plex | bibliotecas, episódios por biblioteca, catálogo de séries, episódios por série, proxy de imagem |
| Scan | iniciar job, status, SSE, cancelar; job `failed` reprova |
| WebSocket | handshake `101` em `https://localhost:8081/ws` |

## Regra de reprovação

- Não declare a tarefa concluída sem a linha `RESULTADO: APROVADO` e código de saída `0`.
- `FAIL > 0`, timeout de start, erro de conexão, status HTTP inesperado, passo de navegador com erro, job com `status = failed` ou `errors > 0` significam **REPROVADO**.
- `SKIP` de dados dependentes do Plex (sem bibliotecas, séries ou thumb) não reprova.
- Em caso de falha, mostre ao usuário a linha `FAIL`, os logs relevantes, os artefatos em `tests/e2e/test-results` e o comando de diagnóstico `./scripts/logs.sh --no-follow --tail 100`.

## Pré-requisitos

- Docker disponível e `.env` com `PLEX_URL`, `PLEX_TOKEN`, `JWT_SECRET`, `ADMIN_PASSWORD` e `MEDIA_ROOT`.
- Node.js + dependências de `tests/e2e` (`npm install`) e Chromium do Playwright (`npx playwright install chromium`).
- Plex acessível com token de escrita para validar bibliotecas e séries.
- O certificado é autoassinado; o script usa `curl -k` e o Playwright usa `ignoreHTTPSErrors`.

## Observações

- O script é idempotente: `scripts/start.sh` recompila, publica e sobe o container.
- A suíte de navegador fica em `tests/e2e/tests/acceptance-real.spec.js`, com config própria em `tests/e2e/acceptance.config.js` (não usa o compose de teste nem o Plex mockado).
- `ACCEPTANCE_BASE_URL` (padrão `https://localhost:8443`) é repassada como `PLAYWRIGHT` baseURL.
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


## design.md
# Design: teste-aceitacao

## Context

A aplicação sobe como container único (.NET 10 + Vue SPA + SQLite) via `docker compose`. `scripts/start.sh` compila, publica, sobe o container e aguarda `https://localhost:8443/login` responder. Os testes E2E existentes usam `docker-compose.test.yml` com Plex mockado, portanto não validam a configuração real do usuário nem o caminho oficial de start.

A autenticação é feita por `POST /auth/login` com `ADMIN_PASSWORD`; a resposta devolve `token` JWT e define o cookie `auth_token`. Rotas `/api/*` exigem `Authorization: Bearer <token>` ou o cookie. O PlexAutoSkip expõe WebSocket em `wss://localhost:8081/ws`.

## Goals / Non-Goals

**Goals:**

- Validar a stack real iniciada por `scripts/start.sh`.
- Exercitar a interface com Playwright em navegador real (Chromium), como o usuário faz.
- Reprovar a tarefa no primeiro erro crítico e continuar coletando os demais resultados quando possível.
- Ser executável localmente com `bash`, `curl`, `python3`, Docker, Node.js e o Chromium do Playwright.
- Limpar os dados temporários criados pelo próprio teste.

**Non-Goals:**

- Substituir os testes E2E Playwright com Plex mockado.
- Validar aparência pixel a pixel, acessibilidade formal ou múltiplos navegadores.
- Testar cenários de carga, concorrência ou recuperação de desastre.
- Alterar endpoints, regras de negócio ou configuração da aplicação.

## Decisions

**1. Script shell único em `scripts/acceptance-test.sh`**

Rationale: o fluxo já é shell (`start.sh`, `stop.sh`, `logs.sh`) e `curl` cobre HTTP/HTTPS/SSE/WebSocket handshake sem adicionar dependências. Python 3 já é usado pelos scripts do repositório para ler JSON.

Alternativa considerada: nova suíte Playwright para tudo. Rejeitada para o escopo de API: `curl` já cobre status, JSON e SSE sem infraestrutura extra. Playwright fica restrito aos fluxos de interface em `tests/e2e/tests/acceptance-real.spec.js`.

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

**7. Interface exercitada por navegador real (Playwright)**

Rationale: `curl` não valida renderização, formulários nem navegação SPA. As funções que o usuário acessa pela interface (login, dashboard, configurações, bibliotecas, séries, editor de marcadores, sincronização) passam a ser exercitadas com Chromium real contra a stack iniciada por `start.sh`. `curl` permanece para contratos sem UI: status, métricas, configuração, tema, CRUD de marcadores, Plex, scan/SSE e WebSocket.

Consequência: o executor passa a depender de Node.js, das dependências de `tests/e2e` e do navegador Chromium do Playwright. Ausência do navegador reprova com instrução de instalação (`npx playwright install chromium`).

**8. Spec de aceitação dedicado**

Rationale: `tests/e2e/tests/acceptance-real.spec.js` com `acceptance.config.js` próprio evita executar os specs do compose de teste (Plex mockado) contra a stack real. Dados ausentes do Plex (bibliotecas ou séries) continuam virando `SKIP`.

## Risks / Trade-offs

[O teste exige Plex real configurado no `.env`] → Mitigar: falhas de conexão e respostas 502 reprovam com mensagem apontando `PLEX_URL`/`PLEX_TOKEN`; dados ausentes viram `SKIP`.

[O scan pode consumir CPU e I/O] → Mitigar: iniciar e cancelar imediatamente; não aguardar varredura completa.

[O container pode já estar em execução] → Mitigar: `start.sh` é idempotente (`docker compose up -d`) e recompila/publica antes de subir.

[O teste grava marcador temporário no volume `/data`] → Mitigar: usa `RatingKey` único com timestamp e remove o marcador ao final, inclusive em caso de falha.

[Certificado autoassinado] → Mitigar: todas as chamadas usam `curl -k` e o Playwright usa `ignoreHTTPSErrors`.

[Interface pode oscilar por causa de animações ou latência] → Mitigar: seletores estáveis (`h1`, `.library-card`, `.marker-row`), timeouts explícitos e `workers: 1` na config de aceitação.

[Dependência de Node.js e do Chromium do Playwright] → Mitigar: o executor instala `tests/e2e/node_modules` quando ausente e reprova com instrução de `npx playwright install chromium` quando o navegador falta.

## Migration Plan

1. Criar `scripts/acceptance-test.sh` com as verificações e o relatório.
2. Criar `tests/e2e/acceptance.config.js` e `tests/e2e/tests/acceptance-real.spec.js`.
3. Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo de uso.
4. Executar `python3 scripts/sync-agent-configs.py`.
4. Validar as cópias em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.
5. Validar a suíte de navegador: `(cd tests/e2e && npx playwright install chromium && npx playwright test --config=acceptance.config.js)`.
6. Rollback: remover o script, a config e o spec de aceitação; nenhuma alteração de runtime é necessária.

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
- [x] 2.9 Implementar verificação de WebSocket: handshake `101` em `wss://localhost:8081/ws`.
- [x] 2.10 Exibir relatório `PASS/SKIP/FAIL` e sair com código `1` quando `FAIL > 0`.

## 3. Interface em navegador real (Playwright)

- [x] 3.1 Criar `tests/e2e/acceptance.config.js` apontando para `ACCEPTANCE_BASE_URL` (padrão `https://localhost:8443`), com `ignoreHTTPSErrors` e `workers: 1`.
- [x] 3.2 Criar `tests/e2e/tests/acceptance-real.spec.js` com login (senha incorreta e correta + cookie `auth_token`), dashboard, configurações, bibliotecas/séries/editor de marcadores e sincronização via UI.
- [x] 3.3 Marcar `SKIP` quando o Plex não tiver bibliotecas ou séries; reprovar em erro de UI ou job `failed`.
- [x] 3.4 Integrar a execução do Playwright ao `scripts/acceptance-test.sh`, convertendo cada resultado em `PASS`/`SKIP`/`FAIL`.
- [x] 3.5 Reprovar com instrução explícita quando o Chromium do Playwright não estiver instalado.

## 4. Skill e sincronização

- [x] 3.1 Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo, interpretação do relatório e regra de reprovação.
- [x] 3.2 Executar `python3 scripts/sync-agent-configs.py`.
- [x] 3.3 Validar que a skill existe em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.

## 4. Verificação

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

