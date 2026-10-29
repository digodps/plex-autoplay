# Tasks

## 1. Definição da Skill e Workflow de Agentes

- [x] 1.1 Criar `.agents/skills/dev-spec/SKILL.md` definindo o protocolo multi-agente (Orquestrador Backend, Subagente Frontend, Subagente Tester, portões de code review e execução no Distrobox) e verificar que o arquivo existe com cabeçalho de metadados válido
- [x] 1.2 Criar `.agents/workflows/dev-spec.md` e `.agents/commands/dev-spec.md` com a documentação de uso do slash command `/dev-spec` e verificar que ambos os arquivos existem
- [x] 1.3 Documentar o contrato de divisão de diretórios (`src/**` para Backend e `vue/**` para Frontend) no SKILL.md e verificar que está alinhado com o design

## 2. Script e Automação de Suporte

- [x] 2.1 Criar `scripts/dev-spec.py` para orquestrar e validar os pré-requisitos do ambiente (.env, distrobox, gh CLI e status OpenSpec) e verificar que `python scripts/dev-spec.py --help` executa sem erros
- [x] 2.2 Adicionar rotina de geração de commits via `scripts/commit-skill.py` e abertura de Pull Request via `gh pr create` no script e verificar via flag `--dry-run`

## 3. Validação e Qualidade

- [x] 3.1 Executar validação de conformidade com `openspec validate multi-agent-dev-workflow` e verificar que todas as especificações e tarefas passam sem erros
- [x] 3.2 Executar simulação de dry-run do fluxo completo e verificar que o isolamento de branch e montagem dos comandos estão corretos

## Workflow follow-up

- Arquivar a mudança após implementação e validação via `/opsx-archive`.
- Sincronizar o delta spec `multi-agent-dev` para os specs duráveis do repositório.
