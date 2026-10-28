# Command `/commit` — GitHub Copilot

Registre este command no GitHub Copilot (Copilot Chat / editor) para
chamar a skill de commit no contexto do repositório aberto.

## Como instalar

Adicione a seguinte configuração ao seu arquivo de commands do Copilot
(conforme documentação do seu cliente GitHub Copilot):

```json
{
  "name": "commit",
  "description": "Executa a skill de commit por blocos lógicos",
  "command": "python scripts/commit-skill.py",
  "workingDirectory": "${workspaceFolder}",
  "arguments": ["--change", "${input:changeName}", "--dry-run"]
}
```

Ou, se o seu cliente Copilot suportar comandos via markdown:

```markdown
# /commit

Executa `python scripts/commit-skill.py` no repositório aberto,
agrupando alterações em blocos lógicos por task do OpenSpec + tipo
de mudança.

Padrão: `tipo(🎯escopo): título` + corpo descritivo livre.
Cada bloco vira um `git commit` separado.

Uso: `/commit [--change <nome>] [--dry-run]`
```

## Comportamento

O command aponta para o mesmo ponto único de implementação:
`scripts/commit-skill.py` — usado também pelo OpenCode (`/commit`)
e pelo Claude Code (slash command `/commit`).

Não usa `Co-authored-by`. Commit direto por bloco, sem confirmação
intermediária.
