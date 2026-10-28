# Proposal

## Why

Os testes e2e (Playwright) foram escritos para a stack antiga (Python/Flask) mas o projeto agora usa .NET 10 + Vue 3. Os seletores de UI, caminhos dos scripts e endpoints da API não correspondem mais à stack atual, causando 7 de 14 testes falhando mesmo quando a aplicação está funcionando corretamente.

## What Changes

- Corrigir `container.spec.js`: caminhos dos scripts `start.sh`, `stop.sh`, `logs.sh` apontam para a raiz, mas estão em `scripts/`
- Corrigir `libraries.spec.js`: seletores de UI `[data-testid="library-card"]` não existem na aplicação .NET/Vue
- Corrigir `markers.spec.js`: formulário de criação e seletores de marcador não existem na aplicação .NET/Vue; endpoint `GET /api/markers` retorna 500
- Corrigir `images.spec.js`: proxy de imagens retorna HTTP 500

## Capabilities

### New Capabilities

- (nenhum)

### Modified Capabilities

- `e2e-integration`: Não há alteração nos requisitos de comportamento dos testes e2e. A correção é apenas de implementação (seletores, caminhos, endpoints) para funcionar com a stack .NET/Vue atual.

## Impact

- Arquivos afetados: `tests/e2e/tests/container.spec.js`, `tests/e2e/tests/libraries.spec.js`, `tests/e2e/tests/markers.spec.js`, `tests/e2e/tests/images.spec.js`
- Nenhum arquivo existente além dos testes e2e é modificado
