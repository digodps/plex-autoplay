# Spec — Correção de tela preta na sincronização (scan)

## Contexto

Na tela de Sincronização (`/sync`), o usuário inicia um escaneamento clicando em "Iniciar Escaneamento". O frontend chama `POST /api/plex/scan`, recebe um `job_id` e conecta um `EventSource` (SSE) em `/api/sync/log/stream/{jobId}` para atualizar status e logs em tempo real.

Hoje a tela fica preta após iniciar o escaneamento. O status não sai de `idle`, o console não é preenchido e a UI deixa de ser renderizada.

## Requisitos funcionais

### RF1 — Iniciar escaneamento
Ao clicar no botão "Iniciar Escaneamento" (quando `status !== 'running'`):
- Deve chamar `POST /api/plex/scan`.
- Ao obter `job_id`, deve armazená-lo e conectar o SSE em `/api/sync/log/stream/{jobId}`.
- O `status` deve passar para `running`.

### RF2 — Receber logs/status via SSE
O `EventSource` deve:
- Receber mensagens `data:` com `{ status, processed, skipped, errors, logs }`.
- Atualizar `status`, `processed`, `skipped`, `errors` reativamente.
- Acrescentar novos `logEntries` ao console (máximo 100).
- Ao encerrar (`completed`/`failed`/`cancelled`), fechar o `EventSource` e manter o `status` final.

### RF3 — Tratamento de erro do SSE
- `eventSource.onerror` deve fechar o `EventSource` e definir `status = "failed"` com feedback visível.
- Não deve deixar a tela em branco/preta silenciosamente.

### RF4 — Tratamento de erro HTTP 401/apiFetch
- Erros 401 em chamadas de API (`/api/plex/scan`) não devem redirecionar silenciosamente de forma que deixe a tela preta.
- Deve manter a UI funcional ou exibir mensagem de erro clara.

### RF5 — Tema aplicado (light/dark)
- Os componentes da tela de sincronização devem respeitar o tema configurado (classes `dark:` do Tailwind).
- Fundo, textos, bordas e botões devem renderizar corretamente tanto em light quanto em dark.

### RF6 — Menus/navegação funcionando
- Abertura, navegação, fechamento e estado ativo dos menus devem funcionar antes e depois do escaneamento.
- O breadcrumb e a top bar (`AppHeader`) devem continuar visíveis e funcionais.

### RF7 — Sem tela preta
- Após iniciar o escaneamento, a página `/sync` deve continuar renderizando o shell (`AppHeader` + `<main>` + `router-view`).
- Nenhuma exceção JavaScript não tratada deve quebrar o componente `SyncView`.

## Critérios de aceitação (QA)

- [ ] Build passa: `npm run build` (vue), `dotnet build -c Release`, `docker compose build`.
- [ ] `npm run lint` passa no `vue/plex-frontend`.
- [ ] `openspec validate fix-sync-black-screen --strict` passa.
- [ ] Acessar `http://localhost:<port>/sync` com Playwright:
  - Tema light/dark toggle aplicado corretamente.
  - Menus funcionando (abertura/navegação/fechamento/estado ativo).
  - Clicar "Iniciar Escaneamento" inicia job (status `running`) e não deixa tela preta.
- [ ] Relatório QA (`qa-report-1.md`) com `PASS` e URL de validação acessada.
