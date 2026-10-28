# Tasks

## 1. Correção do achado [2]

- [x] 1.1 Aplicar correção nos arquivos: src/PlexAutoplay.Infrastructure/Persistence/Repositories/SqliteMarkerRepository.cs e verificar que o achado desaparece na próxima execução de /code-review

## Resolução

Falso positivo confirmado: o único `new` da classe é `new SqliteConnection(_connectionString)` (factory de conexão sobre string de configuração) e DTOs anônimos do Dapper. Nenhuma dependência injetável é instanciada; refatorar adicionaria abstração sem benefício. Documentado e arquivado sem alteração de código.

## Workflow follow-up

- Validar com `openspec validate --change fix-2-clean-code-/-solid` antes de seguir para archive/apply.
