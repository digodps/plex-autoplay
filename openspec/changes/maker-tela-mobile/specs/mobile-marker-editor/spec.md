# Spec — mobile-marker-editor

## Purpose
Define o comportamento responsivo do editor de marcadores (intro/créditos) em dispositivos móveis: imagem/thumbnail acima e campos de edição em linha única.

## ADDED Requirements

### Requirement: Episode thumbnail above controls on mobile
O sistema SHALL exibir a miniatura do episódio com largura total acima dos controles de marcador quando o editor for acessado em viewport < 640px, mantendo a imagem à esquerda apenas em desktop (≥ 640px).

#### Scenario: Thumbnail above controls on mobile viewport
- **WHEN** o usuário abre a tela de edição de marcadores (`/series/:seriesKey/markers`) em largura 375px
- **THEN** cada episódio exibe o `div` de miniatura (`/api/images/...`) em largura total, acima dos controles de intro e créditos
- **AND** os controles de marcador empilham verticalmente abaixo da miniatura

#### Scenario: Episode without thumbnail displays fallback icon on mobile
- **WHEN** o episódio não possui miniatura disponível em viewport < 640px
- **THEN** o container à esquerda exibe um ícone de fallback no mesmo espaço reservado (largura total)

### Requirement: Marker editing fields in a single horizontal line
O sistema SHALL exibir todos os campos de edição de um marcador (Start ms, End ms, Salvar, Descartar) em uma única linha horizontal quando o usuário entra em modo de edição.

#### Scenario: Editing fields in single line on mobile
- **WHEN** o usuário clica para editar um marcador (intro ou créditos) em viewport < 640px
- **THEN** os dois inputs numéricos e os dois botões são renderizados em uma linha flex horizontal, sem quebra de linha
- **AND** cada input exibe `aria-label` ("Início (ms)" / "Fim (ms)") para acessibilidade
- **AND** cada input/botão tem alvo de toque mínimo de 44px

#### Scenario: Editing fields side-by-side on desktop
- **WHEN** o usuário edita um marcador em viewport ≥ 640px
- **THEN** os campos permanecem lado a lado com labels visíveis (comportamento atual)

### Requirement: Touch-friendly editing
O sistema SHALL garantir que inputs e botões de edição tenham alvo de toque mínimo de 44px em qualquer viewport.

#### Scenario: Minimum touch target
- **WHEN** o modo de edição está ativo
- **THEN** inputs e botões têm `min-height: 44px` e `min-width: 44px`
- **AND** há espaçamento ≥ 8px entre elementos interativos