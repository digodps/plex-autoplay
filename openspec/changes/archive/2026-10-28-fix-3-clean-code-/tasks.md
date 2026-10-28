# Tasks

## 1. Correção do achado [3]

- [x] 1.1 Aplicar correção nos arquivos: src/PlexAutoplay.Web/Endpoints/MarkerEndpoints.cs e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

Falso positivo confirmado: os `new` são DTOs anônimos de resposta (`Results.Ok(new { ... })`) e `new StreamReader(Request.Body)` (BCL, código de bindings HTTP). Nada de dependências injetáveis; documentado e arquivado sem alteração de código.

## Workflow follow-up

- Validar com `openspec validate --change fix-3-clean-code-/-solid` antes de seguir para archive/apply.
