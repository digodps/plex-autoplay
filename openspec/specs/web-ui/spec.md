# web-ui Specification

## Purpose
TBD - created by archiving change ajustar-menu-sidebar. Update Purpose after archive.

## Requirements

### Requirement: Menu lateral em português

O menu lateral SHALL exibir rótulos legíveis em português para cada item de navegação, em vez de caminhos de rota.

#### Scenario: Exibição dos rótulos
- **WHEN** o usuário autenticado abre o layout com o menu lateral
- **THEN** os itens exibem `Início`, `Bibliotecas`, `Sincronização` e `Configurações`

### Requirement: Espaçamento do ícone

O ícone de cada item do menu lateral SHALL ter espaçamento consistente em relação ao texto.

#### Scenario: Item do menu renderizado
- **WHEN** um item do menu lateral é renderizado
- **THEN** o ícone possui margem (`m-1`) separando-o do rótulo

### Requirement: Complete breadcrumb trail

A migalha de pão SHALL exibir todos os níveis conhecidos da rota atual,
incluindo `Bibliotecas`, e SHALL usar um ícone de seta para a direita entre os
itens.

#### Scenario: Lista de bibliotecas

- **WHEN** o usuário visita `/libraries`
- **THEN** a trilha mostra `Início > Bibliotecas`, com `Bibliotecas` como item
  atual e sem seta depois do último item

#### Scenario: Séries de uma biblioteca

- **WHEN** o usuário visita `/libraries/1/series` e a biblioteca `1` se chama
  `Animes`
- **THEN** a trilha mostra `Início > Bibliotecas > Animes`, com `Início` e
  `Bibliotecas` clicáveis e `Animes` com `aria-current="page"`

#### Scenario: Biblioteca carregando

- **WHEN** o usuário visita `/libraries/1/series` antes de as bibliotecas
  carregarem
- **THEN** a trilha mostra `Início > Bibliotecas`, sem inventar título de
  biblioteca

#### Scenario: Biblioteca desconhecida

- **WHEN** as bibliotecas carregaram e a chave da rota não existe
- **THEN** o último item da trilha mostra `Biblioteca indisponível`
- **AND** o último item recebe `aria-current="page"`

#### Scenario: Marcadores de uma série

- **WHEN** o usuário visita `/series/42/markers` com
  `?libraryKey=1&libraryTitle=Animes&seriesTitle=Aosh`
- **THEN** a trilha mostra `Início > Bibliotecas > Animes > Aosh`, com
  `Bibliotecas` apontando para `/libraries` e `Animes` apontando para
  `/libraries/1/series`

#### Scenario: Deep link de marcadores sem metadados

- **WHEN** o usuário visita `/series/42/markers` sem `libraryKey`,
  `libraryTitle` e `seriesTitle`
- **THEN** a trilha mostra `Início > Bibliotecas > Série`
- **AND** `Bibliotecas` aponta para `/libraries`

#### Scenario: Separador visual

- **WHEN** a trilha tem mais de um item
- **THEN** cada intervalo entre itens exibe um ícone de seta para a direita,
  marcado como decorativo e sem texto `/`

#### Scenario: Tela estreita

- **WHEN** a trilha não cabe na largura disponível
- **THEN** a lista rola horizontalmente e mantém todos os níveis na ordem
