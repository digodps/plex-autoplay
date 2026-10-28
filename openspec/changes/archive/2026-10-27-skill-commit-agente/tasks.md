# Tasks

## 1. Skill de commit (implementação principal)

- [x] 1.1 Criar `scripts/commit-skill.py` que lê `git status --porcelain`, mapeia arquivos para tasks do OpenSpec (via `tasks.md` da mudança em andamento), agrupa por mesma task + mesmo tipo de mudança, monta mensagem (`tipo(🎯escopo): título` + corpo livre descritivo) e executa `git add` + `git commit` por bloco. Verificar que o arquivo é criado em `scripts/` e é executável.
- [x] 1.2 Implementar formatação e validação do cabeçalho `tipo(🎯escopo): título` usando conventional commit com emoji. Verificar que tipos inválidos são rejeitados e que o emoji está presente no título.
- [x] 1.3 Implementar corpo descritivo livre (por que + o que foi feito) na mensagem de commit. Verificar que commits sem corpo não são aceitos.
- [x] 1.4 Implementar commit direto por bloco, sem confirmação intermediária por bloco. Verificar que cada bloco lógico resulta em um `git commit` separado, nunca um único commitão com tudo.

## 2. Documentação e testes da skill

- [x] 2.1 Documentar no próprio `scripts/commit-skill.py` (ou README adjacente) o padrão de commit: `tipo(🎯escopo): título`, corpo livre, sem `Co-authored-by`, agrupamento por task + tipo. Verificar que a documentação está presente e legível.
- [x] 2.2 Testar a skill com um conjunto simulado de arquivos modificados. Verificar que os commits são gerados por blocos lógicos (por task + tipo), não como um único commitão.

## 3. Validação da mudança OpenSpec

- [x] 3.1 Rodar `openspec validate` para confirmar que a mudança está coerente (proposal, design, tasks consistentes, `skip_specs: true` aplicado corretamente). Verificar saída válida sem erros.

## 4. Invocação via `/commit` em cada ambiente

- [x] 4.1 Registrar `/commit` como comando direto no terminal OpenCode, que executa `scripts/commit-skill.py` no repositório atual. Verificar que digitar `/commit` dispara a skill.
- [x] 4.2 Registrar `/commit` como slash command no Claude Code, que dispara `scripts/commit-skill.py` no working tree ativo. Verificar que o comando está disponível e executa a skill.
- [x] 4.3 Registrar `/commit` como command no GitHub Copilot (Copilot Chat / editor), que chama `scripts/commit-skill.py` no contexto do repositório aberto. Verificar que o command aparece e executa a skill.
- [x] 4.4 Verificar que todos os três ambientes usam o mesmo `scripts/commit-skill.py` como ponto único de implementação da lógica de commit. Verificar consistência de comportamento entre ambientes.

## Workflow follow-up

- Revisar a mudança com o time antes de aplicar.
- Após aprovação, executar `/opsx-apply` para implementar as tarefas acima.
- Archivar a mudança após revisão e aplicação.
