# Proposal

## Why

A marca (logo do cabeçalho e favicon) é um quadrado arredondado com fundo
em gradiente e borda, visualmente pesado: ocupa muito espaço visual e destoa
do restante da UI, que usa ícones de linha finos e discretos. O usuário pediu
uma marca com **fundo transparente** e mais discreta.

## What Changes

- Redesenhar `AppLogo.vue` como uma marca de **linha** (traço fino), sem
  `rect` de fundo em gradiente e sem borda — fundo transparente.
- Atualizar `public/favicon.svg` para a mesma marca em linha.
- Manter as proporções, o significado (triângulo "play" + barra) e o tamanho
  atual usado no cabeçalho (`:size="28"`).

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `web-ui`: a marca da aplicação passa a ser um traço fino com fundo
  transparente, em vez de um bloco sólido preenchido.

## Impact

- `vue/plex-frontend/src/components/AppLogo.vue` (reescrita do SVG).
- `vue/plex-frontend/public/favicon.svg` (mesma marca).
- Sem mudança de API, contrato, rota ou dependência.
- Sem impacto em testes E2E (o `AppLogo` não tem `data-testid`; acessibilidade
  via `role="img"` + `aria-label` preservada).
