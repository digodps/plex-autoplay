---
description: "Executa a skill de commit por blocos lógicos (tipo(🎯escopo): título + corpo descritivo)"
---

Executa a skill de commit (`scripts/commit-skill.py`) no repositório atual,
agrupando alterações em blocos lógicos por task do OpenSpec + tipo de mudança.

Padrão de commit gerado: `tipo(🎯escopo): título` + corpo descritivo livre.
Cada bloco vira um `git commit` separado (nunca um commitão).

Uso:
```
/commit [--change <nome>] [--dry-run]
```

Exemplos:
```
/commit              # commita todos os blocos pendentes
commit --dry-run     # mostra o que seria commitado, sem criar commits
commit --change skill-commit-agente  # usa a mudança OpenSpec informada
```

O comando executa:
1. `git status --porcelain` para listar arquivos alterados
2. Mapeia arquivos para tasks do OpenSpec (via `tasks.md` da mudança em andamento)
3. Agrupa por mesma task + mesmo tipo de mudança
4. Monta mensagem `tipo(🎯escopo): título` + corpo descritivo
5. Executa `git add` + `git commit` por bloco (sem confirmação intermediária)

Não usa `Co-authored-by`. Não valida conteúdo semântico do corpo (apenas presença).
