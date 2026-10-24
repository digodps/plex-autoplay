# Contribuindo

Contribuições são bem-vindas. Para manter o histórico legível, faça commits
pequenos, cada um cobrindo um bloco lógico e independente de alterações.

## Convenção de commits

Use este formato no título:

```text
<tipo>(<ícone>): <resumo curto>
```

Na descrição do commit, explique o que foi feito e, quando útil, o motivo:

```text
Descrição: <resumo das alterações deste bloco lógico>
```

Tipos aceitos:

| Tipo | Quando usar |
| --- | --- |
| `feature` | Adiciona uma funcionalidade |
| `fix` | Corrige um defeito |
| `doc` | Altera documentação |
| `refactor` | Reorganiza código sem mudar o comportamento |
| `test` | Adiciona ou altera testes |
| `chore` | Faz manutenção, configuração ou tarefas auxiliares |

Escolha um emoji relacionado ao conteúdo do commit. Exemplos:

```text
feature(🎬): adiciona avanço automático de episódios

Descrição: Detecta o fim do episódio e inicia o próximo pela API do Plex.
```

```text
fix(🐛): corrige a leitura de marcadores personalizados

Descrição: Trata arquivos sem dados de série sem interromper o monitoramento.
```

```text
doc(📚): documenta a instalação no ZimaOS

Descrição: Adiciona instruções para importar o Compose pela interface do NAS.
```

Quando um conjunto de alterações incluir mais de um assunto independente,
separe-o em commits distintos. Não inclua tokens, senhas ou outros segredos
nos commits.
