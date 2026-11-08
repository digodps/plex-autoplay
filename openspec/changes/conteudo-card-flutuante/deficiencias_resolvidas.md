# EGM — Achados e resolução

Goldfish Review A/S/C sobre `conteudo-card-flutuante` (artefatos e shell atual).

Legenda: Aceito, Parcial, Rejeitado.

| # | Achado | Classificação | Resolução |
|---|---|---|---|
| A1 / S1 | 5 views pintam `bg-gray-50 dark:bg-gray-900 min-h-screen` e anulam o card | Aceito | Task 1.4 remove fundo próprio e `min-h-screen` dessas views |
| A2 | `scrollBehavior` não reseta o scroller interno na troca de rota | Aceito | Task 1.2 adiciona `ref` no scroller + `watch(route.fullPath)` para `scrollTop = 0` |
| A3 / S3 / C4 | Card precisa de `overflow-hidden` para recortar cantos | Aceito | Card é o próprio scroller (`overflow-y-auto` + `rounded-2xl`); sem camada extra |
| A4 / S5 / C8 | `sticky top-0` do header vira no-op ao sair do scroller | Aceito | Header passa a `relative z-30 shrink-0`, sem `sticky` e sem `border-b` |
| A5 / C3 | `body` branco aparece no overscroll | Aceito | `body` do `style.css` passa a `bg-gray-50 dark:bg-gray-900` |
| A6 / S6 | `shadow-card` é quase invisível no escuro | Aceito | Trocar por `shadow-default` |
| A7 | Headers internos das views são sticky com borda e fundo próprio | Aceito | Task 1.4 remove `sticky`/`border-b`/fundo próprio desses headers |
| A8 | `SyncView` tem scroller aninhado com barra visível | Aceito | Task 1.4 adiciona `no-scrollbar overscroll-contain` ao log |
| A9 | `h-screen` estoura viewport mobile | Aceito | Shell e task usam `h-dvh` |
| A10 / S4 / C6 / C7 | Spec com cláusula duplicada, cenário escuro incompleto e prosa no requisito | Aceito | Spec reescrita com tokens exatos e cenários espelhados |
| A11 | `rounded-2xl` diverge de `--radius` | Rejeitado | Card é superfície externa; `rounded-2xl` é intencional |
| S2 | Header não deve ser dono do fundo do shell | Aceito | Header fica transparente; fundo vem do shell |
| S7 | `PasswordModal` dentro do header | Rejeitado | Comportamento existente fora do escopo visual do card |
| S8 | Extrair `useBreadcrumbs()` | Rejeitado | Refatoração não relacionada; aumenta diff |
| S9 / C10 | Design repete testes e Impact cita `style.css` especulativo | Parcial | Impact mantém `style.css` (necessário para `body`); lista de QA fica só nas tasks |
| C1 | Sem teste estático de regressão | Parcial | QA Playwright valida estilos computados; teste estático de classes seria frágil |
| C2 | Sem cenário para barra de rolagem invisível | Aceito | Cenário "Header fixo e rolagem interna" cobre `scrollbar-width`/barra invisível |
| C5 | QA subjetivo | Aceito | Task 2.4 fixa asserts: cor computada, `offsetWidth - clientWidth`, `scrollTop` |
| C9 | `h-full` extra no scroller | Aceito | Card é o scroller; uma camada a menos |
| C11 | Margem do card `m-2` vs padding do header `px-4` | Rejeitado | Alinhamento intencional: margem do card + padding próprio do header |
| C12 | Rolagem por teclado sem foco | Parcial | Card recebe rolagem nativa por roda/touch; `tabindex` exigiria gerenciar foco de toda view |

## Decisões registradas

- Sem dependências novas; `no-scrollbar` e `shadow-default` já existem.
- Card é o scroller: `overflow-y-auto no-scrollbar` + `rounded-2xl` no mesmo elemento.
- Views perdem fundo próprio para o tom do card prevalecer em todas as rotas.
