# QA Report 1

- **Resultado:** PASS
- **Change:** `conteudo-card-flutuante`
- **URL validada:** http://localhost:8097 (container isolado `plex-card-qa`, imagem `plex-autoplay:conteudo-card-flutuante`)
- **Navegador:** Playwright/Chromium, viewport 1280x800
- **Evidências:** `qa-light.png`, `qa-dark.png`

## Checks

| # | Check | Evidência |
|---|---|---|
| 1 | Login chega ao `/dashboard` | PASS |
| 2 | Card de conteúdo e header renderizados | PASS |
| 3 | Tema claro aplicado | `data-theme=light` |
| 4 | Card claro `white` sobre body `gray-50` | `rgb(255,255,255)` vs `rgb(248,249,250)` |
| 5 | Cantos arredondados e margem de flutuação | `border-radius: 16px`, `x=12`, `y=60` |
| 6 | Card preenche largura útil | `1256px` em viewport de `1280px` |
| 7 | Barra de rolagem invisível no claro | `scrollWidth-clientWidth=0`, `scrollbar-width=none` |
| 8 | Tema escuro aplicado | `data-theme=dark` |
| 9 | Card escuro `gray-800` sobre body `gray-900` | `rgb(33,38,45)` vs `rgb(24,24,27)` |
| 10 | Barra de rolagem invisível no escuro | `diff=0` |
| 11 | Rolagem dentro do card | `scrollTop=400` |
| 12 | Header fixo durante rolagem | bounding box idêntico antes/depois |
| 13 | Janela não rola | `window.scrollY=0` |
| 14 | Menu do usuário abre e mostra opções | `aria-expanded=true` |
| 15 | Navegação pelo menu chega a `/settings` | PASS |
| 16 | Menu fecha após navegar | `aria-expanded=false` |
| 17 | Troca de rota reseta scroll do card | `300 -> 0` |
| 18 | Sem erros de página | 0 `pageerror` |

## Regressões

Nenhuma. Build .NET, lint e build do frontend passaram antes do QA.

## Docker acceptance

- `docker build --no-cache -t plex-autoplay:conteudo-card-flutuante .` concluído.
- Container isolado `plex-card-qa` executado em `8097` com cópia do banco (`/tmp/opencode/plex-card-data`).
- `/api/status` respondeu `401` (esperado sem sessão); `/api/auth/setup/status` respondeu `setupRequired=false`.
- QA revalidado após rebase em `origin/main` (`04e4d91`): 23/23 PASS; screenshots atualizados.
