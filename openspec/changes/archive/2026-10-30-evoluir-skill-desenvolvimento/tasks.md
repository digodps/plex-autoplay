# Tasks

## 1. Documentação e Guia de Desenvolvimento

- [x] 1.1 Criar `DEVELOPMENT_GUIDE.md` baseado nas regras do `code-review-skill.py` e verificar que o arquivo está na raiz do repositório
- [x] 1.2 Atualizar referências ao `DEVELOPMENT_GUIDE.md` nos arquivos de skill e comando `.opencode` e `.agents` e verificar que todas apontam corretamente

## 2. Especificações e Planejamento da Change

- [x] 2.1 Criar change `evoluir-skill-desenvolvimento` via `openspec new change` e verificar que os arquivos proposal.md, design.md e `.openspec.yaml` foram gerados
- [x] 2.2 Autor `proposal.md` especificando os 5 agentes, novos papéis (Reviewer, Repairer) e guia de desenvolvimento
- [x] 2.3 Autor `design.md` detalhando a arquitetura e decisões técnicas (inclusão do Agente 4 e 5, testes manuais no browser na máquina do dev)
- [x] 2.4 Criar deltas de spec para `development-guide`, `repair-agent`, `multi-agent-dev` e `code-review` e verificar com `openspec validate --change evoluir-skill-desenvolvimento`

## 3. Validação Final do Planejamento

- [x] 3.1 Executar `openspec validate --change evoluir-skill-desenvolvimento` e garantir que o planejamento está 100% válido e sem erros de especificação

## Workflow follow-up

- Executar `openspec archive --change evoluir-skill-desenvolvimento` após a aprovação da proposal e design.
- Sincronizar as specs delta para o diretório principal de specs se necessário.
