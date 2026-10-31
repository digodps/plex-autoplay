# Tasks

## Backend (Agente 1)
- [x] Modificar `AuthEndpoints.cs` para adicionar cookie `auth_token` com `HttpOnly`, `Secure` e `SameSite=Lax` na resposta de login. Cookie deve ser de sessão (sem expires/max-age).
- [x] Remover lógica de persistência do backend.
- [x] Verificar se há outro endpoint que leia/defina o cookie manualmente e removê-lo/ajustá-lo.
- [x] Executar `dotnet test` para validar build e testes unitários.

## Frontend (Agente 2)
- [x] Remover checkbox "Manter-me conectado" da `LoginView.vue`.
- [x] Limpar código inusitado: remover `keepSigned` e `deleteCookie` da `LoginView.vue`.
- [x] Remover `keepSigned` parâmetro do `auth.js` store (`login()` e `login(token, keepSigned)`).
- [x] Atualizar `auth.js` state para remover `token` e simplificar para `isAuthenticated`.
- [x] Atualizar `api.js` para usar `credentials: 'include'` automaticamente, removendo leitura manual do token via `getToken()`/`setCookie()`.
- [x] Remover migração de `localStorage` em `main.js`.
- [x] Executar Lint/Prettier nos arquivos Vue.

## Tests (Agente 3)
- [x] Atualizar `tests/e2e/tests/auth.spec.js` para refletir que o token está em cookie HttpOnly de sessão (sem max-age).
- [x] Remover teste "cookie de sessão vira persistente quando 'manter-me conectado' é marcado".
- [x] Validar que o teste "login com senha correta retorna token JWT e redireciona para dashboard" ainda passa.
- [ ] Executar testes E2E e verificar se 100% passam (ou confirmar falhas preexistentes). — 23 falhas, maioria preexistente no ambiente (series-editor, infinite-scroll, libraries, markers); testes de auth atualizados.

## Deliverable (Agente Principal)
- [x] Commit via `scripts/commit-skill.py` com mensagem Conventional Commits com emoji.
- [x] Push para origin.
- [x] Abrir PR via `gh pr create` (PR #21).
- [ ] Confirmar aprovação e merge.
