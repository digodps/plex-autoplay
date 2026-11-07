# QA Report 1 — alteracao-senha-usuario

**Status:** PASS
**URL validada:** http://localhost:8096 (container `qa-ui`, imagem `plex-autoplay:alteracao-senha-usuario`, volume `/tmp/qa-ui-data`)
**Data:** 2026-11-07
**Método:** curl no backend + Playwright headless (Chromium) na UI final + inspeção do SQLite e dos logs do container.

## Ambiente

- Build: `dotnet build` e `dotnet test` (12 testes) PASS.
- `npm run lint` e `npm run build` PASS; bundle copiado para `src/PlexAutoplay.Web/wwwroot`.
- `docker build --no-cache -t plex-autoplay:alteracao-senha-usuario .` PASS.
- Container isolado com volume novo: `DISABLE_HTTP=false`, `JWT_SECRET` com ≥32 bytes.

## Evidências

### Token de setup (uso único)

- Primeiro start loga:
  - `CONFIGURACAO INICIAL - TOKEN DE SETUP (USO UNICO)`
  - `SETUP_TOKEN=<43 chars base64url>`
  - aviso: "O token deixa de valer apos a primeira conclusao do setup."
- `GET /api/auth/setup/status` → `{"setupRequired":true}` antes, `false` após.
- `POST /api/auth/setup` com token errado → `401`.
- `POST /api/auth/setup` com token correto → `204`.
- Reutilização do mesmo token → `409` ("Configuração inicial já concluída"); token também já invalidado em memória.
- Reinício após setup: nenhum `SETUP_TOKEN=` novo no log; login continua funcionando.
- Reinício antes do setup: token rotacionado (`t1 != t2`).

### Backend (curl)

- `POST /api/auth/login` senha criada → `200` com JWT.
- `POST /api/auth/login` senha antiga (`admin`) → `401`.
- `PUT /api/auth/password` sem JWT → `401`.
- `PUT /api/auth/password` com senha atual errada → `403`.
- `PUT /api/auth/password` com senha atual correta → `204`.
- Após a troca: login com senha antiga → `401`, com senha nova → `200`.
- SQLite: `AuthCredential` contém `admin` com hash `pbkdf2-sha256$210000$...` (90 chars).

### Playwright (UI, http://localhost:8096)

Asserts executados com `node:assert/strict`, todos PASS:

1. `/setup` mostra aviso de token de **uso único** e comando `docker logs`.
2. Setup com token cria senha e redireciona para `/login`.
3. Login com senha errada exibe erro.
4. Login com a senha criada entra no `/dashboard`.
5. Botão de configurações (engrenagem) ausente no header (`count() === 0`).
6. Menu do usuário exibe `Alterar senha` e abre o modal.
7. Modal com senha atual errada exibe "Senha atual incorreta".
8. Modal com senha atual correta persiste a troca e fecha.
9. Após logout, senha antiga é rejeitada.
10. Login com a senha nova funciona.
11. Toggle de tema aplica `dark` e retorna para `light`.
12. Menu do usuário abre, item ativo visível, fecha ao clicar fora.

Screenshots: `/tmp/opencode/qa-senha-setup.png`, `qa-senha-modal.png`, `qa-senha-theme.png`, `qa-senha-dashboard.png`.

## Achados

1. **[CORRIGIDO] Redirect indevido em `/setup`.** `apiFetch` redirecionava para `/login` quando `/api/theme` retornava `401`, impedindo a tela de setup de renderizar. Correção em `vue/plex-frontend/src/api.js`: redirect só fora de `/login` e `/setup`. Revalidado após rebuild da imagem.
2. Sem demais falhas na iteração 1.

## Conclusão

Todos os critérios da change foram validados na UI e na API. Loop de correção encerrado na iteração 1 (1 achado corrigido e revalidado).
