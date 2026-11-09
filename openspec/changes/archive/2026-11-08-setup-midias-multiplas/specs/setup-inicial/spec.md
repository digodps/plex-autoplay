# setup-inicial Specification

## Purpose

Define o primeiro acesso do operador: um único fluxo que cria a senha e já deixa Plex e diretórios de mídia configurados, de forma validada e repetível em caso de falha.

## ADDED Requirements

### Requirement: Setup inicial coleta credenciais e configuração do Plex

O sistema SHALL, quando não houver credencial de operador configurada, exigir um fluxo de primeiro acesso que colete token de setup, nova senha, URL do Plex, token do Plex e ao menos um diretório de mídia antes de liberar o login.

#### Scenario: Setup concluído com dados válidos
- **WHEN** o operador envia token de setup válido, senha que atende a política, URL do Plex acessível com o token informado e ao menos um diretório existente
- **THEN** o sistema valida tudo, persiste `PLEX_URL`, `PLEX_TOKEN` e `MEDIA_ROOTS`, cria a credencial, invalida o token de setup e responde `204`

#### Scenario: URL ou token do Plex inválidos
- **WHEN** `GET /identity` do Plex falha ou excede 10 segundos com a URL/token informados
- **THEN** o sistema responde `400` com erro prefixado `plex_url:` ou `plex_token:` e não cria credencial nem persiste configuração

#### Scenario: Esquema de URL não suportado
- **WHEN** a URL do Plex não é `http://` nem `https://` absoluta
- **THEN** o sistema responde `400` com `plex_url: deve ser uma URL absoluta http:// ou https://`

#### Scenario: Nenhum diretório de mídia válido
- **WHEN** a lista de diretórios está vazia ou contém caminho inexistente/relativo
- **THEN** o sistema responde `400` listando os caminhos inválidos e não cria credencial

#### Scenario: Falha ao criar credencial
- **WHEN** a gravação da configuração do Plex/mídia teve sucesso mas a criação da credencial falha
- **THEN** o setup continua disponível para nova tentativa e o login permanece bloqueado

#### Scenario: Token de setup inválido ou já usado
- **WHEN** o token de setup enviado não é válido
- **THEN** o sistema responde `401` e mantém o setup disponível

#### Scenario: Setup já concluído
- **WHEN** já existe credencial de operador
- **THEN** o sistema responde `409` e não altera configuração existente

### Requirement: Interface de primeiro acesso

O sistema SHALL apresentar a página `/setup` como fluxo sequencial de três passos — (1) token de setup e senha, (2) URL e token do Plex, (3) diretórios de mídia — com navegação para voltar e avançar, exibindo os erros retornados pelo backend sem perder os valores preenchidos.

#### Scenario: Erro de validação exibido
- **WHEN** o backend responde `400` com `errors`
- **THEN** a página mostra cada erro e mantém URL, listas de diretórios e demais campos preenchidos

#### Scenario: Teste de conexão no passo do Plex
- **WHEN** o operador aciona "Testar conexão" no passo 2
- **THEN** a página chama `POST /api/plex/identity` sem sessão (permitido enquanto o setup está pendente) e mostra sucesso ou a mensagem de erro do Plex

#### Scenario: Setup concluído entra autenticado
- **WHEN** o backend responde `204`
- **THEN** a página autentica com a senha recém-criada e abre o dashboard sem passar pela tela de login

#### Scenario: Setup já concluído antes do último passo
- **WHEN** o backend responde `409`
- **THEN** a página redireciona para `/login`

#### Scenario: Diretório escolhido pelo navegador antes de existir sessão
- **WHEN** o operador abre o seletor de diretórios durante o setup
- **THEN** a listagem funciona mesmo sem sessão autenticada

### Requirement: Seed retrocompatível de configuração

O sistema SHALL, ao inicializar sem a chave `MEDIA_ROOTS`, derivar a lista de diretórios da chave legada `MEDIA_ROOT` (banco ou ambiente) para não quebrar instalações existentes.

#### Scenario: Instalação existente com MEDIA_ROOT
- **WHEN** o banco possui apenas `MEDIA_ROOT=/media`
- **THEN** o sistema expõe `media_roots` como `["/media"]`

#### Scenario: Banco já populado sem MEDIA_ROOTS
- **WHEN** o banco tem outras chaves mas não `MEDIA_ROOTS`
- **THEN** o sistema grava a lista derivada de `MEDIA_ROOTS`/`MEDIA_ROOT` do ambiente ou da linha `MEDIA_ROOT` existente
