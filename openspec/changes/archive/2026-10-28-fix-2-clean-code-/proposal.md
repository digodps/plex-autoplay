# Proposal

## Why

Correção do achado [2] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## What Changes

- Corrigir o problema identificado: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)
- Arquivos afetados: src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs

## Capabilities

### New Capabilities

- code-review-fix-2: Correção específica do achado [2] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs
- Referências:   SOLID - D (Dependency Inversion Principle)

## Resultado da aplicação

Falso positivo confirmado: o único `new` da classe é `new SqliteConnection(_connectionString)` (factory de conexão sobre string de configuração) e DTOs anônimos do Dapper. Nenhuma dependência injetável é instanciada; refatorar adicionaria abstração sem benefício. Documentado e arquivado sem alteração de código.
