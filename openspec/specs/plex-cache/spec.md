# plex-cache Specification

## Purpose
Fornece uma camada de cache genérica e plugável que permite armazenar e recuperar pares chave/valor com expiração, possibilitando trocar o provedor de cache (memória, Redis, disco) sem alterar o código da aplicação consumidora.

## Requirements

### Requirement: Cache genérico acessível pela aplicação
O sistema SHALL expor uma abstração de cache que permita obter, armazenar e remover entradas por chave, com expiração opcional, sem que o código da aplicação conheça o provedor concreto.

#### Scenario: Gravação e leitura de entrada no cache
- **WHEN** a aplicação armazena um valor sob uma chave com expiração de 30 minutos
- **THEN** uma leitura subsequente da mesma chave retorna o valor armazenado

#### Scenario: Entrada expirada não é retornada
- **WHEN** uma entrada de cache excede seu tempo de expiração
- **THEN** a leitura da chave indica ausência (miss)

### Requirement: Troca de provedor sem impacto na aplicação
O sistema SHALL permitir substituir a implementação do provedor de cache (ex: in-memory por Redis ou disco) alterando apenas o registro de serviços, sem modificar o código da aplicação.

#### Scenario: Mesma operação com backend diferente
- **WHEN** a aplicação solicita leitura ou gravação de cache com um backend Redis configurado
- **THEN** a operação é executada pelo backend Redis sem alterar o código chamador

#### Scenario: Registro de novo backend no DI
- **WHEN** uma nova implementação do contrato de cache é adicionada e registrada no container de DI
- **THEN** a aplicação passa a usar o novo backend sem alterar use cases nem endpoints
