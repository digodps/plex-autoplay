# web-ui Specification

## Purpose
TBD - created by archiving change ajustar-menu-sidebar. Update Purpose after archive.

## Requirements

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

### Requirement: Card de conteúdo flutuante

O shell autenticado SHALL renderizar o conteúdo das views dentro de um card com cantos arredondados, flutuando sobre o body, com uma margem pequena em relação à viewport.

O card SHALL usar `white` no tema claro e `gray-800` no tema escuro. O body SHALL usar `gray-50` no tema claro e `gray-900` no tema escuro.

#### Scenario: Tema claro

- **WHEN** o usuário autenticado abre qualquer rota do shell com o tema claro
- **THEN** o body e o header exibem o fundo `gray-50`
- **AND** o card exibe fundo `white` com cantos arredondados e sombra
- **AND** o card mantém uma margem pequena em relação às bordas da viewport

#### Scenario: Tema escuro

- **WHEN** o usuário alterna para o tema escuro
- **THEN** o body e o header exibem o fundo `gray-900`
- **AND** o card exibe `gray-800` com cantos arredondados e sombra
- **AND** o card mantém a mesma margem pequena em relação à viewport

#### Scenario: Views sem superfície própria

- **WHEN** qualquer view do shell é renderizada dentro do card
- **THEN** a view não pinta fundo `gray-50`/`gray-900` nem força `min-h-screen`
- **AND** o tom de superfície do card permanece visível

### Requirement: Header fixo e rolagem interna

O `AppHeader` SHALL permanecer fixo no topo, sem borda inferior, e a rolagem SHALL ocorrer somente dentro do card. A barra de rolagem do card SHALL permanecer invisível, sem remover a rolagem por roda, toque ou trackpad.

#### Scenario: Conteúdo maior que a viewport

- **WHEN** o conteúdo de uma view excede a altura da viewport
- **THEN** o header permanece visível no topo sem rolar
- **AND** somente o conteúdo dentro do card rola
- **AND** a barra de rolagem do card não é visível

#### Scenario: Troca de rota

- **WHEN** o usuário navega para outra rota do shell
- **THEN** a rolagem do card volta ao topo

#### Scenario: Navegação preservada

- **WHEN** o usuário alterna o tema, abre o menu do usuário, navega por uma rota ou fecha o menu
- **THEN** o comportamento e os estados ativos existentes continuam funcionando

### Requirement: Navegação superior por breadcrumb

O shell autenticado SHALL renderizar uma top bar fixa com marca, breadcrumb, toggle de tema e menu do usuário, SHALL NOT renderizar menu lateral, rail, dock ou lista de navegação lateral.

#### Scenario: Shell sem menu lateral
- **WHEN** o usuário autenticado abre qualquer rota do shell
- **THEN** nenhum elemento `<aside>` de navegação ou botão de expandir/recolher menu é renderizado

#### Scenario: Breadcrumb permite voltar
- **WHEN** o usuário está em `/series/:seriesKey/markers`
- **THEN** o breadcrumb mostra `Início → Bibliotecas → <biblioteca> → <série>`, com `Bibliotecas` e a biblioteca clicáveis e o item atual com `aria-current="page"`

#### Scenario: Top bar mantém ações
- **WHEN** o usuário abre o menu do usuário na top bar
- **THEN** as ações Configurações, Alterar senha e Sair continuam disponíveis, e o toggle de tema permanece visível
