# code-review skill

Ponto único de implementação: `scripts/code-review-skill.py`.

A skill varre o working tree, executa lint por stack (.NET/C# com
dotnet-format + StyleCop.Analyzers; JS/Vue com ESLint + Prettier), cruza
com OWASP Top 10 (2021) e NIST SSDF, avalia Clean Code e SOLID, pergunta
interativamente o que corrigir e gera specs de correção OpenSpec.

Registrado como comando `/code-review` em `.github/commands/code-review.md`.
