# EGM — deficiências resolvidas (Goldfish Review A/S/C)

## A — Arquitetura / integração

| # | Achado | Status | Motivo / resolução |
|---|--------|--------|--------------------|
| A1 | Header ainda contém controles de tema/logout | Rejeitado como achado | É o próprio escopo do change; implementado. |
| A2 | Sidebar sem tema/logout | Rejeitado como achado | É o próprio escopo; implementado. |
| A3 | `isCollapsed` ausente no store | Rejeitado como achado | É o próprio escopo; implementado com `sidebar.collapsed`. |
| A4 | SidebarItem ignora colapso | Rejeitado como achado | É o próprio escopo; recebe prop `collapsed`. |
| A5 | Toggle de compacto ausente | Rejeitado como achado | Implementado com ícone `menu`. |
| A6 | Chaves de persistência fragmentadas | Parcial | Aceito apenas como documentação: `sidebar.collapsed` é escopo próprio; tema mantém `theme.mode`. Nenhuma unificação necessária. |

## S — SOLID / coesão

| # | Achado | Status | Motivo / resolução |
|---|--------|--------|--------------------|
| S1 | SidebarArea vira god component | Rejeitado | Arquivo tem ~90 linhas e um único dono (navegação + shell). Extrair `SidebarControls`/`SidebarFooter` para dois usos inexistentes viola YAGNI. |
| S2 | Store mistura estado transiente e preferência; membros mortos | Aceito (parte morta) | Removidos `page`, `selected`, `isLoading`, `setPage`. `isCollapsed` inicializa direto do `localStorage`, sem estado intermediário. |
| S3 | Lógica de tema duplicada; `cycleMode` morto | Aceito | Adicionado `theme.toggle()`; `cycleMode` removido. |
| S4 | SidebarItem lê store só para fechar drawer, duplicando handler do `<ul>` | Aceito | Store removida do item; `@click="sidebar.closeSidebar"` permanece só no `<ul>`. |
| S5 | `aria-label` no SVG é ignorado (`AppIcon` tem `aria-hidden`) | Aceito | Nome acessível movido para o `<router-link>` (`aria-label` + `title`). |
| S6 | Hambúrguer inline quebrado + duplicado | Aceito | Ícone `menu` adicionado ao `AppIcon`; botão inline quebrado da sidebar removido. |

## C — Clean Code / escopo / testabilidade

| # | Achado | Status | Motivo / resolução |
|---|--------|--------|--------------------|
| C1 | Delta de spec inválido (`openspec validate --strict`) | Aceito | Movido para `specs/navegacao-lateral/spec.md` com `## ADDED Requirements` e 5 `#### Scenario:`. `validate --strict` passa. |
| C2 | `aria-label` no ícone descartado | Aceito | Mesma correção de S5. |
| C3 | Dois hambúrgueres / `isSidebarOpen` morto | Parcial | Hambúrguer quebrado da sidebar removido; `isSidebarOpen` é usado pela animação do header no mobile e permanece. |
| C4 | Limpeza do header incompleta nas tasks | Aceito | Removidos `useRouter`, `useThemeStore`, `useAuthStore`, `AppIcon`, `handleLogout`, `toggleTheme` e `div` vazio. Lint sem `no-unused-vars`. |
| C5 | Policies de boundaries em sintaxe legada | Rejeitado | Pré-existente, fora do escopo; só emite warning de migração. Registrado como dívida. |
| C6 | Sem teste automatizado da persistência | Parcial | Verificação executável feita manualmente via build + lint; adicionar e2e de sidebar quando existir suíte e2e de UI. Registrado como próximo passo. |

## EGM — iteração 2 (empilhamento do tema, remoção do header, item ativo)

### A — Arquitetura

| # | Achado | Status | Motivo / resolução |
|---|--------|--------|--------------------|
| A2.1 | Remover o header mata o único controle de menu mobile | Aceito (parcial) | O botão do header só animava `isSidebarOpen`, estado que nenhum componente consumia para exibir/ocultar a sidebar. Removidos `isSidebarOpen`, `toggleSidebar`, `closeSidebar` e o handler no `<ul>`. Drawer off-canvas no mobile fica como próximo passo (fora do escopo). |
| A2.2 | Item ativo falha em rota aninhada e no modo compacto | Aceito | `isActive` passou a casar prefixo (`route.path === to \|\| startsWith(to + "/")`) e o destaque foi movido para o `<router-link>` (visível no modo compacto). |
| A2.3 | Tema não fica em linha própria | Aceito | Container dos controles agora é `flex flex-col` fixo. |
| A2.4 | Delta conflita com o spec main (`navegacao-lateral` exige rail 88px/`AppSidebar.vue`) | Rejeitado | Spec main já estava defasado antes deste change (descreve componente que não existe). Corrigi-lo é change separado. |
| A2.5 | `isCollapsed` persistido vaza para drawer mobile | Rejeitado | Não existe drawer mobile; sidebar é in-flow. Sem efeito. |

### S — SOLID

| # | Achado | Status | Motivo / resolução |
|---|--------|--------|--------------------|
| S2.1 | Header ainda renderizado | Aceito | Removido de `DefaultLayout.vue`; `HeaderArea.vue` deletado. |
| S2.2 | Botões do topo não empilham | Aceito | Mesma correção de A2.3. |
| S2.3 | Markup duplicado dos dois botões | Rejeitado | Dois botões apenas; extrair componente com um consumidor de variante viola YAGNI. Estilo compartilhado por uma constante `actionClasses`. |
| S2.4 | Estado ativo por igualdade exata | Aceito | Mesma correção de A2.2. |
| S2.5 | Destaque ativo incompleto | Aceito | `bg-primary text-white` no link, incluindo modo compacto. |

### C — Clean Code / a11y

| # | Achado | Status | Motivo / resolução |
|---|--------|--------|--------------------|
| C2.1 | Contraste do destaque no escuro (2.89:1) | Aceito | `bg-primary text-white` = 6.15:1 nos dois temas. |
| C2.2 | Nenhum foco visível; design cita classes inexistentes | Aceito | `focus-visible:ring-2 ring-primary ring-offset-2` em links e botões; design corrigido. |
| C2.3 | Header não removido; tasks desatualizadas | Aceito | Tasks 12-16 adicionadas e concluídas. |
| C2.4 | Deleção do header deixa estado morto | Aceito | Removidos `isSidebarOpen`/`toggleSidebar`/`closeSidebar`. |
| C2.5 | Classes de hover inválidas (`hover:hover`, `dark:hover:bg-stroke`) | Aceito | Corrigido para `hover:bg-stroke dark:hover:bg-strokedark`. |
| C2.6 | Sem teste executável do item ativo | Rejeitado | Regra é uma linha de prefixo; teste exigiria `@vue/test-utils` (dependência nova). Coberto por build + verificação manual. |
