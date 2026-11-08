# QA Report — Change `icone-marca-discreto`

URL de validação: http://localhost:8443 (container `plex-autoplay` via `docker-compose.test.yml`, porta 8443 → 80)
Data: 2026-11-08
It. 1 de 5

## Resultado: PASS

## Evidências (Playwright)

Todos os checks passaram com navegador real (Chromium) contra a URL acima:

| Check | Resultado | Detalhe |
|---|---|---|
| Login → dashboard | PASS | redireciona para `/dashboard` |
| Logo sem `rect` de fundo | PASS | `rects=0` (fundo transparente) |
| Logo usa traço (stroke) | PASS | `strokePaths=2` |
| Logo `currentColor` (adapta tema) | PASS | `currentColorPaths=2` |
| Logo discreta (≤32px) | PASS | `28x28` |
| Toggle de tema altera `data-theme` | PASS | `light → dark` |
| Menu usuário abre | PASS | link Configurações visível |
| Menu navega → Ajustes | PASS | `http://localhost:8443/settings` |
| Menu fecha após navegação | PASS | `open=false` |
| Breadcrumb marca item atual | PASS | `aria-current="page"` = "Ajustes" |
| Sem erros de runtime | PASS | `pageErrors.length === 0` |

Screenshot: `/tmp/opencode/qa/01-dashboard-theme.png`, `/tmp/opencode/qa/02-dashboard-final.png`

## Conclusão

O ícone da marca agora é um traço fino com fundo transparente (não mais bloco sólido com gradiente), ocupa 28×28 px, adapta-se ao tema claro/escuro, e o menu/navegação/tema continuam funcionando. QA **PASS**.
