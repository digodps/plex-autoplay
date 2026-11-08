# web-ui Specification (Delta)

## MODIFIED Requirements

### Requirement: Complete breadcrumb trail

A migalha de pão SHALL exibir todos os níveis conhecidos da rota atual abaixo
da home, começando em `Bibliotecas`, e SHALL separar os níveis com uma seta
para a direita. O logo do cabeçalho SHALL ser o único acesso à home no
cabeçalho.

#### Scenario: Dashboard

- **WHEN** o usuário visita `/dashboard`
- **THEN** a trilha não exibe itens nem setas
- **AND** o logo `Plex Autoplay` continua levando à home (`/`)

#### Scenario: Lista de bibliotecas

- **WHEN** o usuário visita `/libraries`
- **THEN** a trilha mostra `Bibliotecas`, com `Bibliotecas` como item atual
- **AND** não há seta sobrando antes ou depois do item

#### Scenario: Séries de uma biblioteca

- **WHEN** o usuário visita `/libraries/1/series` e a biblioteca `1` se chama
  `Animes`
- **THEN** a trilha mostra `Bibliotecas > Animes`, com `Bibliotecas` clicável
  e `Animes` com `aria-current="page"`
- **AND** a leitura do cabeçalho é `Plex Autoplay > Bibliotecas > Animes`

#### Scenario: Biblioteca carregando

- **WHEN** o usuário visita `/libraries/1/series` antes de as bibliotecas
  carregarem
- **THEN** a trilha mostra `Bibliotecas`, sem inventar título de biblioteca

#### Scenario: Biblioteca desconhecida

- **WHEN** as bibliotecas carregaram e a chave da rota não existe
- **THEN** o último item da trilha mostra `Biblioteca indisponível`
- **AND** o último item recebe `aria-current="page"`

#### Scenario: Marcadores de uma série

- **WHEN** o usuário visita `/series/42/markers` com
  `?libraryKey=1&libraryTitle=Animes&seriesTitle=Aosh`
- **THEN** a trilha mostra `Bibliotecas > Animes > Aosh`
- **AND** `Animes` aponta para `/libraries/1/series`

#### Scenario: Deep link de marcadores sem metadados

- **WHEN** o usuário visita `/series/42/markers` sem `libraryKey`,
  `libraryTitle` e `seriesTitle`
- **THEN** a trilha mostra `Bibliotecas > Série`

#### Scenario: Separador visual

- **WHEN** a trilha tem mais de um item
- **THEN** cada intervalo entre itens exibe um ícone de seta para a direita,
  marcado como decorativo e sem texto `/`

#### Scenario: Tela estreita

- **WHEN** a trilha não cabe na largura disponível
- **THEN** a lista rola horizontalmente e mantém todos os níveis na ordem

#### Scenario: Rotas fora de bibliotecas

- **WHEN** o usuário visita `/settings` ou outra rota com `meta.title`
- **THEN** a trilha mostra apenas o título da página como item atual

#### Scenario: Rota sem título

- **WHEN** o usuário visita uma rota sem `meta.title`
- **THEN** a trilha mostra `Página` como item atual
