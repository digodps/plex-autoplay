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

<<<<<<< HEAD
## Segurança de credenciais

Nunca exponha credenciais no chat, em logs ou em mensagens de commit:

- **Não execute** `env`, `printenv`, `set`, `export -p`, `git config --list`, `git remote -v` ou qualquer comando que imprima valores de variáveis, tokens, senhas ou URLs com credenciais.
- **Não imprima** conteúdo de `.env`, `.git-credentials`, `~/.netrc`, chaves SSH, tokens (`ghp_`, `gho_`, `github_pat_`, `AKIA...`) ou URLs no formato `https://usuario:senha@host`.
- **Sanitize** qualquer saída de erro de `git`: mostre apenas a mensagem, nunca o ambiente ou a configuração completa.
- **Antes de commitar**, inspecione o diff em busca de segredos. Se encontrar token, senha, chave privada ou credencial, aborte o commit e reporte o arquivo/linha sem revelar o valor.
- **Use** apenas `git config user.name` e `git config user.email` quando necessário; nunca `--list` sem filtro.

=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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