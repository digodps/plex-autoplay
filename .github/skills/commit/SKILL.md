---
name: commit
description: Padroniza commits em blocos lógicos seguindo Conventional Commits com emoji. Valida OpenSpec ativo; sem OpenSpec, infere contexto dos arquivos alterados.
allowed-tools: Bash(*)
license: MIT
---

# Skill `commit` (`/commit`)

Padroniza commits em blocos lógicos (tipo + escopo + título + corpo descritivo obrigatório).

## Padrão de Commit

```
tipo(🎯escopo): título

corpo descritivo livre: por que + o que foi feito
```

**Tipos válidos:** feat 🚀 | fix 🐛 | docs 📖 | chore 🔧 | style 🎨 | refactor ♻️ | test ✅ | perf ⚡ | ci 👷 | build 📦 | revert ⏪

**Regras:**
- Corpo descritivo **obrigatório** (não aceita commit sem corpo)
- Escopo opcional (ex: `feat(cache): adiciona Redis`)
- **NÃO** usar Co-authored-by
- Commit direto por bloco, sem confirmação intermediária

## Comportamento

1. **Com OpenSpec ativo**: Agrupa arquivos por task + tipo (lê `tasks.md` da mudança)
2. **Sem OpenSpec ativo**: Infere contexto dos arquivos alterados (caminhos, extensões, padrões) e gera blocos coerentes

## Uso

```
/commit [--change <nome>] [--dry-run]
```

## Fonte

- Script: `scripts/commit-skill.py`
- Após edição, execute `python3 scripts/sync-agent-configs.py`