# Spec: Library Card Series Mosaic Hover

## ADDED Requirements

### Requirement: REQ-MOSAIC-DISPLAY
**Descrição**: Cada card de biblioteca SHALL exibir um mosaico 2×3 de pôsteres de séries em vez do ícone estático de TV.

#### Scenario: Library com séries mostra mosaico
- **Given** uma biblioteca com pelo menos 1 série que tenha `thumb`
- **When** o usuário visita `/libraries`
- **Then** o card exibe até 6 pôsteres em grid 2×3
- **And** cada pôster usa `background-image: url(/api/images/...)` com o `thumb` da série

#### Scenario: Library sem séries mostra ícone neutro
- **Given** uma biblioteca com 0 séries ou sem `thumb`
- **When** o usuário visita `/libraries`
- **Then** o card exibe o ícone `tv` em `var(--text-tertiary)` (cor neutra)

### Requirement: REQ-MOSAIC-HOVER-SHUFFLE
**Descrição**: Ao passar o mouse sobre o card, o sistema SHALL embaralhar (shuffle) os pôsteres suavemente.

#### Scenario: Hover inicia shuffle
- **Given** um card com mosaico de pôsteres
- **When** o mouse entra no card
- **Then** a ordem dos pôsteres é alterada aleatoriamente a cada ~400 ms
- **And** transição suave via CSS `transform` + `opacity`

#### Scenario: Mouse leave restaura ordem original
- **When** o mouse sai do card
- **Then** o shuffle para e a ordem original (cached) é restaurada

#### Scenario: Reduced motion desabilita animação
- **Given** usuário com `prefers-reduced-motion: reduce`
- **When** passa mouse no card
- **Then** não há shuffle nem transição

### Requirement: REQ-MOSAIC-PERFORMANCE
**Descrição**: O carregamento SHALL ser não bloqueante e MUST manter cache de resultados.

#### Scenario: Cache evita refetch
- **Given** biblioteca já foi carregada uma vez
- **When** o usuário revisita `/libraries` na mesma sessão
- **Then** o mosaic usa dados do cache em memória sem nova requisição HTTP