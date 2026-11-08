# Delta: series-full-page-view

## ADDED Requirements

### Requirement: Título da biblioteca sem estado provisório

A página de séries SHALL exibir um skeleton no lugar do título da biblioteca
enquanto o nome real não for resolvido e SHALL NOT renderizar um título
derivado da key da rota em nenhum momento.

#### Scenario: Título resolvido sem piscar

- **WHEN** o usuário abre `/libraries/{key}/series`
- **THEN** a trilha mostra um skeleton no lugar do título enquanto
  `/api/plex/libraries` não responde
- **AND** quando a resposta chega, o skeleton é substituído pelo título real
- **AND** o texto `Biblioteca {key}` nunca é exibido

#### Scenario: Falha ao carregar o título

- **WHEN** `/api/plex/libraries` falha
- **THEN** a trilha mostra um rótulo neutro de indisponível, nunca a key crua

#### Scenario: Conteúdo só aparece após carregar

- **WHEN** o título da biblioteca ainda está carregando
- **THEN** o grid de séries permanece em skeleton e a tela não exibe estado
  final incompleto
