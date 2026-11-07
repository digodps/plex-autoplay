# Delta: navegacao-lateral

## ADDED Requirements

### Requirement: Item ativo cobre rotas filhas mapeadas

O menu lateral SHALL destacar um único item também nas rotas filhas de uma
seção, usando prefixos explícitos por item.

#### Scenario: Marcadores de uma série destacam Bibliotecas

- **WHEN** o usuário está em `/series/{seriesKey}/markers`
- **THEN** o item "Bibliotecas" é o único item com `aria-current="page"` e
  destaque visual

#### Scenario: Séries de uma biblioteca destacam Bibliotecas

- **WHEN** o usuário está em `/libraries/{key}/series`
- **THEN** o item "Bibliotecas" é o único item com `aria-current="page"` e
  destaque visual
