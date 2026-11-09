# Design — mobile-marker-editor

## Contexto
Refer a `proposal.md` (por que) e `specs/mobile-marker-editor/spec.md` (requisitos). O `MarkerEditorView.vue` (`/series/:seriesKey/markers`) renderiza temporadas/episódios; cada episódio mostra miniatura à esquerda (`aspect-video w-48 shrink-0`) e os marcadores intro/créditos à direita, no `MarkerRow.vue`. Em edição, `MarkerRow` exibe inputs Start/End ms e botões Salvar/Descartar em container `flex flex-wrap gap-3` com labels acima dos inputs (`flex flex-col`).

## Goals / Non-Goals

- **Goals**: imagem/thumbnail acima e campos de edição em uma linha única em viewports < 640px; layout desktop inalterado; manter data-testids e acessibilidade; tocar-friendly (mínimo 44px).
- **Non-Goals**: mudar backend/API; novo tipo de marcador; reordenar episódios; funcionalidade CRUD completa; tema escuro/light.

## Decisões

| Decisão | Rationale | Alternativa considerada |
|---------|-----------|-------------------------|
| Ajustar `MarkerEditorView.vue`: em mobile exibir miniatura com largura total acima dos controles (`lg:flex-row` → `flex-col` via classes utilitárias, sem novo CSS) | Mínimo impacto; reutiliza classes Tailwind existentes | Criar componente separado de episódio mobile — overhead desnecessário |
| `MarkerRow.vue`: remover labels acima dos inputs, substituir por `aria-label`; reduzir inputs `w-32`→`w-24`; manter ordem Início/Fim/Salvar/Descartar em linha única (`flex items-center gap-2`) | Atende "todos os campos em uma linha" e mínimo espaço horizontal em celular | Manter labels visíveis — consome largura e quebra a linha |
| Classes utilitárias inline (sem novo arquivo CSS) | Consistente com o restante do projeto (Tailwind já no ar) | Novo CSS — quebra convenção |

## Riscos / Trade-offs

- [Reduzir tamanho dos inputs] → campos podem ficar apertados para números longos → mitigate: manter `step="10"` e permitir zoom do navegador.
- [Esconder labels em mobile via `aria-label`] → leitores de tela perdem contexto visual → mitigate: `aria-label` preserva descrição textual (WCAG 2.1 SC 1.3.1, 4.1.2).
- [Largura total da miniatura em mobile] → perda do layout lado-a-lado em telas pequenas → aceitável e requerido pelo usuário.

## Migration Plan

- Build frontend `npm run build` → cópia para `wwwroot` (pipeline canônico `./scripts/build.sh`).
- Rollback: reverter commit de `MarkerEditorView.vue` e `MarkerRow.vue`.

## Open Questions

- Nenhuma.