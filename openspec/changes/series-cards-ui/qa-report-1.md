# QA Report - Iteração 1

**Status:** PASS  
**URL de Validação:** `http://localhost:8080`  
**Data:** 2026-11-14

## Critérios Verificados

1. **Tema (light/dark toggle e persistência):** PASS. Alternância via botão de tema altera `html[data-theme]` corretamente e persiste no localStorage/API.
2. **Menus e Navegação:** PASS. Breadcrumb exibe trilha correta em `/libraries`, menu do usuário abre/fecha via aria-expanded.
3. **Cards de Séries (Streaming UI):** PASS. Grid renderiza com aspect ratio 2/3, cantos arredondados, título sobreposto com gradiente, animação de zoom no hover, clique navega para `/series/{seriesKey}/markers`.
4. **Validação Visual Dark/Light nos Cards:** PASS. Adaptação correta de classes `dark:` e `bg-white`/`dark:bg-gray-800`.

## Evidências
- 5 testes Playwright executados com sucesso (tema, breadcrumb, menu do usuário, renderização dos cards, tema dark/light).
