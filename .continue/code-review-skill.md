# Slash command `/code-review` — Continue

Registre este comando no Continue para disparar a skill de revisão de
código no working tree ativo.

## Como instalar

Copie o conteúdo para o arquivo de comandos do Continue:

```markdown
---
description: "Executa a skill de revisão de código (lint, OWASP Top 10, NIST SSDF, Clean Code, SOLID)"
---

Executa `python scripts/code-review-skill.py` no repositório atual.

Uso: `/code-review [--fix 1,2,3] [--dry-run]`
```

## Comportamento

Ponto único de implementação: `scripts/code-review-skill.py`.

A skill varre o working tree, executa lint por stack (.NET/C# com
dotnet-format + StyleCop.Analyzers; JS/Vue com ESLint + Prettier), cruza
com OWASP Top 10 (2021) e NIST SSDF, avalia Clean Code e SOLID, pergunta
interativamente o que corrigir e gera specs de correção OpenSpec.
