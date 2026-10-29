# Spec Delta

## MODIFIED Requirements

### Requirement: Listar séries de uma biblioteca
O sistema SHALL expor um endpoint que liste as séries de uma biblioteca do tipo `show`, contendo rating key, título, descrição (summary) e capa (thumb) de cada série.

#### Scenario: Lista de séries retornada com sucesso
- **WHEN** cliente requisita GET /api/series/{sectionKey} com biblioteca válida
- **THEN** resposta 200 contém array de séries com ratingKey, title, summary, thumb

#### Scenario: Biblioteca não encontrada ou sem acesso
- **WHEN** cliente requisita biblioteca inexistente ou sem permissão
- **THEN** resposta 404 ou 502 com mensagem de erro apropriada

### Requirement: Paginação server-side do catálogo
O sistema SHALL aceitar parâmetros de página e tamanho de página no endpoint de listagem de séries e SHALL delegar a paginação ao Plex via cabeçalhos de container (X-Plex-Container-Start/Size), retornando também a informação de total de itens para a UI paginar.

#### Scenario: Paginação funcionando
- **WHEN** cliente requisita com page=2 e pageSize=10
- **THEN** resposta contém items da página 2, totalSize, page=2, pageSize=10

#### Scenario: Tamanho de página respeita limites
- **WHEN** cliente requisita pageSize=200
- **THEN** resposta usa pageSize=100 (limite máximo)

### Requirement: Buscar séries por nome
O sistema SHALL aceitar um termo de busca (query) no endpoint de listagem de séries e SHALL filtrar as séries por correspondência de título, mantendo a paginação aplicada se possível.

#### Scenario: Busca com resultado
- **WHEN** cliente requisita com query="breaking"
- **THEN** resposta contém apenas séries com "breaking" no título

#### Scenario: Busca sem resultado
- **WHEN** cliente requisita com query="xyz123nonexistent"
- **THEN** resposta contém array vazio e totalSize=0

### Requirement: Listar episódios de uma série
O sistema SHALL expor um endpoint que liste os episódios de uma série achatados (temporada, número do episódio, título, rating key e caminho local resolvido), agrupados no cliente por temporada.

#### Scenario: Episódios listados com caminho local resolvido
- **WHEN** cliente requisita GET /api/series/{sectionKey}/{seriesKey}/episodes
- **THEN** resposta 200 contém array de episódios com rating_key, title, season, episode, local_path

#### Scenario: Série sem episódios
- **WHEN** cliente requisita episódios de série vazia
- **THEN** resposta 200 contém array vazio

## REMOVED Requirements

### Requirement: Exibir séries em view separada com busca e paginação
**Reason**: A visualização de séries foi movida para dentro da view de Bibliotecas (inline expansion), eliminando a necessidade de uma página separada `/series` com busca global e paginação independente.
**Migration**: A funcionalidade de busca e paginação é mantida, mas agora opera dentro do contexto da biblioteca selecionada na view de Bibliotecas. O endpoint `/api/series/{sectionKey}` continua a ser usado.

### Requirement: Rota `/series` para listagem global de todas as séries
**Reason**: Removida para simplificar navegação. Usuário seleciona biblioteca primeiro, depois vê suas séries.
**Migration**: Navegação direta para `/libraries` → clica na biblioteca → vê séries inline.