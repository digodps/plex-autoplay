# Spec Delta

## Purpose

Remove o título duplicado da biblioteca ("Biblioteca → {título}") e do subtítulo ("Biblioteca selecionada.") da página de séries, deixando exclusivamente a migalha de pão do header (AppHeader breadcrumb).

## ADDED Requirements

### Requirement: Página de séries exibe apenas migalha do header
O sistema SHALL remover o título de página "Biblioteca → {título}" e o subtítulo "Biblioteca selecionada." da página `/libraries/{key}/series`, mantendo apenas a migalha de pão do header (AppHeader breadcrumb, gerada por `buildBreadcrumb`).

#### Scenario: Página de séries sem título duplicado
- **WHEN** o usuário acessa a página de séries de uma biblioteca
- **THEN** a página não exibe o texto "Biblioteca → {título}" nem o subtítulo "Biblioteca selecionada."

#### Scenario: Migalha de pão do header mantida
- **WHEN** o usuário acessa a página de séries de uma biblioteca
- **THEN** o header exibe a migalha de pão "Bibliotecas > {título da biblioteca}" via `buildBreadcrumb`
