# Spec Delta

## Purpose
Garante cobertura de teste end-to-end para o comportamento de infinite scroll no catálogo de séries, prevenindo regressões onde o carregamento automático de páginas para de funcionar.

## ADDED Requirements

### Requirement: Teste E2E de infinite scroll
O sistema SHALL possuir teste automatizado Playwright que valida o carregamento de múltiplas páginas via infinite scroll na view de séries.

#### Scenario: Infinite scroll carrega páginas subsequentes
- **WHEN** teste navega para `/libraries/{key}/series` com biblioteca que tem >24 séries
- **AND** rola a página até o fim do grid (simulando scroll até a sentinela)
- **THEN** API é chamada para página 2
- **AND** novos cards de série são renderizados no DOM
- **AND** total de cards renderizados > 24 (tamanho da primeira página)

#### Scenario: Infinite scroll para no fim do catálogo
- **WHEN** teste rola até carregar todas as séries disponíveis
- **THEN** indicador "Fim do catálogo" aparece
- **AND** nenhuma chamada adicional à API é feita ao rolar mais

#### Scenario: Busca combinada com infinite scroll
- **WHEN** teste digita termo de busca no campo de busca
- **AND** aguarda debounce e carregamento da página 1 filtrada
- **AND** rola até o fim
- **THEN** API é chamada para página 2 com o mesmo parâmetro `query`
- **AND** resultados filtrados são anexados corretamente

### Requirement: Teste roda em CI
O teste de infinite scroll SHALL ser executável em pipeline CI/CD headless sem dependência de servidor Plex real (usa mock).

#### Scenario: Teste passa com mock Plex
- **WHEN** teste roda com mock-plex-server configurado com 50 séries fictícias
- **THEN** teste completa em < 30 segundos
- **AND** valida carregamento de pelo menos 3 páginas (24 + 24 + 2 séries)
- **AND** não falha por flakiness de timing (usa waits explícitos)