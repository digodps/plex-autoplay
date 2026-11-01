# Design: teste-aceitacao

## Context

A aplicação sobe como container único (.NET 10 + Vue SPA + SQLite) via `docker compose`. `scripts/start.sh` compila, publica, sobe o container e aguarda `https://localhost:8443/login` responder. Os testes E2E existentes usam `docker-compose.test.yml` com Plex mockado, portanto não validam a configuração real do usuário nem o caminho oficial de start.

A autenticação é feita por `POST /auth/login` com `ADMIN_PASSWORD`; a resposta devolve `token` JWT e define o cookie `auth_token`. Rotas `/api/*` exigem `Authorization: Bearer <token>` ou o cookie. O PlexAutoSkip expõe WebSocket em `wss://localhost:8081/ws`.

## Goals / Non-Goals

**Goals:**

- Validar a stack real iniciada por `scripts/start.sh`.
<<<<<<< HEAD
- Exercitar cada função exposta pelo sistema com uma chamada dedicada.
- Reprovar a tarefa no primeiro erro crítico e continuar coletando os demais resultados quando possível.
- Ser executável localmente sem dependências além de `bash`, `curl`, `python3` e Docker.
=======
- Exercitar a interface com Playwright em navegador real (Chromium), como o usuário faz.
- Reprovar a tarefa no primeiro erro crítico e continuar coletando os demais resultados quando possível.
- Ser executável localmente com `bash`, `curl`, `python3`, Docker, Node.js e o Chromium do Playwright.
>>>>>>> origin/main
- Limpar os dados temporários criados pelo próprio teste.

**Non-Goals:**

- Substituir os testes E2E Playwright com Plex mockado.
<<<<<<< HEAD
- Validar interface visual, acessibilidade ou navegador real.
=======
- Validar aparência pixel a pixel, acessibilidade formal ou múltiplos navegadores.
>>>>>>> origin/main
- Testar cenários de carga, concorrência ou recuperação de desastre.
- Alterar endpoints, regras de negócio ou configuração da aplicação.

## Decisions

**1. Script shell único em `scripts/acceptance-test.sh`**

Rationale: o fluxo já é shell (`start.sh`, `stop.sh`, `logs.sh`) e `curl` cobre HTTP/HTTPS/SSE/WebSocket handshake sem adicionar dependências. Python 3 já é usado pelos scripts do repositório para ler JSON.

<<<<<<< HEAD
Alternativa considerada: nova suíte Playwright. Rejeitada para este escopo porque o teste precisa validar a stack real e não a interface; Playwright adicionaria tempo e complexidade sem cobrir melhor os endpoints.
=======
Alternativa considerada: nova suíte Playwright para tudo. Rejeitada para o escopo de API: `curl` já cobre status, JSON e SSE sem infraestrutura extra. Playwright fica restrito aos fluxos de interface em `tests/e2e/tests/acceptance-real.spec.js`.
>>>>>>> origin/main

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

<<<<<<< HEAD
=======
**7. Interface exercitada por navegador real (Playwright)**

Rationale: `curl` não valida renderização, formulários nem navegação SPA. As funções que o usuário acessa pela interface (login, dashboard, configurações, bibliotecas, séries, editor de marcadores, sincronização) passam a ser exercitadas com Chromium real contra a stack iniciada por `start.sh`. `curl` permanece para contratos sem UI: status, métricas, configuração, tema, CRUD de marcadores, Plex, scan/SSE e WebSocket.

Consequência: o executor passa a depender de Node.js, das dependências de `tests/e2e` e do navegador Chromium do Playwright. Ausência do navegador reprova com instrução de instalação (`npx playwright install chromium`).

**8. Spec de aceitação dedicado**

Rationale: `tests/e2e/tests/acceptance-real.spec.js` com `acceptance.config.js` próprio evita executar os specs do compose de teste (Plex mockado) contra a stack real. Dados ausentes do Plex (bibliotecas ou séries) continuam virando `SKIP`.

>>>>>>> origin/main
## Risks / Trade-offs

[O teste exige Plex real configurado no `.env`] → Mitigar: falhas de conexão e respostas 502 reprovam com mensagem apontando `PLEX_URL`/`PLEX_TOKEN`; dados ausentes viram `SKIP`.

[O scan pode consumir CPU e I/O] → Mitigar: iniciar e cancelar imediatamente; não aguardar varredura completa.

[O container pode já estar em execução] → Mitigar: `start.sh` é idempotente (`docker compose up -d`) e recompila/publica antes de subir.

[O teste grava marcador temporário no volume `/data`] → Mitigar: usa `RatingKey` único com timestamp e remove o marcador ao final, inclusive em caso de falha.

<<<<<<< HEAD
[Certificado autoassinado] → Mitigar: todas as chamadas usam `curl -k`.
=======
[Certificado autoassinado] → Mitigar: todas as chamadas usam `curl -k` e o Playwright usa `ignoreHTTPSErrors`.

[Interface pode oscilar por causa de animações ou latência] → Mitigar: seletores estáveis (`h1`, `.library-card`, `.marker-row`), timeouts explícitos e `workers: 1` na config de aceitação.

[Dependência de Node.js e do Chromium do Playwright] → Mitigar: o executor instala `tests/e2e/node_modules` quando ausente e reprova com instrução de `npx playwright install chromium` quando o navegador falta.
>>>>>>> origin/main

## Migration Plan

1. Criar `scripts/acceptance-test.sh` com as verificações e o relatório.
<<<<<<< HEAD
2. Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo de uso.
3. Executar `python3 scripts/sync-agent-configs.py`.
4. Validar as cópias em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.
5. Rollback: remover o script e a skill; nenhuma alteração de runtime é necessária.
=======
2. Criar `tests/e2e/acceptance.config.js` e `tests/e2e/tests/acceptance-real.spec.js`.
3. Criar `.agent-source/skills/teste-aceitacao/SKILL.md` com o fluxo de uso.
4. Executar `python3 scripts/sync-agent-configs.py`.
4. Validar as cópias em `.agents`, `.claude`, `.continue`, `.opencode`, `.github` e `.dsh`.
5. Validar a suíte de navegador: `(cd tests/e2e && npx playwright install chromium && npx playwright test --config=acceptance.config.js)`.
6. Rollback: remover o script, a config e o spec de aceitação; nenhuma alteração de runtime é necessária.
>>>>>>> origin/main

## Open Questions

Nenhuma.
