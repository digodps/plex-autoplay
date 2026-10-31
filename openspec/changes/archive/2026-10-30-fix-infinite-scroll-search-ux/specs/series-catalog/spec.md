# Spec Delta

## MODIFIED Requirements

### Requirement: Buscar séries por nome
O sistema SHALL aceitar um termo de busca (query) no endpoint de listagem de séries e SHALL filtrar as séries por correspondência de título, mantendo a paginação aplicada se possível. **A busca deve funcionar em tempo real (debounced) na view de página completa de séries.**

#### Scenario: Busca com correspondência
- **WHEN** o usuário busca "Arrow" na página de séries
- **THEN** o sistema retorna apenas séries cujo título contém o termo informado.

#### Scenario: Busca sem correspondência
- **WHEN** o usuário busca um termo que não casa com nenhuma série
- **THEN** o sistema retorna lista vazia (a UI exibe estado vazio, não erro).

#### Scenario: Filtro em tempo real ao digitar na view de séries
- **WHEN** usuário digita no campo de busca na página `/libraries/{key}/series`
- **THEN** após 300ms de debounce, sistema chama API com parâmetro `query`
- **AND** resultados são filtrados no servidor e retornados na primeira página
- **AND** paginação é reiniciada (página 1)
- **AND** indicador de carregamento é exibido durante a busca

#### Scenario: Limpar busca restaura catálogo completo
- **WHEN** usuário limpa o campo de busca
- **THEN** sistema chama API sem parâmetro `query`
- **AND** catálogo completo é exibido a partir da página 1