# Tasks

## 1. Melhorar Helpers de Cookies

- [x] 1.1 Atualizar `deleteCookie` em `vue/plex-frontend/src/api.js` para garantir que o cookie seja removido com `path=/`. Verificação: executar `deleteCookie('auth_token')` no console do navegador e conferir se o cookie desaparece do Application tab.

## 2. Prevenção de Loop no Login

- [x] 2.1 Modificar `LoginView.vue` para chamar `deleteCookie('auth_token')` imediatamente ANTES da requisição POST `/auth/login` (em vez de após a resposta ok). Verificação: fazer login com um cookie inválido preexistente e conferir no log do backend se a primeira requisição autenticada após o login usa o novo token.
## 3. Auto-Login
- [x] 3.1 Adicionar verificação em `LoginView.vue` (no `onMounted`) que: se `isAuthenticated` for true, redireciona para a dashboard imediatamente.

