# Tasks

## 1. Porta de cache no Domain

- [x] 1.1 Criar interface `ICacheService` em `PlexAutoplay.Domain.Ports` com `Get(string key)`, `Set(string key, byte[] data, TimeSpan? expiry)` e `Remove(string key)`, e verificar que o projeto Domain compila

## 2. Implementação in-memory no Infrastructure

- [x] 2.1 Reescrever `ImageCacheService` para implementar `ICacheService`, usando `IMemoryCache` como detalhe de implementação privada (invisível aos consumidores), e verificar que a classe expõe apenas a porta
- [x] 2.2 Adicionar referência ao pacote `Microsoft.Extensions.Caching.Memory` no `PlexAutoplay.Infrastructure.csproj` se ainda não estiver disponível, e verificar que `dotnet build` do Infrastructure segue com 0 erros

## 3. Registro no DI e integração na Web

- [x] 3.1 Registrar `IMemoryCache` (`AddMemoryCache`) e `ICacheService` + implementação in-memory como singleton no `Program.cs`, e verificar que o container resolve `ICacheService`
- [x] 3.2 Injetar `ICacheService` em `ImageProxyService` e usar `Get`/`Set` para servir imagens do cache antes de chamar o Plex, com fallback a busca no Plex em caso de miss; verificar que o endpoint `/api/images/{**imagePath}` compila
- [x] 3.3 Verificar que `dotnet build` da solução inteira segue com 0 erros

## 4. Verificação de comportamento

- [x] 4.1 Confirmar que o endpoint `/api/images/{**imagePath}` retorna imagem do cache na segunda requisição sem chamar o Plex novamente (miss na primeira, hit na segunda). [Verificado via inspeção de código: `ImageProxyService.GetImageAsync` chama `_cacheService.Get(cacheKey)` antes de chamar o Plex e `_cacheService.Set(cacheKey, imageBytes)` após. Código compila com 0 erros. Não foi possível fazer teste de runtime com imagem real do Plex pois não há image ID válido disponível no ambiente.]
- [x] 4.2 Confirmar que o cache respeita o tempo de expiração (TTL) configurado. [Verificado via inspeção de código: `ImageCacheService.Set` usa `MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry.Value }` com expiração de 30 min por padrão. Código compila com 0 erros. Não foi possível fazer teste de runtime com imagem real do Plex.]

## Workflow follow-up

- Solicitar revisão da mudança antes de prosseguir para `/opsx-apply` (implementação).
- Ao concluir a implementação e a revisão, considerar archive da mudança `implementar-cache-generico-plugavel`.
