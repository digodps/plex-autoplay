# Proposal

## Why

A camada de cache do `plex-autoplay` (`ImageCacheService`) está acoplada diretamente a `IMemoryCache` do `Microsoft.Extensions.Caching.Memory`, não está registrada no container de DI e não é consumida pelo `ImageProxyService`, embora o spec `plex-images` já exija que imagens proxy sejam cacheadas. Como consequência, não é possível trocar o provedor de cache (ex: in-memory → Redis → disco) sem mexer na aplicação, e o requisito de cache do spec não está implementado no código.

## What Changes

- Adicionar porta de cache genérica no Domain (`ICacheService`), desacoplada de qualquer provedor concreto
- Implementar `ImageCacheService` no Infrastructure usando `IMemoryCache` como detalhe de implementação, atrás da porta
- Registrar a porta e a implementação no DI container (`Program.cs`)
- Fazer `ImageProxyService` usar o cache via porta, servindo imagens repetidas sem chamar o Plex
- Manter a possibilidade de trocar o backend (Redis, disco, etc.) adicionando novas implementações da porta, sem alterar a aplicação

## Capabilities

### New Capabilities

- `plex-cache`: Camada de cache genérica e plugável. Define uma abstração de cache (chave → valor, expiração) que pode ser implementada por in-memory, Redis, disco ou outro backend, sem impactar a aplicação consumidora.

### Modified Capabilities

- (nenhum) O comportamento exigido pelo spec `plex-images` (cachear imagens proxy) não muda; apenas a forma como é implementado passa a ser intercambiável.

## Impact

- `PlexAutoplay.Domain`: nova porta `ICacheService` (em `Ports/`)
- `PlexAutoplay.Infrastructure`: implementação `ImageCacheService` atrás da porta + registro no DI
- `PlexAutoplay.Web`: `Program.cs` registro de serviços e `ImageProxyService` consumindo a porta
- Dependências: nenhuma nova; `IMemoryCache` já está disponível via `Microsoft.Extensions.Caching.Memory`
