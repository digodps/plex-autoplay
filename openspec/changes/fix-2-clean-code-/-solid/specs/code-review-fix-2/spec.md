# Spec Delta

## Purpose

Corrigir o achado [2] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## ADDED Requirements

### Requirement: Corrigir achado [2]
O sistema SHALL corrigir o problema: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs
- **THEN** o achado [2] não é mais reportado na próxima execução de /code-review
