# Design

## Context

O `plex-autoplay` utiliza OpenSpec para planejar mudanças (`openspec/changes/` com `proposal.md`, `specs/`, `design.md`, `tasks.md`), mas não há qualquer mecanismo que isole a implementação de cada mudança em um branch dedicado e em uma pasta de trabalho separada. O working tree atual tem 74 arquivos modificados/deletados, não há Git hooks instalados, e o `gh` (GitHub CLI) não está disponível no ambiente. Não existe skill para criar um branch remoto, cloná-lo em pasta irmã e deixar os artefatos da mudança prontos para implementação. Veja a motivação em `proposal.md - Why`.

## Goals / Non-Goals

**Goals:**
- Criar a skill `implement-spec` que, a partir de uma mudança OpenSpec ativa, cria um branch remoto dedicado
- Nome do branch: `feature/<nome-da-mudança>-<span>` onde `<span>` segue `dd-mm-yyyy HH:mm:ss:mmm`
- Push do branch para o remote (origin)
- Criar pasta irmã no mesmo pai da solução (`plex-implement-<nome-da-mudança>-<span>/`)
- `git clone -b <branch>` naquela pasta irmã
- Copiar artefatos da mudança (proposal, design, tasks, specs) para o clone
- Commit e push dos artefatos no branch remoto, deixando-o pronto para trabalho
- O agente implementa todas as tasks do branch; o PR é criado manualmente pelo usuário

**Non-Goals:**
- Automatizar a criação de PR (feito manualmente pelo usuário)
- Substituir o pre-commit existente (ruff, actionlint)
- Alterar specs de comportamento do sistema (esta mudança é de ferramenta/processo)
- Resolver conflitos de merge ou fazer merge para main

## Decisions

**1. Isolamento via `git clone` (não `git worktree`)**
- Rationale: `git clone` cria um repositório completamente separado na pasta irmã, evitando que o estado sujo do working tree local (74 arquivos) vaze para o ambiente do agente
- Alternativa considerada: `git worktree`. Rejeitado: compartilha o mesmo `.git` e o estado sujo do diretório principal poderia afetar as worktrees

**2. Nome do branch: `feature/<nome-da-mudança>-<span>`**
- Rationale: o nome da mudança OpenSpec já é único e descritivo; o span (`dd-mm-yyyy HH:mm:ss:mmm`) evita colisão entre execuções paralelas da mesma mudança
- Alternativa considerada: usar o nome da spec. Rejeitado: o usuário pediu que o branch seja nomeado pela mudança selecionada, não pela spec

**3. Span: `dd-mm-yyyy HH:mm:ss:mmm` (sanitizado nos nomes de branch e pasta)**
- Rationale: formato legível e com milissegundos para exibição e logs; colisão improvável entre agentes paralelos. No nome do branch e da pasta, espaços e dois-pontos são substituídos por `-` (ex: `feature/implementar-cache-generico-plugavel-27-10-2026-14-30-00-123`), evitando problemas de aspas em comandos shell
- Alternativa considerada: timestamp Unix ou ID aleatório. Rejeitado: menos legível para humanos

**4. Sanitização do span em nomes de arquivo**
- Rationale: espaços e `:` são inválidos ou problemáticos em nomes de branch e pasta no shell. A skill sanitiza o span antes de usá-lo como nome de branch (`feature/<mudança>-<span-sanitizado>`) e nome de pasta (`plex-implement-<mudança>-<span-sanitizado>/`). O span original com espaços é mantido apenas em mensagens e logs
- Alternativa considerada: usar o span literalmente com aspas em todos os comandos. Rejeitado: mais frágil e propenso a erros de escaping

**5. Criação do branch: local → push → clone**
- Rationale: o branch precisa existir no remote para que o `git clone -b` funcione. A skill cria localmente, faz push, e depois clona aquele branch na pasta irmã
- Alternativa considerada: clonar primeiro e fazer switch depois. Rejeitado: o clone precisaria do branch já existente no remote

**6. Artefatos copiados para o clone: proposal, design, tasks, specs**
- Rationale: o agente precisa do contexto de planejamento para saber o que implementar
- Alternativa considerada: copiar apenas código-fonte já modificado. Rejeitado: o clone começa limpo a partir do branch; não há código-fonte da mudança ainda

**7. `skip_specs: true` no `.openspec.yaml`**
- Rationale: mudança de ferramenta/processo sem alteração de comportamento de sistema; nenhum spec existente ou novo precisa ser criado ou modificado

## Risks / Trade-offs

[Espaços e dois-pontos no span podem quebrar comandos shell sem aspas] → Mitigar: a skill deve envolver nomes de branch e pasta em aspas ao invocar `git`

[Working tree local tem 74 arquivos modificados/deletados] → Mitigar: criar o branch a partir do `main` remoto limpo (`git fetch origin main`), não a partir do `main` local sujo

[Múltiplas mudanças ativas (`https-somente-certificado`, `implementar-cache-generico-plugavel`)] → Mitigar: a skill deve selecionar a mudança ativa informada pelo usuário ou listar e pedir escolha

[`gh` não instalado] → Mitigar: o PR é criado manualmente pelo usuário; a skill apenas deixa o branch pronto para trabalho

[Branches remotos se acumulam se os PRs não forem mesclados] → Mitigar: manutenção periódica de branches obsoletos no GitHub

## Migration Plan

1. Criar a skill `implement-spec` (ex: `scripts/implement-spec.py`)
2. A skill lê a mudança ativa via `openspec list --json` / `openspec status --change <nome> --json`
3. Cria branch `feature/<mudança>-<span>`, push para origin
4. Cria pasta irmã `plex-implement-<mudança>-<span>/` e `git clone -b <branch>`
5. Copia artefatos da mudança para o clone, commit e push
6. O agente implementa as tasks no clone e faz commit local
7. O usuário cria o PR manualmente no GitHub
8. Rollback: deletar o branch remoto e a pasta irmã

## Open Questions

- Se houver múltiplas mudanças ativas, a skill deve listar e pedir escolha ou aceitar o nome como parâmetro?
- O span deve ser mantido com espaços e dois-pontos nos nomes de branch/pasta, ou sanitizado?
- A skill deve criar o branch a partir do `main` remoto limpo ou do `main` local (com os 74 arquivos sujos)?
