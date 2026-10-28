# Proposal

## Why

Correção do achado [11] da revisão de código: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)

## What Changes

- Corrigir o problema identificado: Instanciação direta de dependências concretas detectada - possível violação do DIP (Inversão de Dependência)
- Arquivos afetados: src/PlexAutoplay.Web/wwwroot/assets/_plugin-vue_export-helper-CGgr_-gC.js

## Capabilities

### New Capabilities

- code-review-fix-11: Correção específica do achado [11] da skill /code-review

### Modified Capabilities

- (nenhum)

## Impact

- Arquivos: src/PlexAutoplay.Web/wwwroot/assets/_plugin-vue_export-helper-CGgr_-gC.js
- Referências:   SOLID - D (Dependency Inversion Principle)

## Resultado da aplicação

Falso positivo confirmado: o arquivo era bundle minificado gerado em wwwroot/assets (código de build, não fonte). Resolvido excluindo artefatos de build do scan da skill (EXCLUDED_PATH_PREFIXES) e dos linters (.prettierignore/eslint ignores). Sem alteração de código-fonte.
