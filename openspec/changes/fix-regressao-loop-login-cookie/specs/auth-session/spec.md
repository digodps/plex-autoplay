# Spec Delta

## ADDED Requirements

### Requirement: Auto-login com cookie válido

O sistema SHALL redirecionar o usuário para a dashboard automaticamente caso ele acesse a rota `/login` possuindo um token de autenticação válido no cookie.

#### Scenario: Redirecionamento automático ao acessar login com sessão ativa

- **WHEN** o usuário acessa `/login` tendo um `auth_token` válido no cookie
- **THEN** o sistema detecta o token e redireciona para a dashboard sem exibir o formulário de login.

### Requirement: Deleção robusta de cookie

O sistema SHALL garantir que a função `deleteCookie` em `vue/plex-frontend/src/api.js` sempre inclua `path=/` e `SameSite=Lax` (e `Secure` em HTTPS), cobrindo todas as variações de cookie que podem ter sido criadas anteriormente.

#### Scenario: DeleteCookie remove cookie independente de como foi criado

- **WHEN** `deleteCookie('auth_token')` é chamado
- **THEN** o cookie é removido com `path=/`, garantindo que cookies criados com diferentes caminhos sejam todos eliminados.

## MODIFIED Requirements

### Requirement: Limpeza de cookie antes do login

O sistema SHALL limpar qualquer cookie de autenticação existente (`auth_token`) antes de enviar a requisição de login ou armazenar um novo token, garantindo que cookies corrompidos, expirados ou obsoletos não interfiram na nova sessão.

#### Scenario: Realização de login com cookie antigo existente

- **WHEN** o usuário submete o formulário de login possuindo um cookie de autenticação antigo ou inválido no navegador
- **THEN** o sistema remove o cookie existente antes de enviar as credenciais/armazenar o novo token e redireciona para o dashboard com o novo token válido.

#### Scenario: Limpeza ocorre antes da requisição POST

- **WHEN** o usuário clica em "Entrar" com um cookie `auth_token` inválido preexistente
- **THEN** o cookie antigo é removido ANTES da requisição POST `/auth/login` ser enviada, evitando que o backend rejeite a nova tentativa de login.