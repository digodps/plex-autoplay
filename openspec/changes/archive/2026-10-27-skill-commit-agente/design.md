# Design

## Context

O repositório `plex-autoplay` já utiliza OpenSpec para planejar mudanças (`openspec/changes/` com `proposal.md`, `specs/`, `design.md`, `tasks.md`), mas não há qualquer padrão formal de mensagens de commit nem mecanismo que agrupe alterações em blocos lógicos. O histórico atual mistura conventional commits com e sem escopo, com e sem emoji, e sem corpo descritivo consistente. Não existem Git hooks instalados (apenas `.sample` padrão). O diretório `scripts/` contém scripts shell (`logs.sh`, `run-e2e-tests.sh`, `start.sh`, `stop.sh`) — local natural para abrigar a skill de commit. Esta mudança não altera comportamento de sistema, apenas o processo de commit. Veja a motivação em `proposal.md - Why`.

## Goals / Non-Goals

**Goals:**
- Todo commit segue o padrão `tipo(🎯escopo): título` (conventional commit com emoji)
- Todo commit possui corpo descritivo livre (por que + o que foi feito)
- O agente commita por blocos lógicos: agrupados por task do OpenSpec e tipo de mudança, nunca um único "commitão"
- O padrão vale para todos os commits: do agente e de humanos
- Execução direta: o agente commita cada bloco sem confirmação intermediária
- Sem `Co-authored-by` fixo nos commits
- Invocação via comando `/commit` direto no terminal (OpenCode), como slash command no Claude e como command no GitHub Copilot

**Non-Goals:**
- Validar o conteúdo semântico do corpo descritivo (apenas presença)
- Impor escopo obrigatório (opcional, entre parênteses, como nos exemplos existentes do repositório)
- Alterar specs de comportamento do sistema (esta mudança é de ferramenta/processo)
- Substituir o pre-commit existente (ruff, actionlint) — eles continuam independentes

## Decisions

**1. Local da skill: `scripts/commit-skill.py`**
- Rationale: `scripts/` já existe e abriga scripts de automação do repositório; um script Python é fácil de invocar pelo agente e de manter
- Alternativa considerada: hook Git (`commit-msg` ou `prepare-commit-msg`). Rejeitado para o agente, pois exige instalação por desenvolvedor e não se distribui facilmente com o repositório; pode ser adicionado depois para commits humanos

**2. Agrupamento em blocos lógicos: mesma task do OpenSpec + mesmo tipo de mudança**
- Rationale: cada task no `tasks.md` de uma mudança OpenSpec representa um bloco de trabalho coeso; agrupar por task + tipo (ex: `feat(cache)`, `fix(cache)`, `docs(cache)`) produz commits pequenos e rastreáveis
- Alternativa considerada: agrupar apenas por diretório. Rejeitado: não reflete intenção de trabalho; uma mesma task pode tocar diretórios diferentes

**3. Cabeçalho: `tipo(🎯escopo): título`, corpo livre**
- Rationale: segue conventional commit já usado no repositório, adiciona emoji para leitura rápida no `git log`, e corpo livre evita cerimônia excessiva
- Alternativa considerada: corpo com campos fixos (`Por que:` / `O que:`). Rejeitado: mais rígido que o necessário; o corpo livre atende o requisito de descrever por que e o que

**4. Commit direto (sem confirmação por bloco)**
- Rationale: o agente tem contexto completo das alterações; confirmar cada bloco adicionaria interação desnecessária
- Alternativa considerada: apresentar cada mensagem para aprovação antes de commitar. Rejeitado: impede fluxo autônomo do agente

**5. `skip_specs: true` no `.openspec.yaml`**
- Rationale: mudança de ferramenta/processo sem alteração de comportamento de sistema; nenhum spec existente ou novo precisa ser criado ou modificado

**6. Invocação via `/commit` em OpenCode, Claude e GitHub Copilot**
- Rationale: o usuário quer chamar a skill diretamente em cada ambiente com o mesmo comando `/commit`, sem precisar lembrar sintaxe diferente por ferramenta
- OpenCode: `/commit` como comando direto no terminal, que executa `scripts/commit-skill.py` no repositório atual
- Claude: slash command `/commit` registrado no Claude Code, que dispara a skill Python no working tree ativo
- GitHub Copilot: command `/commit` no Copilot Chat / editor, que chama a skill no contexto do repositório aberto
- Alternativa considerada: fazer o usuário digitar `python scripts/commit-skill.py` manualmente. Rejeitado: fricção desnecessária; o ponto é um comando único e curto em todos os ambientes
- Como cada ambiente expõe o comando é detalhe de integração com a ferramenta; o script Python (`scripts/commit-skill.py`) permanece o único ponto de implementação da lógica de commit

## Risks / Trade-offs

[O agente pode agrupar arquivos de tasks diferentes no mesmo commit] → Mitigar: a skill lê `tasks.md` da mudança em andamento e só agrupa arquivos da mesma task

[Corpo descritivo vago ou genérico] → Mitigar: revisão humana no `git log`; o padrão exige presença do corpo, não sua qualidade

[Commits humanos escapam do padrão] → Mitigar: adicionar hook Git (`commit-msg`) para validação automática como passo futuro, fora do escopo desta mudança

[A skill em Python exige runtime Python no ambiente do agente] → Mitigar: o repositório já é .NET/Python híbrido (tests, Dockerfile com Python para certificado); Python está disponível

## Migration Plan

1. Criar `scripts/commit-skill.py` implementando: leitura de `git status --porcelain`, mapeamento de arquivos para tasks do OpenSpec, agrupamento por task + tipo, montagem de mensagem (`tipo(🎯escopo): título` + corpo), `git add` + `git commit` por bloco
2. O agente passa a usar a skill para todos os seus commits
3. Expor a skill como comando `/commit` em: OpenCode (terminal), Claude (slash command) e GitHub Copilot (command no Copilot Chat). Cada ambiente aponta para o mesmo `scripts/commit-skill.py`
4. Commits humanos continuam livres; adoção voluntária do padrão (hook opcional depois)
5. Rollback: remover `scripts/commit-skill.py` e os registros de `/commit` em cada ambiente; os commits já feitos seguem o padrão naturalmente, sem dependência do script

## Open Questions

Nenhuma. Todas as decisões foram resolvidas com base na conversa de exploração e nos exemplos existentes do repositório.
