# code-review Specification

## Purpose

Define o comportamento do comando `/code-review`: varredura de código .NET/C# e JS/Vue, lint por stack, referência a OWASP Top 10 e NIST SSDF, avaliação de Clean Code e SOLID, seleção interativa de itens para correção e geração de specs de correção via OpenSpec.

## Requirements

### Requirement: Varrer código alterado no repositório
O comando `/code-review` SHALL varrer o working tree e o diff do repositório para identificar arquivos alterados nas stacks suportadas (.NET/C# e JS/Vue).

#### Scenario: Varredura de arquivos alterados
- **WHEN** o usuário executa `/code-review` em um repositório com alterações em `src/**/*.cs` e `vue/**/*.vue`
- **THEN** o sistema lista apenas os arquivos alterados pertencentes às stacks suportadas

### Requirement: Executar lint por stack
O sistema SHALL executar lint padronizado de mercado para cada stack: .NET/C# com dotnet-format e StyleCop.Analyzers; JS/Vue com ESLint e Prettier.

#### Scenario: Lint .NET/C# com StyleCop
- **WHEN** há arquivos `.cs` alterados no working tree
- **THEN** o sistema executa StyleCop.Analyzers e reporta violações de estilo e clean code de C#

#### Scenario: Lint JS/Vue com ESLint e Prettier
- **WHEN** há arquivos `.js` ou `.vue` alterados no working tree
- **THEN** o sistema executa ESLint e Prettier e reporta violações de formatação e estilo

### Requirement: Referenciar OWASP Top 10 (consolidado)
O sistema SHALL cruzar os achados de lint e revisão com o OWASP Top 10 (versão consolidada, 2021), mapeando cada risco aplicável.

#### Scenario: Mapeamento de injeção
- **WHEN** um endpoint .NET aceita entrada do usuário sem validação
- **THEN** o sistema sinaliza o achado referenciando OWASP A01:2021 (Injection)

#### Scenario: Mapeamento de falha de autenticação
- **WHEN** um token JWT é emitido sem expiração (`exp`)
- **THEN** o sistema sinaliza o achado referenciando OWASP A02:2021 (Broken Authentication)

#### Scenario: Mapeamento de exposição de dados sensíveis
- **WHEN** um componente Vue exibe dados sensíveis sem máscara
- **THEN** o sistema sinaliza o achado referenciando OWASP A02:2021 ou A04:2021 conforme o caso

### Requirement: Referenciar NIST SSDF
O sistema SHALL mapear práticas de desenvolvimento seguro encontradas (ou ausentes) para controles do NIST SSDF.

#### Scenario: Mapeamento de práticas seguras
- **WHEN** o repositório não possui análise estática de segurança configurada
- **THEN** o sistema referencia NIST SSDF (ex: PW.3 / PS.3) como prática ausente

### Requirement: Avaliar Clean Code e SOLID
O sistema SHALL avaliar os princípios de Clean Code e SOLID (Responsabilidade Única, Aberto/Fechado, Substituição de Liskov, Segregação de Interface, Inversão de Dependência) nos arquivos varridos.

#### Scenario: Violação de Responsabilidade Única (S)
- **WHEN** uma classe C# acumula responsabilidades de acesso a dados, regras de negócio e apresentação
- **THEN** o sistema sinaliza violação do princípio SRP e sugere separação

#### Scenario: Violação de Inversão de Dependência (D)
- **WHEN** uma classe C# instancia diretamente dependências concretas em vez de usar interfaces
- **THEN** o sistema sinaliza violação de DIP e sugere injeção de dependência

### Requirement: Perguntar interativamente o que será corrigido
O sistema SHALL exibir ao usuário a lista numerada de achados e perguntar quais itens deseja corrigir.

#### Scenario: Seleção de itens para correção
- **WHEN** o sistema exibe `[1] JWT sem expiração`, `[2] Classe com SRP violado`, `[3] Componente Vue sem validação`
- **THEN** o usuário responde `1, 3` e o sistema considera apenas os itens 1 e 3 para correção

### Requirement: Gerar specs de correção para itens selecionados
Para cada item selecionado pelo usuário, o sistema SHALL criar uma change OpenSpec completa com `proposal.md`, `design.md`, `specs/<capability>/spec.md` e `tasks.md`.

#### Scenario: Criação de change de correção
- **WHEN** o usuário seleciona o item `1` (JWT sem expiração)
- **THEN** o sistema cria `openspec/changes/<change-nome>/proposal.md`, `design.md`, `specs/<capability>/spec.md` e `tasks.md` para a correção daquele item
