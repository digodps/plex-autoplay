# Proposal

## Why

O backend do plex-autoplay já possui a estrutura de 4 camadas e o grafo de
dependência corretos, mas a implementação fura as regras de Clean Architecture,
SOLID e Clean Code: adaptadores concretos vivem na Web e no Domain, endpoints
acessam repositório direto e duplicam a regra de scan, há código morto não
fiado, `catch` nu engolindo erro e sync-over-async (`.Wait()`/`.Result`) em
repositórios, endpoints e serviço. Isso gera risco de deadlock, divergência de
regra entre caminhos paralelos e dificuldade de teste/evolução.

## What Changes

- **Camadas**: todo adaptador concreto (HTTP, filesystem, config, job store)
  migra para Infrastructure; Domain e Application ficam com portas e casos de
  uso puros.
- **Casos de uso**: endpoints (leitura e escrita) passam a consumir casos de uso
  de Application; `SaveMarkerUseCase` retorna resultado para mapear 201/409;
  novo `LibraryScanUseCase` elimina a regra de scan duplicada.
- **Inicialização**: `DatabaseInitializer` assíncrono executa schema dos 4
  repositórios no composition root; construtores sem DDL/`.Wait()`.
- **Higiene**: zero `catch` nu, zero sync-over-async, parsing de contrato
  tipado, código morto removido, `.env` atrás de porta.
- **Enforcement**: teste de arquitetura reprova build em violação de dependência.

## Impact

- **Affected specs**: `backend-architecture` (nova).
- **Affected code**: os 4 projetos .NET — `Domain` (1 arquivo sai), `Application`
  (novos casos de uso/portas), `Infrastructure` (novos adaptadores +
  `DatabaseInitializer`), `Web` (endpoints viram fachada; 1 endpoint morto sai).
- **Contrato HTTP**: preservado (mesmas rotas, métodos e formatos de resposta;
  `/api/markers` continua aceitando query e corpo).
- **Comportamento**: preservado; 13 testes e2e Playwright devem seguir verdes.

> Rollback: `git revert` do commit; tag `pre-clean-arch` criada antes.
