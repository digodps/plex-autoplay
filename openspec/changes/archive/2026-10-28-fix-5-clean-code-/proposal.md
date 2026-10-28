# Proposal

## Why

Correção do achado [5] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## What Changes

- Corrigir o problema identificado: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)
- Arquivos afetados: src/PlexAutoplay.Web/Program.cs

## Capabilities

### New Capabilities

- code-review-fix-5: Correção específica do achado [5] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/Program.cs
- Referências:   SOLID - D (Dependency Inversion Principle)

## Resultado da aplicação

Falso positivo confirmado: `new` apenas no bootstrap de aplicação (Serilog, builder, app de configuração em Program.cs) — código de composição raiz, onde a DI ocorre justamente. Documentado e arquivado sem alteração de código.
