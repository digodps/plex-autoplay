# Spec Delta

## Purpose

Define a Netflix-style genre carousel for the series catalog, grouping series by their genre with horizontal scrolling, section headers, and smooth hover/scroll animations.

## ADDED Requirements

### Requirement: Agrupar séries por gênero

O sistema SHALL agrupar as séries de uma biblioteca pelo campo `genre` retornado pela API e SHALL exibir um carrossel horizontal por gênero na página de séries.

#### Scenario: Séries agrupadas por gênero

- **WHEN** o usuário acessa `/libraries/{key}/series`
- **THEN** o sistema busca todas as séries da biblioteca
- **AND** agrupa-as por `genre`
- **AND** exibe uma seção horizontal por gênero, com o nome do gênero como título

#### Scenario: Série sem gênero

- **WHEN** uma série não possui gênero preenchido
- **THEN** o sistema a agrupa sob a seção "Outros", exibida por último

#### Scenario: Gêneros ordenados

- **WHEN** o sistema renderiza os grupos de gênero
- **THEN** os gêneros são ordenados alfabeticamente, exceto "Outros" que aparece por último

### Requirement: Carrossel horizontal com rolagem suave

O sistema SHALL exibir cada grupo de gênero como um carrossel horizontal com rolagem suave, botões de navegação laterais e rolagem por roda do mouse.

#### Scenario: Botões de rolagem aparecem conforme necessidade

- **WHEN** um carrossel possui conteúdo além da área visível
- **THEN** botões de rolagem esquerda/direita aparecem ao passar o mouse
- **AND** o botão esquerdo só aparece quando há conteúdo à esquerda
- **AND** o botão direito só aparece quando há conteúdo à direita

#### Scenario: Rolagem por roda do mouse

- **WHEN** o usuário usa a roda do mouse sobre um carrossel
- **THEN** o carrossel rola horizontalmente

### Requirement: Animação de destaque dos cards

O sistema SHALL animar cada card de série com escala e elevação ao passar o mouse ou focar por teclado, destacando o item em interação.

#### Scenario: Destaque ao passar o mouse

- **WHEN** o usuário passa o mouse sobre um card
- **THEN** o card escala suavemente, eleva-se e recebe sombra
- **AND** a imagem do pôster aplica leve zoom

#### Scenario: Acessibilidade por teclado

- **WHEN** o usuário navega por teclado até um card
- **THEN** o card recebe destaque de foco visível
- **AND** pressionar Enter navega para os marcadores da série

### Requirement: Tema claro e escuro

O sistema SHALL respeitar o tema ativo (claro/escuro) usando as variáveis CSS globais, sem cores fixas que quebrem o contraste.

#### Scenario: Alternância de tema

- **WHEN** o usuário alterna entre tema claro e escuro
- **THEN** os carrosséis, títulos de gênero, botões e cards usam as cores do tema ativo corretamente
