# Spec Delta

## Purpose

Permite sincronizar incrementalmente bibliotecas e séries do Plex para o banco local, detectando adições e removendo o que não existe mais no servidor.

## ADDED Requirements

### Requirement: Descobrir e persistir bibliotecas de séries
O sistema SHALL consultar a API do Plex, filtrar bibliotecas do tipo "show" e persistir cada uma no banco local com rating key, título, tipo e agente.

#### Scenario: Biblioteca nova é inserida no banco
- **WHEN** a sincronização encontra uma biblioteca do tipo "show" que ainda não existe no banco
- **THEN** o sistema insere um registro na tabela de bibliotecas com os dados da Plex

#### Scenario: Biblioteca já existente não é duplicada
- **WHEN** a sincronização encontra uma biblioteca do tipo "show" já presente no banco
- **THEN** o sistema mantém o registro existente sem criar duplicados

### Requirement: Persistir séries de cada biblioteca
O sistema SHALL listar as séries de cada biblioteca de séries e persistir cada série no banco com rating key, título, sumário, capa e a biblioteca a que pertence.

#### Scenario: Série nova é inserida vinculada à biblioteca
- **WHEN** a sincronização encontra uma série que ainda não existe no banco para uma biblioteca
- **THEN** o sistema insere um registro na tabela de séries com o rating key, título, sumário, capa e a chave da biblioteca

#### Scenario: Série já existente é atualizada
- **WHEN** a sincronização encontra uma série já presente no banco com título ou capa alterados no Plex
- **THEN** o sistema atualiza os campos de título, sumário e capa do registro existente

### Requirement: Remover bibliotecas que não existem mais no Plex
O sistema SHALL comparar as bibliotecas do banco com as bibliotecas atuais do Plex e remover do banco as bibliotecas que não aparecem mais na API.

#### Scenario: Biblioteca removida do Plex é removida do banco (hard delete)
- **WHEN** uma biblioteca existe no banco mas não é mais listada pela API do Plex
- **THEN** o sistema remove o registro da biblioteca e todas as séries vinculadas a ela do banco

### Requirement: Remover séries que não existem mais no Plex
O sistema SHALL comparar as séries do banco com as séries atuais de cada biblioteca no Plex e remover do banco as séries que não aparecem mais na API.

#### Scenario: Série removida do Plex é removida do banco (hard delete)
- **WHEN** uma série existe no banco para uma biblioteca mas não é mais listada na API do Plex para essa biblioteca
- **THEN** o sistema remove o registro da série do banco

### Requirement: Consultar catálogo de séries do banco
O sistema SHALL expor o endpoint de listagem de séries de uma biblioteca lendo do banco local, com paginação e busca por título, sem chamar a API do Plex para cada requisição.

#### Scenario: Listagem paginada lida do banco
- **WHEN** o cliente requisita a página 1 com tamanho 24 de séries de uma biblioteca
- **THEN** o sistema retorna até 24 séries do banco para essa biblioteca, com o total de itens para paginação

#### Scenario: Busca por título no banco
- **WHEN** o cliente envia um termo de busca no endpoint de listagem de séries
- **THEN** o sistema retorna apenas as séries cujo título contém o termo, aplicando paginação
