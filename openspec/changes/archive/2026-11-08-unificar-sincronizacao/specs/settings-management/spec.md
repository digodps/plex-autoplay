# Spec Delta — settings-management

## MODIFIED Requirements

### Requirement: API de configurações persistidas

O sistema SHALL expor `GET /api/settings` e `PUT /api/settings`, protegidos
por JWT, para ler e gravar configurações operacionais no SQLite. O intervalo de
sincronização é **único** (`sync_interval_minutes`); não existe mais
`sync_catalog_interval_minutes`.

#### Scenario: Leitura autenticada

- **WHEN** uma requisição autenticada `GET /api/settings` é recebida
- **THEN** o sistema retorna `plex_url`, `plex_token_set`, `media_root` e
  `sync_interval_minutes`
- **AND** não retorna `sync_catalog_interval_minutes`
- **AND** os valores vêm da tabela `AppSetting`
- **AND** o token do Plex nunca é retornado

#### Scenario: Gravação autenticada

- **WHEN** uma requisição autenticada `PUT /api/settings` contém valores válidos
- **THEN** o sistema grava todos os campos no SQLite em uma única transação
- **AND** a resposta reflete os valores persistidos

#### Scenario: Token preservado quando não enviado

- **WHEN** `PUT /api/settings` não inclui `plex_token`
- **THEN** o token existente permanece inalterado
- **WHEN** `plex_token` é enviado vazio
- **THEN** o token é limpo

#### Scenario: Validação de entrada

- **WHEN** `plex_url` não é uma URL absoluta `http://` ou `https://`
- **THEN** o sistema retorna HTTP 400 sem gravar nada
- **WHEN** `sync_interval_minutes` está fora de `1..1440`
- **THEN** o sistema retorna HTTP 400 sem gravar nada
- **WHEN** `media_root` não é absoluto ou não existe
- **THEN** o sistema retorna HTTP 400 sem gravar nada

#### Scenario: Requisição sem autenticação

- **WHEN** `GET /api/settings` ou `PUT /api/settings` é chamado sem JWT válido
- **THEN** o sistema retorna HTTP 401
- **AND** não expõe o token do Plex

### Requirement: Página de ajustes orientada ao banco

A página `/settings` SHALL carregar as configurações de `/api/settings`,
exibir URL do Plex, token, `MEDIA_ROOT` e o **único** intervalo de
sincronização (`sync_interval_minutes`), e SHALL salvar as alterações via
`PUT /api/settings`.

#### Scenario: Abertura da página

- **WHEN** o operador abre `/settings`
- **THEN** os campos são preenchidos com os valores persistidos no banco
- **AND** o navegador não usa `localStorage` como fonte de configuração
- **AND** o campo de token aparece como senha e vazio quando já configurado
- **AND** existe apenas um campo de intervalo, sem o campo de catálogo

#### Scenario: Salvamento bem-sucedido

- **WHEN** o operador altera um campo e salva
- **THEN** a interface exibe confirmação de sucesso
- **AND** os valores permanecem após recarregar a página

#### Scenario: Erro de salvamento

- **WHEN** a API retorna erro de validação
- **THEN** a interface exibe a mensagem retornada
- **AND** mantém os valores digitados para correção

## REMOVED Requirements

- Chave de configuração `sync_catalog_interval_minutes` e a chave persistida
  `SYNC_CATALOG_INTERVAL_MINUTES`, substituídas por um intervalo único de
  sincronização. Valor pré-existente no banco é ignorado.