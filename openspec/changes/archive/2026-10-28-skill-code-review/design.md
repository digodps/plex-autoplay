# Design

## Context

O repositório `plex-autoplay` já possui uma skill de commit (`scripts/commit-skill.py`) com ponto único de implementação e registro espelhado nos clientes de agente (`.claude/`, `.opencode/`, `.continue/`, `.dsh/`, `.agents/`, `.github/`). Não há equivalente para revisão de código. A stack é .NET/C# (backend), JS/Vue (frontend) e Python (testes/e2e), com lint já parcial via `.pre-commit-config.yaml` (dotnet-format, ESLint, Prettier) e CI com ruff/mypy para Python. A proposta deste change escopoa a skill a .NET/C# e JS/Vue, com OWASP Top 10 (2021) e NIST SSDF como referências de segurança.

## Goals / Non-Goals

**Goals:**
- Fornecer comando `/code-review` que varra o working tree, rode lint por stack, cruze com OWASP Top 10 e NIST SSDF, avalie Clean Code/SOLID, pergunte interativamente o que corrigir e gere specs de correção OpenSpec para itens selecionados.
- Ponto único de implementação (`scripts/code-review-skill.py`) com registro espelhado igual ao `/commit`.

**Non-Goals:**
- Modificar código-fonte do produto (`.NET`, `vue/`, `tests/`).
- Alterar `.pre-commit-config.yaml` ou CI nesta change.
- Suportar Python na primeira versão da skill.
- Executar correções automaticamente (a skill apenas gera specs; a implementação fica para o usuário / `/opsx-apply`).

## Decisions

### Ponto único de implementação em Python
**Decisão:** `scripts/code-review-skill.py` concentra toda a lógica (varredura, lint, referências, pergunta interativa, geração de specs).
**Alternativa:** Espalhar lógica em scripts por cliente de agente.
**Razão:** Igual ao padrão do `/commit`; evita divergência de comportamento entre clientes.

### Registro espelhado em 6 locais de agente
**Decisão:** Documentação e comando `/code-review` replicados em `.claude/`, `.opencode/`, `.continue/`, `.dsh/`, `.agents/` e `.github/`.
**Razão:** O projeto já espelha skills dessa forma; quem usa qualquer cliente enxerga o comando.

### Referências de segurança fixas (OWASP Top 10 2021 + NIST SSDF)
**Decisão:** Usar OWASP Top 10 (versão consolidada 2021) e NIST SSDF como referências normativas, embutidas como conhecimento da skill.
**Alternativa:** Buscar referências online em tempo de execução.
**Razão:** Determinismo offline e velocidade; OWASP 2021 está consolidado.

### Stacks alvo: .NET/C# e JS/Vue
**Decisão:** Primeira versão cobre apenas C# e Vue/JS.
**Alternativa:** Incluir Python desde o início.
**Razão:** Pedido do usuário; Python já tem ruff/mypy no CI e pode ser adicionado depois.

## Risks / Trade-offs

[Lint não configurado localmente] → O StyleCop.Analyzers não está instalado no projeto; a skill deve rodar com fallbacks gracefully (avisar que não está instalado em vez de falhar).

[Pergunta interativa depende do chat do agente] → Se o agente não suportar input interativo, a skill degrada para listar achados e esperar o usuário informar os itens por comando (`/code-review --fix 1,3`).

[Gerar specs sem validar] → As changes geradas para correção precisam ser validadas com `openspec validate` antes de seguir para `/opsx-apply`; a skill deve orientar isso na saída.
