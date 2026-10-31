# series-catalog Specification

## Purpose

Permitir ao usuário navegar pelo catálogo de séries de cada biblioteca Plex do tipo `show`, com capa, título e descrição, em listas paginadas no servidor, e consultar os episódios de uma série para editá-la.

## Requirements

### Requirement: Listar séries de uma biblioteca
O sistema SHALL expor um endpoint que liste as séries de uma biblioteca do tipo `show`, contendo rating key, título, descrição (summary) e capa (thumb) de cada série.

#### Scenario: Listar séries com metadados
- **WHEN** o frontend solicita a lista de séries de uma biblioteca
- **THEN** o sistema retorna séries com rating key, título, descrição e URL de capa que passa pelo proxy de imagens interno (sem expor o token do Plex).

#### Scenario: Capa indisponível
- **WHEN** uma série não possui thumb no Plex
- **THEN** a lista ainda inclui a série, com a capa ausente (o frontend deve tratar casos sem capa).

### Requirement: Paginação server-side do catálogo
O sistema SHALL aceitar parâmetros de página e tamanho de página no endpoint de listagem de séries e SHALL delegar a paginação ao Plex via cabeçalhos de container (X-Plex-Container-Start/Size), retornando também a informação de total de itens para a UI paginar.

#### Scenario: Página solicitada
- **WHEN** infinite scroll solicita próxima página com pageSize=24
- **THEN** o sistema retorna no máximo 24 séries correspondentes ao intervalo paginado e um total para habilitar controles de paginação

#### Scenario: Consulta grande não congela a UI
- **WHEN** uma biblioteca possui milhares de séries
- **THEN** cada resposta carrega apenas a página solicitada, nunca a biblioteca inteira.

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

### Requirement: Listar episódios de uma série
O sistema SHALL expor um endpoint que liste os episódios de uma série achatados (temporada, número do episódio, título, rating key e caminho local resolvido), agrupados no cliente por temporada.

#### Scenario: Abrir série no editor
- **WHEN** o usuário clica numa série na página de catálogo
- **THEN** o frontend obtém a lista de episódios da série com seus rating keys, permitindo localizar marcadores por episódio.

#### Scenario: Série sem episódios
- **WHEN** a série não tem episódios disponíveis no Plex
- **THEN** o sistema retorna lista vazia e a UI exibe estado vazio.
