# Spec Delta

## MODIFIED Requirements

### Requirement: Discover Plex libraries
O sistema SHALL consultar a API do Plex Media Server para listar todas as seções de biblioteca disponíveis.

#### Scenario: Lista de bibliotecas retornada
- **WHEN** cliente requisita GET /api/plex/libraries
- **THEN** resposta 200 contém array de bibliotecas com key, title, type

### Requirement: Filter to series libraries only
O sistema SHALL incluir apenas bibliotecas do tipo "show" (séries) no conjunto sincronizado.

#### Scenario: Apenas bibliotecas do tipo show
- **WHEN** cliente lista bibliotecas
- **THEN** resposta contém apenas bibliotecas com type="show"

### Requirement: Map episodes to local media files
O sistema SHALL listar os episódios de cada biblioteca de séries e resolver o caminho do arquivo de mídia de cada episódio usando o valor MediaPart.file do Plex e a MEDIA_ROOT configurada.

#### Scenario: Caminho local resolvido corretamente
- **WHEN** sistema processa episódios de uma biblioteca
- **THEN** cada episódio tem local_path resolvido via MEDIA_ROOT

## ADDED Requirements

### Requirement: Exibir séries inline na view de bibliotecas
O sistema SHALL permitir que o usuário expanda uma biblioteca na view de Bibliotecas para visualizar suas séries diretamente, com paginação e busca, sem navegar para uma view separada.

#### Scenario: Expansão inline de biblioteca
- **WHEN** usuário clica em uma biblioteca na grid
- **THEN** a biblioteca expande inline mostrando séries paginadas daquela biblioteca

#### Scenario: Busca dentro da biblioteca expandida
- **WHEN** usuário digita termo de busca na biblioteca expandida
- **THEN** séries são filtradas por título mantendo paginação

#### Scenario: Paginação dentro da biblioteca expandida
- **WHEN** usuário navega páginas na biblioteca expandida
- **THEN** carrega próxima/página anterior de séries daquela biblioteca

#### Scenario: Colapso da biblioteca
- **WHEN** usuário clica novamente na biblioteca expandida ou em outra
- **THEN** biblioteca colapsa e grid de bibliotecas volta ao estado original