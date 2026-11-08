# series-full-page-view Specification (Delta)

## MODIFIED Requirements

### Requirement: Breadcrumb de navegação

O sistema SHALL exibir a trilha completa da rota no topo da página de séries,
permitindo retorno à lista de bibliotecas e à biblioteca ativa com um clique, sem
depender do botão "voltar" do navegador.

#### Scenario: Breadcrumb exibe hierarquia e permite retorno

- **WHEN** usuário está na página `/libraries/{key}/series`
- **THEN** breadcrumb mostra `Início > Bibliotecas > {Título da Biblioteca}`
- **AND** clicar em "Início" navega para `/dashboard`
- **AND** clicar em "Bibliotecas" navega para `/libraries`
- **AND** título da biblioteca ativa é exibido sem link (estado atual)

#### Scenario: Breadcrumb acessível

- **WHEN** leitor de tela navega pelo breadcrumb
- **THEN** estrutura semântica `<nav aria-label="Breadcrumb">` com `<ol>` e `<li>`
  é usada
- **AND** o item atual tem `aria-current="page"`
