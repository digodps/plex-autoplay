# Tasks

## 1. Correção do achado [7]

- [x] 1.1 Aplicar correção nos arquivos: src/PlexAutoplay.Web/Services/PlexApiClient.cs e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

Falso positivo confirmado: `new Meter(...)`/`CreateCounter` (singletons estáticos de telemetria) e `new HttpRequestMessage(...)` (BCL). Nenhuma dependência injetável; documentado e arquivado sem alteração de código.

## Workflow follow-up

- Validar com `openspec validate --change fix-7-clean-code-/-solid` antes de seguir para archive/apply.
