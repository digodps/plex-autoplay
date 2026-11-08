# web-ui Specification (Delta)

## REMOVED Requirements

### Requirement: Menu lateral em português

**Reason**: O menu lateral foi removido.

**Migration**: Nenhuma ação do usuário.

### Requirement: Espaçamento do ícone

**Reason**: Requisito exclusivo dos itens do menu lateral removido.

**Migration**: Nenhuma.

## ADDED Requirements

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
