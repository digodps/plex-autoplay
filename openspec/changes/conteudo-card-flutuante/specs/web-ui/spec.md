# web-ui Specification (Delta)

## ADDED Requirements

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
