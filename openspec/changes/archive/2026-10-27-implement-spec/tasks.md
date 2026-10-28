# Tasks

## 1. Skill de implementação (implementação principal)

- [x] 1.1 Criar `scripts/implement-spec.py` que: identifica a mudança ativa do OpenSpec, gera span no formato `dd-mm-yyyy HH:mm:ss:mmm`, cria branch `feature/<mudança>-<span>`, faz push para origin, cria pasta irmã `plex-implement-<mudança>-<span>/`, executa `git clone -b <branch> <origin> <pasta-irmã>`, copia artefatos da mudança (proposal, design, tasks, specs) para o clone, commit e push dos artefatos. Verificar que o arquivo é criado em `scripts/` e é executável.
- [x] 1.2 Implementar geração de span no formato `dd-mm-yyyy HH:mm:ss:mmm`. Verificar que o formato está correto e único por execução.
- [x] 1.3 Implementar criação do branch local `feature/<mudança>-<span>` a partir do `main` remoto limpo e push para origin. Verificar que o branch existe no remote.
- [x] 1.4 Implementar criação da pasta irmã no mesmo pai da solução e `git clone -b <branch>` nela. Verificar que o clone aponta para o branch correto.
- [x] 1.5 Implementar cópia dos artefatos da mudança (proposal.md, design.md, tasks.md, specs/) para o clone. Verificar que os arquivos estão presentes no clone.
- [x] 1.6 Implementar commit e push dos artefatos no branch remoto, deixando-o pronto para trabalho. Verificar que o branch remoto contém todos os artefatos.

## 2. Documentação e testes da skill

- [x] 2.1 Documentar no próprio `scripts/implement-spec.py` (ou README adjacente) o padrão de branch `feature/<mudança>-<span>`, o formato do span `dd-mm-yyyy HH:mm:ss:mmm`, o fluxo de clone em pasta irmã e o fato de que o PR é criado manualmente. Verificar que a documentação está presente e legível.
- [x] 2.2 Testar a skill com uma mudança simulada. Verificar que o branch remoto é criado, o clone é feito na pasta irmã, os artefatos são copiados e commitados no branch remoto.

## 3. Validação da mudança OpenSpec

- [x] 3.1 Rodar `openspec validate` para confirmar que a mudança está coerente (proposal, design, tasks consistentes, `skip_specs: true` aplicado corretamente). Verificar saída válida sem erros.

## Workflow follow-up

- Revisar a mudança com o time antes de aplicar.
- Após aprovação, executar `/opsx-apply` para implementar as tarefas acima.
- Archivar a mudança após revisão e aplicação.
