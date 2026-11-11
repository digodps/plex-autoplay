# Proposal: Breadcrumb de biblioteca dinâmica na página de marcadores

## Why

Na página de marcadores de uma série (`series-markers`), a última migalha do breadcrumb usa um nome fixo ("Biblioteca") quando o título da série não é informado, em vez de usar o nome dinâmico da biblioteca (ex.: "Animes"). Isso deixa a trilha com um rótulo genérico quando a biblioteca está disponível.

## What Changes

- Em `src/lib/breadcrumb.js`, rota `series-markers`: mudar o último item da trilha de `text(query.seriesTitle) || "Biblioteca"` para `text(query.seriesTitle) || lib || "Biblioteca"`, de modo que, sem título da série, exiba o nome da biblioteca (ex.: "Animes") e somente como último recurso o fixo "Biblioteca".

## Capabilities

### New Capabilities
- None

### Modified Capabilities
- `breadcrumb`: última migalha da página de marcadores passa a mostrar o nome dinâmico da biblioteca quando disponível.

## Impact

- Apenas `src/lib/breadcrumb.js` (uma linha). Sem alteração de rotas, endpoints, bancos ou outros componentes.
