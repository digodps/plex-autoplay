---
description: "Executa a skill de setup isolado para mudanças OpenSpec (branch remoto + clone em pasta irmã)"
---

Executa a skill de setup (`scripts/setup-spec.py`) no repositório atual,
criando um branch remoto dedicado para uma mudança OpenSpec e clonando-o em uma
pasta irmã fora da solução atual.

Padrão de branch: `feature/<mudanca>-<span>` onde `<span>` segue `dd-mm-yyyy HH:mm:ss:mmm`
(sanitizado para nomes de branch e pasta). Cada mudança gera seu próprio branch e clone isolados.

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
5. Cria pasta irmã `plex-setup-<mudanca>-<span-sanitizado>/` no mesmo pai da solução
6. Executa `git clone -b <branch> <origin> <pasta-irma>`
7. Copia os artefatos da mudança (proposal, design, tasks, specs) para o clone
8. Commita e pusha os artefatos no branch remoto, deixando-o pronto para trabalho

O PR é criado manualmente pelo usuário (este comando não automatiza).
Não usa `Co-authored-by`.
