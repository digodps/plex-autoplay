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
O sistema SHALL exibir um breadcrumb no topo da página de séries permitindo retorno à lista de bibliotecas com um clique, sem depender do botão "voltar" do navegador.

#### Scenario: Breadcrumb exibe hierarquia e permite retorno
- **WHEN** usuário está na página `/libraries/{key}/series`
- **THEN** breadcrumb mostra `[← Bibliotecas] {Título da Biblioteca}`
- **AND** clicar em "Bibliotecas" navega para `/libraries`
- **AND** título da biblioteca ativa é exibido mas não é clicável (estado atual)

#### Scenario: Breadcrumb acessível
- **WHEN** leitor de tela navega pelo breadcrumb
- **THEN** estrutura semântica `<nav aria-label="Breadcrumb">` com `<ol>` e `<li>` é usada
- **AND** link de retorno tem texto acessível "Voltar às bibliotecas"

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