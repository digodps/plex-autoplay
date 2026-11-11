# Deficiências Resolvidas — fix-biblioteca-titulo

## Goldfish Review A (Arquitetura, Dependências, Integração)

| # | Achado | Classificação | Resolução |
|---|--------|---------------|-----------|
| A1 | Mudança apenas de textos e CSS, nenhuma alteração de arquitetura, dependências ou integração. | **Aceito** | Nenhuma ação necessária. Escopo mantido. |
| A2 | Breadcrumb usa `AppHeader.vue` com ícone chevron-right como separador; `Breadcrumb.vue` não é usado no layout principal (só no App.vue para login/setup). | **Aceito** | Implementar correção no `AppHeader.vue` que é o componente ativo. |
| A3 | Padding do header (`px-4`) não alinha com padding do conteúdo (`p-4 md:p-6`). Solução: `px-4 md:px-6`. | **Aceito** | Aplicar no `AppHeader.vue`. |

## Goldfish Review S (SOLID, Coesão)

| # | Achado | Classificação | Resolução |
|---|--------|---------------|-----------|
| S1 | Renomear "Séries" → "Biblioteca" em 5 arquivos mantém coesão (mesmo conceito). | **Aceito** | Proseguir. |
| S2 | `SeriesView.vue` linha 113 conta "séries" - mudar para "itens" ou manter? Usuário disse "tudo que é Séries para Biblioteca". | **Parcial** | Manter "séries" como contagem de itens da biblioteca (termo do domínio Plex). Não renomear a contagem. |
| S3 | `SidebarArea.vue` item "Biblioteca" navega para `/libraries` (rota `libraries`). Consistente. | **Aceito** | Proseguir. |

## Goldfish Review C (Clean Code, Escopo, Testabilidade)

| # | Achado | Classificação | Resolução |
|---|--------|---------------|-----------|
| C1 | Testes de breadcrumb precisam atualização (2 asserções). | **Aceito** | Atualizar `breadcrumb.test.mjs` linhas 84, 96. |
| C2 | Mudança de fallback "Série" → "Biblioteca" pode confundir usuários? O breadcrumb mostra "Bibliotecas > Biblioteca indisponível" - o segundo nível é a biblioteca selecionada. Usar "Biblioteca" é correto (singular da biblioteca atual). | **Aceito** | Proseguir. |
| C3 | Padding CSS `px-4 md:px-6` é minimal, não quebra responsividade. | **Aceito** | Proseguir. |

## Decisões
- Escopo confirmado: 6 arquivos alterados (5 JS/Vue + 1 test).
- Nenhuma quebra de contrato ou API.
- Build gate e QA Playwright obrigatórios após implementação.