# Tasks

## 1. Correção do achado [5]

- [x] 1.1 Aplicar correção nos arquivos: src/PlexAutoplay.Web/Program.cs e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

Falso positivo confirmado: `new` apenas no bootstrap de aplicação (Serilog, builder, app de configuração em Program.cs) — código de composição raiz, onde a DI ocorre justamente. Documentado e arquivado sem alteração de código.

## Workflow follow-up

- Validar com `openspec validate --change fix-5-clean-code-/-solid` antes de seguir para archive/apply.
