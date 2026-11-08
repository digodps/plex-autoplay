## ADDED Requirements

### Requirement: Destaque da miniatura no hover e foco

O sistema SHALL destacar visualmente a miniatura do card de série quando o usuário passa o mouse sobre o card ou o foca via teclado, mantendo a navegação e o layout do grid inalterados.

#### Scenario: Hover destaca a miniatura

- **WHEN** o usuário posiciona o mouse sobre um card de série em `/libraries/{key}/series`
- **THEN** a miniatura da série recebe realce visual (leve zoom e/ou overlay)
- **AND** o card continua clicável e navega para `/series/{seriesKey}/markers`

#### Scenario: Foco por teclado destaca a miniatura

- **WHEN** o usuário navega com Tab até um card de série
- **THEN** a miniatura apresenta o mesmo realce visual do hover
- **AND** o indicador de foco permanece visível

#### Scenario: Movimento reduzido

- **WHEN** o usuário tem `prefers-reduced-motion: reduce`
- **THEN** o realce não usa zoom animado
- **AND** o destaque (overlay/borda) continua perceptível
