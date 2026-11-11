# auth-session Specification

## Purpose

Garantir que todas as páginas do dashboard sejam acessíveis apenas com sessão autenticada, lembrar a sessão do usuário entre visitas (via cookie) conforme a sua escolha, e devolvê-lo à página que tentou acessar após o login.

## Requirements

### Requirement: Guarda de rota em todas as páginas

O sistema SHALL exigir autenticação para todas as rotas do dashboard exceto a rota de login, e SHALL redirecionar o usuário não autenticado para a página de login preservando a rota original solicitada.

#### Scenario: Acesso direto não autenticado

- **WHEN** o usuário abre uma rota interna (ex.: `/series/123/markers`) sem sessão existente
- **THEN** ele é levado à página de login, e o destino original é registrado antes do redirecionamento.

#### Scenario: Acesso autenticado normal

- **WHEN** o usuário autenticado navega entre páginas internas
- **THEN** nenhuma interrupção ou redirecionamento para login acontece.

### Requirement: Retorno à página original após autenticação

O sistema SHALL devolver o usuário à rota que tentou acessar originalmente após o login bem-sucedido, em vez de sempre para o dashboard.

#### Scenario: Retorno após guard

- **WHEN** o usuário tenta acessar `/series/123/markers` sem sessão, autentica-se com senha válida
- **THEN** o sistema o devolve a `/series/123/markers` (a rota original, não um destino fixo).

#### Scenario: Login sem rota original

- **WHEN** o usuário abre a página de login diretamente, sem rota original registrada
- **THEN** após autenticar é levado ao dashboard.

### Requirement: Persistência do token em cookie

<<<<<<< HEAD
O sistema SHALL armazenar o token de autenticação em cookie `HttpOnly` definido pelo servidor na resposta de login, de modo que o JavaScript da página não consiga lê-lo, e o navegador SHALL enviá-lo automaticamente em requisições subsequentes. O cookie SHALL ser de sessão (encerrado ao fechar o navegador) e SHALL ser marcado `Secure` e `SameSite=Lax`.

#### Scenario: Cookie definido pelo servidor no login
- **WHEN** o usuário se autentica com senha válida
- **THEN** a resposta de login inclui `Set-Cookie: auth_token=...; HttpOnly; Secure; SameSite=Lax` sem `Max-Age`/`Expires`.

#### Scenario: Token reutilizado entre visitas
- **WHEN** o usuário navega no dashboard após autenticar-se
- **THEN** o navegador envia o cookie `auth_token` automaticamente e o sistema o considera autenticado sem pedir a senha novamente.

#### Scenario: JavaScript não lê o token
- **WHEN** a aplicação cliente executa `document.cookie`
- **THEN** o valor de `auth_token` não aparece, pois o cookie é `HttpOnly`.

#### Scenario: Logout limpa o cookie
- **WHEN** o usuário efetua logout
- **THEN** o cookie `auth_token` é removido e a navegação passa a exigir novo login.

#### Scenario: Cookie com token inválido
- **WHEN** o cookie contém um token inválido ou expirado (rejeitado pelo backend)
- **THEN** o sistema trata o usuário como não autenticado e o encaminha ao login.

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

### Requirement: Envio automático de credenciais pelo cliente

O cliente web SHALL enviar `credentials: 'include'` em todas as requisições à API e MUST NOT ler, armazenar ou montar manualmente o token de autenticação.

#### Scenario: Requisição autenticada sem manipulação de token
- **WHEN** o cliente chama um endpoint protegido da API
- **THEN** a requisição inclui o cookie de sessão via `credentials: 'include'`, sem header `Authorization` montado pelo JavaScript.
=======
O sistema SHALL armazenar o token de autenticação em cookie no navegador em vez de `localStorage`, reutilizando o token salvo nas visitas seguintes para evitar novo login, e SHALL removê-lo no logout.

#### Scenario: Token reutilizado entre visitas

- **WHEN** o usuário retorna ao dashboard com token válido em cookie
- **THEN** o sistema o considera autenticado sem solicitar a senha novamente.

#### Scenario: Logout limpa o cookie

- **WHEN** o usuário efetua logout
- **THEN** o cookie do token é removido e a navegação passa a exigir novo login.

#### Scenario: Cookie com token inválido

- **WHEN** o cookie contém um token inválido ou expirado (rejeitado pelo backend)
- **THEN** o sistema trata o usuário como não autenticado e o encaminha ao login.

### Requirement: Escolha de manter a autenticação

O sistema SHALL perguntar ao usuário na tela de login se ele deseja manter a autenticação: ao optar por manter, o cookie do token SHALL ser persistente; ao não optar, o token SHALL ser mantido em cookie de sessão (encerrado ao fechar o navegador).

#### Scenario: Usuário opta por manter autenticado

- **WHEN** o usuário marca "manter-me conectado" e autentica-se
- **THEN** o token é salvo em cookie persistente (por exemplo, 30 dias) e as visitas futuras não pedem novo login.

#### Scenario: Usuário não opta por manter autenticado

- **WHEN** o usuário deixa "manter-me conectado" desmarcado e autentica-se
- **THEN** o token é salvo em cookie de sessão, sendo encerrado ao fechar o navegador e pedindo novo login na visita seguinte.

### Requirement: Limpeza de cookie antes do login

O sistema SHALL limpar qualquer cookie de autenticação existente (`auth_token`) antes de tentar realizar a requisição de login ou armazenar um novo token, garantindo que cookies corrompidos ou obsoletos não interfiram na nova sessão.

#### Scenario: Realização de login com cookie antigo existente

- **WHEN** o usuário submete o formulário de login possuindo um cookie de autenticação antigo ou inválido no navegador
- **THEN** o sistema remove o cookie existente antes de enviar as credenciais/armazenar o novo token e redireciona para o dashboard com o novo token válido.
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
