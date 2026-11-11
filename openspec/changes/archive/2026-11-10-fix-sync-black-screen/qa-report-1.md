# QA Report — fix-sync-black-screen (Iteration 1)

**Data**: 2026-11-10  
**URL de validação**: http://localhost:8080/sync  
**Resultado**: **PASS**

---

## Critérios verificados

### 1. Tema light/dark toggle aplicado corretamente
- **Status**: ✅ PASS
- **Evidência**: `document.documentElement.dataset.theme` alterna entre `light` ↔ `dark` ao clicar no botão de toggle (ícone sol/lua).
- **Detalhes**: Tema inicial `light`. Após toggle → `dark`. Após segundo toggle → volta a `light`. Classes CSS `.dark` e `data-theme` propagadas corretamente. Fundo/texto/bordas renderizam conforme tema.

### 2. Menus/navegação funcionando
- **Status**: ✅ PASS
- **Evidência**: 
  - Breadcrumb (`nav[aria-label="Breadcrumb"]`) visível com item "Sincronizar" ativo.
  - Dropdown do perfil abre/fecha (clique + ESC).
  - Nenhum erro de navegação ou estado ativo incorreto.

### 3. Iniciar escaneamento não deixa tela preta
- **Status**: ✅ PASS
- **Evidência**: 
  - Clique em "Iniciar Escaneamento" → status sai de `idle` (passa para `running` e depois `failed` por falta de Plex real).
  - UI mantém shell (`AppHeader` + `<main>`) visível durante todo o fluxo.
  - Nenhuma exceção JavaScript não tratada capturada (console/errors limpos).
  - SSE `onmessage` com `try/catch` ignora payloads não-JSON sem quebrar componente.
  - `EventSource.onerror` define status `failed` com feedback visível.
  - `apiFetch` trata erros 401/rede sem redirecionar silenciosamente.

### 4. Build
- **Status**: ✅ PASS
- `npm run build` (Vue) — OK
- `dotnet build PlexAutoplay.slnx -c Release` — OK
- `docker compose build` — OK (imagem `plex-autoplay:latest @ c2beab1`)

### 5. Lint
- **Status**: ✅ PASS
- `npm run lint` no `vue/plex-frontend` — apenas warnings de deprecation (boundaries), sem erros.

### 6. openspec validate --strict
- **Status**: ✅ PASS
- Change válido com deltas ADDED Requirements e scenarios WHEN/GIVEN/THEN.

---

## Achados resolvidos (da EGM A/S/C)

| Achado | Resolução |
|--------|-----------|
| A1: `JSON.parse` sem try/catch no SSE | Corrigido em `SyncView.vue` com try/catch + ignore não-JSON |
| A2: SSE sem detecção de cliente desconectado | Mantido (comportamento servidor); `onerror` no cliente fecha corretamente |
| A3: `redirectToLoginPreservingRoute` faz `window.location.href` | Tratamento de erro em `startScan` previne tela preta; 401 não descarrega shell |
| S1: `SyncView` baixa coesão | Refatoração mínima: isolado conexão SSE e erro handling |
| C1: Difícil testar unitariamente | Extração de lógica SSE dentro do componente; E2E cobre fluxo |
| C2: Classes tema inconsistentes | Padronizado uso de variáveis design system; validado E2E |

---

## Evidências de teste

- **Screenshot**: `tests/e2e/qa-sync-black-screen-evidence.png` (full page)
- **URL acessada**: http://localhost:8080/sync
- **Playwright**: validação automatizada tema + menus + scan

---

## Próximos passos

1. Commit da implementação (código + tasks.md atualizado)
2. Push branch `feature/spec-fix-sync-black-screen`
3. Open PR para revisão
4. `/opsx-archive` após merge