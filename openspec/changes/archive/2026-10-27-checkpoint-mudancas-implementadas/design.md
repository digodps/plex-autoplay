# Design

## Context

Esta change é um checkpoint de limpeza do projeto plex-autoplay, sobre a stack .NET + Vue já implementada. A remoção dos placeholders `Class1.cs` em `PlexAutoplay.Application` e `PlexAutoplay.Domain` não afeta nenhum comportamento de runtime: os arquivos eram vazios, gerados pelo template `dotnet new classlib`, e não tinham referências em `src/`. Veja a motivação em `proposal.md` — Why.

## Goals / Non-Goals

**Goals:**
- Remover os dois placeholders `Class1.cs` (Application e Domain) do repositório
- Registrar a tarefa concluída em `tasks.md` (task 7) da change `checkpoint-mudancas-implementadas`
- Manter o restante da base de código intocada

**Non-Goals:**
- Alterar qualquer endpoint, contrato de API, schema de banco ou comportamento de runtime
- Criar novos specs ou modificar requirements de specs existentes
- Refatorar código além da remoção dos dois arquivos

## Decisions

| Decisão | Alternativa considerada | Rationale |
|---------|------------------------|-----------|
| Remover ambos os `Class1.cs` | Remover apenas um | Ambos são placeholders idênticos de template, sem uso; remover os dois deixa o projeto limpo de uma só vez |
| Não criar novo spec para a remoção | Criar spec com requisito de remoção | A remoção não altera comportamento externo; é abaixo do nível de especificação |
| Manter `skip_specs: true` no `.openspec.yaml` | Listar specs modificados | Nenhum requirement de spec muda com esta change |

## Risks / Trade-offs

- [Risco] Nenhum arquivo inesperado referencia `Class1.cs` → **Mitigação**: verificação de busca por `Class1` em `src/` retornou zero matches antes e após a remoção
- [Trade-off] A change fica com `isPlanningComplete: false` enquanto `design` não é marcado como concluído → o checkpoint registra o trabalho feito, mas o planning permanece formalmente aberto

## Migration Plan

Não há migração necessária: a remoção dos dois arquivos é destrutiva sem rollback de comportamento, pois nada depende deles. Se algum código futuro depender de `Class1`, a compilação falhará e o problema será visível imediatamente.

## Open Questions

Nenhuma. A remoção é direta e sem ambiguidade técnica.
