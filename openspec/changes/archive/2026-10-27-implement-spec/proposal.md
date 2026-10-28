# Proposal

## Why

O fluxo de implementação de mudanças OpenSpec no `plex-autoplay` não tem um mecanismo que isole o trabalho de cada spec em um branch próprio e em uma pasta de trabalho separada. Sem isso, múltiplos agentes ou desenvolvedores trabalhando em specs diferentes acabam compartilhando o mesmo working tree, gerando conflitos e dificultando a revisão por pull request. É necessário uma skill que, a partir de uma mudança já planejada pelo `/opsx-apply`, crie um branch remoto dedicado, clone-o em uma pasta irmã fora da solução atual, copie os artefatos da mudança para aquele clone e deixe o branch pronto para implementação — com commit e PR feitos posteriormente de forma manual.

## What Changes

- Criar a skill `implement-spec` que recebe o nome da mudança ativa do OpenSpec
- Criar um branch git chamado `feature/<nome-da-mudança>-<span>` onde `<span>` segue o formato `dd-mm-yyyy HH:mm:ss:mmm`
- Fazer push do branch para o remote (origin)
- Criar uma pasta irmã no mesmo pai da solução (`plex-implement-<nome-da-mudança>-<span>/`)
- Fazer `git clone` desse branch naquela pasta irmã
- Copiar os artefatos da mudança (proposal, design, tasks e specs) para o clone
- Commitar e pushar os artefatos no branch remoto, deixando-o pronto para trabalho
- O agente implementa todas as tasks do branch; o PR é criado manualmente pelo usuário

## Capabilities

### New Capabilities

Nenhum. Esta mudança é de ferramenta e processo (skill de implementação isolada); não introduz comportamento de sistema durável que deva ser especificado.

### Modified Capabilities

Nenhum. Os specs existentes continuam inalterados em nível de requisito.

## Impact

- `scripts/`: nova skill de implementação (ex: `scripts/implement-spec.py` ou similar) invocada após o `/opsx-apply`
- `openspec/changes/implement-spec/`: artefatos de planejamento desta própria mudança
- Workflow de implementação: branch remoto dedicado por mudança, clone em pasta irmã, artefatos copiados e commitados no branch
- Possível criação de PR manual no GitHub após a implementação das tasks
