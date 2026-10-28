# Spec Delta

## Purpose

Garantir que todas as páginas do dashboard sejam acessíveis apenas com sessão autenticada, lembrar a sessão do usuário entre visitas (via cookie) conforme a sua escolha, e devolvê-lo à página que tentou acessar após o login.

## ADDED Requirements

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
