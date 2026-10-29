# Spec Delta

## Purpose

Corrigir o achado [10] da revisão de código: Classe com mais de 200 linhas (246) - possível violação do SRP (Responsabilidade Única)

## ADDED Requirements

### Requirement: Corrigir achado [10]
O sistema SHALL corrigir o problema: Classe com mais de 200 linhas (246) - possível violação do SRP (Responsabilidade Única)

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos vue/plex-frontend/src/components/StatusCard.vue
- **THEN** o achado [10] não é mais reportado na próxima execução de /code-review
