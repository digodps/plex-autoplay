# Spec Delta

## Purpose
Adicionar gate de consentimento (aviso estilo Netflix) antes do auto-skip de intro/créditos. Quando desabilitado (default `false`), o `PlexAutoSkipBackgroundService` **não executa nenhum seek**. O usuário deve autorizar explicitamente via ⚙ Ajustes.

## ADDED Requirements

### Requirement: Configuração `auto_skip_authorized`
O sistema SHALL expor configuração operacional `auto_skip_authorized` (bool) com default `false`.

#### Scenario: Não autorizado (default)
- **WHEN** `auto_skip_authorized` é `false` ou não definido
- **THEN** o `PlexAutoSkipBackgroundService` detecta marcadores mas **não executa seek**; nada é feito

#### Scenario: Autorizado pelo usuário
- **WHEN** `auto_skip_authorized` é `true`
- **THEN** o auto-skip funciona normalmente (comportamento original)

### Requirement: Endpoint `/api/settings` inclui `auto_skip_authorized`
GET/PUT `/api/settings` SHALL incluir/expor `auto_skip_authorized` no corpo da requisição e resposta.

#### Scenario: Valor exposto no GET
- **WHEN** chamar GET `/api/settings`
- **THEN** resposta inclui `"auto_skip_authorized": true/false`

#### Scenario: Valor exposto no PUT
- **WHEN** chamar PUT `/api/settings` com `{"auto_skip_authorized": true}`
- **THEN** resposta inclui `"auto_skip_authorized": true`

### Requirement: Frontend — Toggle com aviso de autorização
A página ⚙ Ajustes SHALL exibir controle "Autorizar skip automático".

#### Scenario: Tentar ligar o toggle
- **WHEN** o usuário tenta ligar o toggle
- **THEN** exibir aviso/modal explicando: "O Plex pulará introduções e créditos automaticamente. Autorizar?"
- **WHEN** o usuário confirma
- **THEN** salvar `auto_skip_authorized: true`
- **WHEN** o usuário cancela OU fecha sem confirmar
- **THEN** toggle permanece desligado, `auto_skip_authorized` fica `false`, nenhum dado persistido

### Requirement: Persistência e migração SQLite
O sistema SHALL persistir `AUTO_SKIP_AUTHORIZED` na tabela `AppSetting` (chave-valor) com default `false`.

#### Scenario: Valor default gravado
- **WHEN** a chave `AUTO_SKIP_AUTHORIZED` nunca existiu no banco
- **THEN** `AppSettingsSeeder.SeedAsync` grava valor `false` para garantir operação segura

#### Scenario: Lê e persiste corretamente
- **WHEN** os dados são atualizados via PUT `/api/settings` com `auto_skip_authorized = true`
- **THEN** GET `/api/settings` retorna `true` persistido em banco

## REMOVED Requirements
Nenhum.