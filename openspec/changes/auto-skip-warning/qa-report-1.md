# QA Report — auto-skip-warning

## Escopo
Change: `auto-skip-warning` (aviso de autorização antes do auto-skip, estilo Netflix).

## Build gate
- `dotnet build PlexAutoplay.slnx -c Release` → **PASS**
- `npm run build` (vue/plex-frontend) → **PASS**
- `npx eslint SettingsView.vue` → **PASS** (0 errors, 5 warnings de ordenação/spacing)
- `openspec validate auto-skip-warning --strict` → **PASS**

## Testes
- `dotnet test` (persistence/settings filtrados) → **PASS** (4/4)
- Suíte completa: 31/34 passam. 3 falhas em `PlexModernClientDetectorTests` **pré-existentes** (não tocadas por esta change; arquivo não modificado).
- Persistência nova chave validada em `SettingsPersistenceTests`.

## QA UI (Playwright) — PASSOU
Script `tests/e2e/qa-autoskip-ui-check.mjs` estendido com o toggle/modal de autorização. Executado contra stack real (Docker + Plex real `192.168.18.5:32400`) em `http://localhost:8080`.

```
PASS  Página de login carrega
PASS  Login redireciona para /dashboard
PASS  Toggle de tema presente na header
PASS  Toggle alterna o tema (muda data-theme/class) | false -> true
PASS  Toggle reverte o tema | true -> false
PASS  data-theme reflete o estado efetivo | data-theme=light
PASS  Botão do menu de usuário presente
PASS  Menu de usuário abre
PASS  Menu exibe estado expandido (aria-expanded)
PASS  Menu de usuário fecha
PASS  Breadcrumb mostra item ativo
PASS  Navegação via breadcrumb (sem links na rota atual) | sem link no breadcrumb
PASS  Checkbox 'Autorizar skip automático' presente
PASS  Toggle começa OFF
PASS  Modal Netflix aparece ao tentar ligar o toggle
PASS  Cancelar mantém toggle OFF
PASS  GET /api/settings mantém auto_skip_authorized=false
PASS  Modal Netflix reaparece ao tentar ligar de novo
PASS  Confirmar faz toggle virar ON
PASS  GET /api/settings retorna auto_skip_authorized=true
Total: 20, Passed: 20, Failed: 0  → RESULT: PASS
```

Correções descobertas pela QA e aplicadas:
- `toggleAutoSkip()` reescrito: desligar salva direto; ligar reverte visualmente e abre modal (antes: uncheck não salvava por `:checked` + `return` precoce).
- Removida linha duplicada do toggle (`id="auto-skip-authorized"` duplicado).
- Assets Vue copiados para `src/PlexAutoplay.Web/wwwroot` antes do build Docker (Dockerfile consome wwwroot pré-buildado).

## Veredito
**PASS** — build, lint, spec, unit e QA Playwright de navegador (tema light/dark, menu, breadcrumb, toggle + modal) verificados.
