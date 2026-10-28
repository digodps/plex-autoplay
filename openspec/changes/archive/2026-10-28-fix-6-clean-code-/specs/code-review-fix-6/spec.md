# Spec Delta

## Purpose

Corrigir o achado [6] da revisão de código: Classe com mais de 200 linhas (315) - possível violação do SRP (Responsabilidade Única)

## ADDED Requirements

### Requirement: Corrigir achado [6]
O sistema SHALL corrigir o problema: Classe com mais de 200 linhas (315) - possível violação do SRP (Responsabilidade Única)

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos src/PlexAutoplay.Web/Services/PlexApiClient.cs
- **THEN** o achado [6] não é mais reportado na próxima execução de /code-review
