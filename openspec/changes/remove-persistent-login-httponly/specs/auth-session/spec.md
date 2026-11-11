# Spec Delta

## MODIFIED Requirements

### Requirement: Persistência do token em cookie

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

## ADDED Requirements

### Requirement: Envio automático de credenciais pelo cliente

O cliente web SHALL enviar `credentials: 'include'` em todas as requisições à API e MUST NOT ler, armazenar ou montar manualmente o token de autenticação.

#### Scenario: Requisição autenticada sem manipulação de token
- **WHEN** o cliente chama um endpoint protegido da API
- **THEN** a requisição inclui o cookie de sessão via `credentials: 'include'`, sem header `Authorization` montado pelo JavaScript.

## REMOVED Requirements

### Requirement: Escolha de manter a autenticação

**Reason**: A opção de login persistente foi removida; todas as sessões passam a ser de sessão (encerradas ao fechar o navegador), reduzindo superfície de risco e simplificando a UI.

**Migration**: Nenhuma ação do usuário. Cookies persistentes antigos deixam de ser emitidos; o novo cookie de sessão substitui o anterior no próximo login.

### Requirement: Limpeza de cookie antes do login

**Reason**: O cookie agora é `HttpOnly`, portanto o frontend não pode removê-lo. A reemissão do cookie pelo servidor no login substitui qualquer valor anterior, tornando a limpeza client-side desnecessária.

**Migration**: Nenhuma ação do usuário. O servidor sobrescreve `auth_token` a cada login bem-sucedido.
