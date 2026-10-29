# Spec Delta

## MODIFIED Requirements

### Requirement: Paginação server-side do catálogo

O sistema SHALL aceitar parâmetros de página e tamanho de página no endpoint de listagem de séries e SHALL realizar a paginação no banco local via SQL, retornando também a informação de total de itens para a UI paginar, sem chamar a API do Plex para cada requisição.

#### Scenario: Página solicitada

- **WHEN** o frontend solicita a página 2 com 24 itens por página
- **THEN** o sistema retorna no máximo 24 séries do banco correspondentes ao intervalo paginado e um total para habilitar controles de paginação.

#### Scenario: Consulta grande não congela a UI

- **WHEN** uma biblioteca possui milhares de séries
- **THEN** cada resposta carrega apenas a página solicitada do banco, nunca a biblioteca inteira.
