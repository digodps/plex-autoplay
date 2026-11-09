# QA Report — autoskip-clientes-modernos

URL acessada: **http://localhost:8453** (container `plex-autoplay:autoskip-clientes-modernos`, build --no-cache) via Playwright.

## Passo a passo da validação UI obrigatória

1. Acesso `/login` → formulário visível ✓
2. Login senha `admin123` → redireciona para `/dashboard` com JWT ✓
3. Toggle de tema na header: alterna `class dark` e `data-theme` ✓ (testes falsa→verdadeira→falsa)
4. Botão menu de usuário: abre (`aria-expanded=true`), fecha ✓
5. Breadcrumb mostra item ativo (`aria-current=page`) ✓

## Resultado

| # | Critério | Status |
|---|---|---|
| 1 | Login página carrega | PASS |
| 2 | Login redireciona /dashboard | PASS |
| 3 | Toggle de tema presente | PASS |
| 4 | Toggle alterna data-theme | PASS |
| 5 | Toggle reverte tema | PASS |
| 6 | data-theme reflete estado | PASS |
| 7 | Botão menu do usuário | PASS |
| 8 | Menu abre | PASS |
| 9 | Menu aria-expanded expandido | PASS |
| 10 | Menu fecha | PASS |
| 11 | Breadcrumb item ativo | PASS |
| 12 | Navegação/breadcrumb intacta | PASS |

**RESULT: PASS** (12/12) — Não há regressão no frontend, nenhum menu quebra.

## Achados numerados

1. `IsBrokenClient` removido; lista hardcoded de versão quebrada substituída por categoria dinâmica. Detectado pelos testes unitários (`PlexModernClientDetectorTests`, 12 asserts).
2. Resposta não-XML (OK, XML parcial) tratada por `IsSeekSuccessAsync`, cobrada em 4 casos.
3. Detector moderno mapeia Web / Desktop / SmartTV → PMS proxy-only; Android/Roku continuam direto. Coberto por 9 cenários + IsSeekSuccessAsync 4 casos.

## URL acessada como evidência
http://localhost:8453
