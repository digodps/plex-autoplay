# QA Report 1 — remover-efeito-logo

**Status: PASS**

## Ambiente validado

- **URL de validação acessada:** `http://127.0.0.1:8080` (container `infra-plex-autoplay-1`, imagem `plex-autoplay:latest` @ commit `70cf2ed`)
- Build: `./scripts/build.sh` — imagem OK, health check `/settings` HTTP 200
- Playwright: `tests/e2e/tests/qa-remover-efeito-logo.spec.js`, chromium headless real

## Critérios verificados

| # | Critério | Resultado | Evidência |
|---|----------|-----------|-----------|
| 1 | Logo renderiza sem lightning bolt | ✅ PASS | `path.logo-lightning` count = 0 |
| 2 | Logo SVG tem exatamente 2 paths (triângulo + linha vertical) | ✅ PASS | `svg path` count = 2 |
| 3 | Logo sem animação (animationName === "none") | ✅ PASS | computedStyle check |
| 4 | Tema light/dark aplicado corretamente ao logo | ✅ PASS | `data-theme=light/dark` + logo visível |
| 5 | Tamanhos de logo renderizam corretamente | ✅ PASS | boundingBox width/height > 0 |
| 6 | Menu do usuário (abre/fecha/navega) | ✅ PASS | `aria-expanded` alterna; navegação p/ `/settings` |
| 7 | Tema: toggle system→light→dark (regression) | ⚠️ FAIL (legado) | Falha pré-existente não relacionada à mudança |

**Resultado:** 6/7 passaram. A falha no item 7 é um bug legado do ciclo de tema (reportado no QA anterior como "legado"), não introduzido por esta mudança.

## Arquivos alterados

- `vue/plex-frontend/src/components/AppLogo.vue` — removido path lightning, animationDelay, keyframes, prefers-reduced-motion
- `tests/e2e/tests/qa-electric-logo-animation.spec.js` — deletado (teste órfão do recurso removido)
- `tests/e2e/tests/qa-remover-efeito-logo.spec.js` — adicionado (evidência QA desta mudança)
- `playwright.config.ts` — adicionado (config Playwright correta para QA)

## Build & Lint

- `npm run lint` em `vue/plex-frontend`: **PASS** (AppLogo.vue limpo; 11 erros pré-existentes em arquivos não alterados)
- `npm run build` em `vue/plex-frontend`: **PASS**
- `dotnet build PlexAutoplay.slnx -c Release`: **PASS**
- `./scripts/build.sh`: **PASS** (imagem Docker `plex-autoplay:latest` construída)
- `openspec validate remover-efeito-logo --strict`: **PASS**

## Evidências Playwright

- Trace/screenshots: `test-results/qa-remover-efeito-logo-*/`
- URL acessada: `http://127.0.0.1:8080`

## Conclusão

A feature "remover efeito logo" está implementada, compilada, publicada em imagem Docker e validada em navegador real. O logo voltou ao estado estático original (triângulo + linha vertical). QA **PASS** para o escopo do spec.