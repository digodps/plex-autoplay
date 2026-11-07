# Spec: ajustar-menu-sidebar

## ADDED Requirements

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
