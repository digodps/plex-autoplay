# Proposal

## Why
O sistema precisa garantir maior segurança contra ataques XSS armazenando o token de autenticação em um cookie HTTP-only gerenciado pelo servidor. Além disso, a opção de login persistente ("manter-me conectado") deve ser removida para simplificar a experiência e garantir que todas as sessões sejam de sessão (encerradas ao fechar o navegador).

## What Changes
- Remoção da opção "Manter-me conectado" na página de login.
- O endpoint `/auth/login` passa a definir o cookie `auth_token` como `HttpOnly`, `Secure`, e `SameSite=Lax`.
- O frontend deixa de armazenar/ler o token via JavaScript, delegando o envio automático de credenciais nas requisições HTTP (`credentials: 'include'`).

## Capabilities
### Modified Capabilities
- `auth-session`: alteração para cookie HttpOnly de sessão, removendo a opção de persistência e checkbox na UI.

## Impact
- `src/PlexAutoplay.Web/Endpoints/AuthEndpoints.cs`
- `vue/plex-frontend/src/views/LoginView.vue`
- `vue/plex-frontend/src/stores/auth.js`
- `vue/plex-frontend/src/api.js`
- `tests/e2e/tests/auth.spec.js`
