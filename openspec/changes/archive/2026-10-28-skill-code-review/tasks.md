# Tasks

## 1. Esqueleto e dependências do script

- [x] 1.1 Criar `scripts/code-review-skill.py` com estrutura básica (argumentos CLI, leitura de working tree/diff) e verificar que o arquivo é executável
- [x] 1.2 Adicionar detecção de stacks suportadas (.cs e .vue/.js) e verificar que lista apenas arquivos alterados nessas stacks

## 2. Lint por stack

- [x] 2.1 Implementar execução de dotnet-format e StyleCop.Analyzers para arquivos .cs alterados e verificar que reporta violações (ou avisa quando não instalado)
- [x] 2.2 Implementar execução de ESLint e Prettier para arquivos .js/.vue alterados e verificar que reporta violações de formatação

## 3. Referências de segurança e qualidade

- [x] 3.1 Implementar mapeamento de achados para OWASP Top 10 (2021) e verificar que cada risco aplicável é referenciado
- [x] 3.2 Implementar mapeamento de práticas para NIST SSDF e verificar que práticas ausentes são sinalizadas
- [x] 3.3 Implementar avaliação de Clean Code e SOLID (SRP, OCP, LSP, ISP, DIP) e verificar que violações são sinalizadas com sugestão

## 4. Interação e geração de specs de correção

- [x] 4.1 Implementar exibição numerada de achados e pergunta interativa ao usuário sobre o que corrigir, verificando que a resposta é lida do chat
- [x] 4.2 Implementar geração de change OpenSpec completa (proposal.md, design.md, specs/<cap>/spec.md, tasks.md) para cada item selecionado, verificando que os arquivos são criados em `openspec/changes/`

## 5. Registro do comando (espelhado)

- [x] 5.1 Criar documentação e comando `/code-review` em `.claude/` (`.claude/code-review-skill.md` + `.claude/commands/code-review.md`) e verificar espelhamento igual ao `/commit`
- [x] 5.2 Criar documentação e comando `/code-review` em `.opencode/`, `.continue/`, `.dsh/`, `.agents/` e `.github/` e verificar que todos apontam para `scripts/code-review-skill.py`

## 6. Validação da mudança

- [x] 6.1 Rodar `openspec validate --change skill-code-review` e corrigir eventuais bloqueios antes de seguir para archive/apply

## Workflow follow-up

- Archive a change após os requisitos de revisão do projeto serem satisfeitos.
- Verifique o resultado do archive.
