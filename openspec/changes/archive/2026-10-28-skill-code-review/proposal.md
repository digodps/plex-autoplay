# Proposal

## Why

O repositório não possui uma skill de revisão de código padronizada. Pull requests e commits chegam sem revisão sistemática de lint, segurança (OWASP Top 10 / NIST SSDF) e qualidade (Clean Code / SOLID), o que permite que padrões ruins e vulneráveis entrem na base. Uma skill `/code-review` espelhada nos clientes de agente preenche essa gap.

## What Changes

- Criar `scripts/code-review-skill.py` como ponto único de implementação (igual ao `scripts/commit-skill.py`)
- Criar documentação e registro do comando `/code-review` espelhado em `.claude/`, `.opencode/`, `.continue/`, `.dsh/`, `.agents/` e `.github/`
- A skill varre o working tree, executa lint por stack (.NET/C# com dotnet-format + StyleCop.Analyzers; JS/Vue com ESLint + Prettier), cruza com OWASP Top 10 (2021) e NIST SSDF, e avalia Clean Code / SOLID
- A skill pergunta interativamente ao usuário quais itens deseja corrigir
- Para cada item selecionado, a skill cria uma change OpenSpec completa (`proposal.md`, `design.md`, `specs/<capability>/spec.md`, `tasks.md`)

## Capabilities

### New Capabilities

- `code-review`: Define o comportamento do comando `/code-review` — varredura de código, lint por stack, referência a OWASP Top 10 e NIST SSDF, avaliação de Clean Code e SOLID, seleção interativa de itens para correção e geração de specs de correção. Cada item selecionado gera uma nova change OpenSpec própria.

### Modified Capabilities

- (nenhum — não altera requisitos de specs existentes)

## Impact

- Novos arquivos: `scripts/code-review-skill.py` + arquivos de comando/skill espelhados nos diretórios de agente
- Nenhum código-fonte do produto (`.NET`, `vue/`, `tests/`) é modificado por esta change
- Nenhuma dependência nova é adicionada ao `pre-commit-config.yaml` ou CI nesta change
