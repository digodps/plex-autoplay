# Proposal — maker-tela-mobile

## Why

A tela de edição de marcadores (`MarkerEditorView.vue`) exibe a miniatura do episódio à esquerda e os controles de marcador (intro/créditos) à direita em layout horizontal. Em dispositivos móveis, isso força os campos de edição (Início/Fim ms) a quebrar em múltiplas linhas ou ficar ilegíveis, dificultando a edição rápida de marcadores. O usuário precisa de layout responsivo: imagem/thumbnail **acima** (largura total) e todos os campos de edição **abaixo em uma única linha** horizontal.

## What Changes

- **Layout mobile-first**: no `MarkerEditorView.vue`, em viewports < 640px, a miniatura do episódio ocupa largura total (topo) e os marcadores (intro/créditos) ficam empilhados abaixo em coluna.
- **MarkerRow responsivo**: em modo edição, os dois inputs (Início/Fim ms) + botões Salvar/Descartar exibidos em **uma única linha flexível** (wrap controlado) em qualquer largura; labels condensados ou tooltip-only.
- **Manter layout desktop inalterado**: side-by-side imagem + controles (comportamento atual).
- **Não alterar API, backend, schema** — mudança puramente de apresentação/frontend.

## Capabilities

### New Capabilities

- `mobile-marker-editor`: layout responsivo mobile do editor de marcadores — imagem acima e campos de edição em uma linha.
- Sem mudanças em backend, banco, CI, Docker.