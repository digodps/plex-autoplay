# series-full-page-view Specification

## Purpose
Permite ao usuário navegar para uma página dedicada de séries ao clicar em uma biblioteca, com breadcrumb para retorno, substituindo a expansão inline anterior.

## Requirements

### Requirement: Navegação para página completa de séries
O sistema SHALL navegar para a rota `/libraries/{key}/series` quando o usuário clica em uma biblioteca na lista de bibliotecas, exibindo o catálogo de séries em tela cheia.

#### Scenario: Clicar na biblioteca navega para página de séries
- **WHEN** usuário clica em um card de biblioteca na página `/libraries`
- **THEN** roteador navega para `/libraries/{libraryKey}/series`
- **AND** página de séries carrega com o catálogo da biblioteca selecionada
- **AND** URL reflete a biblioteca ativa para deep-linking e compartilhamento

#### Scenario: Deep-link direto para página de séries
- **WHEN** usuário acessa diretamente `/libraries/{key}/series`
- **THEN** página carrega o catálogo da biblioteca especificada
- **AND** breadcrumb mostra a biblioteca ativa

### Requirement: Breadcrumb de navegação

O sistema SHALL exibir a trilha dos níveis abaixo da home no topo da página de
séries, permitindo retorno à lista de bibliotecas com um clique, sem depender
do botão "voltar" do navegador. O acesso à home fica com o logo do cabeçalho,
conforme `web-ui`.

#### Scenario: Breadcrumb exibe hierarquia e permite retorno

- **WHEN** usuário está na página `/libraries/{key}/series`
- **THEN** breadcrumb mostra `Bibliotecas > {Título da Biblioteca}`
- **AND** clicar em "Bibliotecas" navega para `/libraries`
- **AND** título da biblioteca ativa é exibido sem link (estado atual)

#### Scenario: Breadcrumb acessível

- **WHEN** leitor de tela navega pelo breadcrumb
- **THEN** estrutura semântica `<nav aria-label="Breadcrumb">` com `<ol>` e
  `<li>` é usada
- **AND** o link de retorno tem nome acessível "Bibliotecas"
- **AND** o item atual tem `aria-current="page"`

### Requirement: Catálogo de séries em página completa
O sistema SHALL exibir o grid de séries ocupando toda a largura da área de conteúdo, com toolbar de busca e contador, scroll infinito funcionando no container da página.

#### Scenario: Grid ocupa largura total da área de conteúdo
- **WHEN** usuário visualiza página de séries
- **THEN** grid de séries usa toda a largura disponível (main content area)
- **THEN** toolbar de busca e contador ficam acima do grid
- **THEN** sentinela de infinite scroll está no fim do grid dentro do container rolável da página

#### Scenario: Estados de carregamento e erro
- **WHEN** página carrega inicialmente
- **THEN** skeletons são exibidos (24 cards)
- **WHEN** ocorre erro de API
- **THEN** mensagem de erro com botão "Tentar novamente" é exibida
- **WHEN** catálogo está vazio
- **THEN** estado vazio com ícone e mensagem é exibido

### Requirement: Destaque da miniatura no hover e foco

O sistema SHALL destacar visualmente a miniatura do card de série quando o usuário passa o mouse sobre o card ou o foca via teclado, mantendo a navegação e o layout do grid inalterados.

#### Scenario: Hover destaca a miniatura

- **WHEN** o usuário posiciona o mouse sobre um card de série em `/libraries/{key}/series`
- **THEN** a miniatura da série recebe realce visual (leve zoom e/ou overlay)
- **AND** o card continua clicável e navega para `/series/{seriesKey}/markers`

#### Scenario: Foco por teclado destaca a miniatura

- **WHEN** o usuário navega com Tab até um card de série
- **THEN** a miniatura apresenta o mesmo realce visual do hover
- **AND** o indicador de foco permanece visível

#### Scenario: Movimento reduzido

- **WHEN** o usuário tem `prefers-reduced-motion: reduce`
- **THEN** o realce não usa zoom animado
- **AND** o destaque (overlay/borda) continua perceptível
