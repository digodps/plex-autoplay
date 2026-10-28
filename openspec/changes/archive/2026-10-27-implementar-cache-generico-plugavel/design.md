# Design

## Context

O `ImageCacheService` em `PlexAutoplay.Infrastructure.Cache` depende diretamente de `IMemoryCache` (`Microsoft.Extensions.Caching.Memory`), não está registrado no container de DI e não é consumido pelo `ImageProxyService`. O spec `plex-images` já exige cache de imagens proxy, mas o comportamento não existe no código. A aplicação não tem hoje uma abstração de cache reusável por outros serviços (sync, bibliotecas, etc.). Ver `proposal.md - Why` para a motivação completa.

## Goals / Non-Goals

**Goals:**
- Porta de cache genérica no Domain, sem dependência de provedor concreto
- Implementação in-memory (`IMemoryCache`) atrás da porta, como primeiro backend
- Registro da porta e implementação no DI container (`Program.cs`)
- `ImageProxyService` consumindo a porta, servindo imagens repetidas sem chamar o Plex
- Possibilidade de adicionar Redis/disco depois como novas implementações da porta, sem tocar na aplicação

**Non-Goals:**
- Implementar backend Redis ou disco nesta mudança (apenas a abstração + in-memory)
- Sincronização de cache entre múltiplas instâncias/nós
- Políticas de invalidação além de expiração por tempo (TTL)
- Cache distribuído ou sharding

## Decisions

**1. Porta `ICacheService` no `PlexAutoplay.Domain.Ports`**
- Rationale: segue o padrão já estabelecido pelo projeto (`IMarkerRepository`, `IMarkerDetector`) — interface no Domain, implementação no Infrastructure, composição no Web
- Alternativa considerada: usar `IDistributedCache` do .NET diretamente (rejeitado: trava a aplicação à API do .NET e à serialização `string`/`byte[]`, menos alinhado ao padrão de portas existente)

**2. Interface simples: `Get(string key)`, `Set(string key, byte[] data, TimeSpan? expiry)`, `Remove(string key)`**
- Rationale: cobre o uso atual de imagens (`byte[]`) e é genérica o suficiente para outros tipos no futuro via serialização do chamador
- Alternativa considerada: interface genérica `ICache<TKey, TValue>` (rejeitado por agora: adiciona indireção de serialização antes de haver necessidade real)

**3. Backend in-memory via `IMemoryCache` como implementação inicial**
- Rationale: já está disponível no projeto, sem nova dependência, atende ao cenário dev/single-node
- Alternativa considerada: Redis desde o início (rejeitado: adiciona infraestrutura obrigatória antes de haver demanda)

**4. Registro no DI como singleton em `Program.cs`**
- Rationale: cache é estado compartilhado da aplicação; o consumidor injeta `ICacheService` pelo construtor

## Risks / Trade-offs

- [Backend específico (Redis connection string, caminho de disco) vazar para a aplicação] → Mitigar mantendo configuração de backend dentro da implementação concreta; a aplicação vê apenas `ICacheService`
- [Serialização de valores complexos além de `byte[]` não definida] → Mitigar deixar a serialização a cargo do chamador; revisar se surgir outro tipo de dado
- [Concorrência no acesso ao cache in-memory] → Mitigar `IMemoryCache` já é thread-safe; implementações futuras cuidam da própria concorrência

## Open Questions

- Qual o escopo de vida do cache no DI (singleton vs scoped)? Afeta se cache é compartilhado entre requests ou por request.
- Deveria haver um mecanismo de limpeza/eviction além de expiração absoluta?
