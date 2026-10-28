---
description: "Executa a skill de setup isolado para mudanças OpenSpec (branch remoto dedicado)"
---

Executa a skill de setup (`scripts/setup-spec.py`) no repositório atual,
criando um branch remoto dedicado para uma mudança OpenSpec e deixando o
branch local pronto para uso (apontando para o remoto).

Padrão de branch: `feature/<mudanca>-<span>` onde `<span>` segue `dd-mm-yyyy HH:mm:ss:mmm`
(sanitizado para nomes de branch). Cada mudança gera seu próprio branch isolado.

Uso:
```
/setup-spec [--change <nome>] [--dry-run]
```

Exemplos:
```
/setup-spec                                       # usa a mudança ativa
/setup-spec --change implementar-cache-generico-plugavel  # usa a mudança informada
/setup-spec --dry-run                             # mostra o que seria feito, sem executar git
```

O comando executa:
1. Identifica a mudança OpenSpec ativa (via `openspec list --json` ou `--change`)
2. Gera um span no formato `dd-mm-yyyy HH:mm:ss:mmm`
3. Cria branch `feature/<mudanca>-<span-sanitizado>` a partir do `main` remoto limpo
4. Faz push do branch para origin
5. Deixa o branch local pronto para uso (apontando para o remoto)

O PR é criado manualmente pelo usuário (este comando não automatiza).
Não usa `Co-authored-by`.
