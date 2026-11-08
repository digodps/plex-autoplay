# Change Proposal — series-hover-highlight

## Why

O grid de séries hoje só destaca a borda do card no hover. A miniatura — elemento visual dominante — fica estática, então o alvo de clique não ganha destaque perceptível. Usuário pediu hover que destaque a miniatura da série.

## What Changes

- Card de série em `/libraries/:key/series` ganha destaque visual no hover/focus-visible: miniatura com zoom sutil e overlay de realce; borda/anel de foco continua.
- Sem mudança de dados, rotas ou API.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `series-full-page-view`: adiciona requisito de feedback visual de hover/focus na miniatura do card de série.

## Impact

- `vue/plex-frontend/src/views/Series/SeriesView.vue`
- `openspec/changes/series-hover-highlight/**`

Backend intocado.
