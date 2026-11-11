# Spec Delta

## Purpose

Alterar a última migalha de pão do breadcrumb na rota `series-markers` para exibir o nome dinâmico da biblioteca (ex.: "Animes") quando disponível, mantendo apenas como fallback o nome fixo "Biblioteca".

## ADDED Requirements

### Requirement: Última migalha da página de marcadores usa nome dinâmico da biblioteca
O sistema SHALL, na rota `series-markers`, exibir como última migalha do breadcrumb o nome da biblioteca (ex.: "Animes") quando a biblioteca estiver disponível, em vez do nome fixo "Biblioteca".

#### Scenario: Página de marcadores com biblioteca disponível e sem título da série
- **WHEN** o usuário acessa a página de marcadores de uma série e a biblioteca está disponível (ex.: "Animes") mas sem título da série na query
- **THEN** o breadcrumb exibe "Bibliotecas > Animes > Animes" (último item é o nome dinâmico da biblioteca)

#### Scenario: Página de marcadores sem título da série e sem biblioteca
- **WHEN** o usuário acessa a página de marcadores sem metadados de biblioteca nem título da série
- **THEN** o breadcrumb exibe o nome fixo "Biblioteca" como último recurso

#### Scenario: Página de marcadores com título da série
- **WHEN** o usuário acessa a página de marcadores com título da série informado (ex.: "Aosh")
- **THEN** o breadcrumb exibe "Bibliotecas > Animes > Aosh" (último item é o título da série)
