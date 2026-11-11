---
description: "Padroniza commits em blocos lógicos (Conventional Commits + emoji + corpo descritivo). Valida OpenSpec ativo; sem OpenSpec, infere contexto dos arquivos."
---

Executa a skill `commit` para agrupar arquivos modificados e gerar commits padronizados.

Uso:
```
/commit [--change <nome>] [--dry-run]
```

Exemplos:
```
/commit                    # usa mudança OpenSpec em andamento (ou infere contexto)
/commit --change minha-opc # força uso de mudança específica
/commit --dry-run          # mostra o que faria sem commitar
```

Comportamento:
1. **Com OpenSpec ativo** (`--change` ou mudança `in-progress`): agrupa arquivos por task (lê `tasks.md`)
2. **Sem OpenSpec ativo**: analisa arquivos modificados (caminhos, extensões) e infere:
   - **tipo** (feat/fix/chore/docs/refactor/test/perf/ci/build/style/revert)
   - **escopo** (diretório raiz: src/, vue/, scripts/, etc.)
   - **título + corpo** baseado no contexto dos arquivos

Padrão obrigatório:
```
tipo(🎯escopo): título

corpo descritivo livre: por que + o que foi feito
```

Regras:
- Corpo descritivo **obrigatório** (não aceita commit sem corpo)
- Escopo opcional (ex: `feat(cache): adiciona Redis`)
- **NÃO** usar Co-authored-by
- Commit direto por bloco, sem confirmação intermediária