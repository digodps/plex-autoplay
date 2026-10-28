# Spec Delta

## Purpose

Corrigir o achado [1] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## ADDED Requirements

### Requirement: Corrigir achado [1]
O sistema SHALL corrigir o problema: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs
- **THEN** o achado [1] não é mais reportado na próxima execução de /code-review
