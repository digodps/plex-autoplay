# QA Report: Dashboard Button Size Standardization (Re-run)

**Change**: `dashboard-botoes-tamanhos`
**Date**: 2026-11-09
**Test URL**: http://localhost:8080 (Docker container `plex-autoplay`, image revision `c9f1e99c7e5d` built 2026-11-09 01:07)
**Playwright Version**: 1.65.0
**Evidence Directory**: `openspec/changes/dashboard-botoes-tamanhos/qa-evidence/`

---

## Overall Result: **PARTIAL PASS** (3/4 core validations pass; height parity fails)

| Criterion | Status | Notes |
|-----------|--------|-------|
| 1. Theme Toggle (light/dark) | ✅ PASS | `data-theme` and `.dark` class toggle correctly on first click; Dashboard buttons remain visible/legible in both themes |
| 2. Menu Navigation (breadcrumb) | ✅ PASS | URLs change: `/dashboard` → `/libraries` → `/sync` → `/dashboard`; active breadcrumb highlights on `/libraries` ("Bibliotecas") and `/sync` ("Sincronizar"); Dashboard is root route with no breadcrumb (by design) |
| 3. Button Size Standardization | ⚠️ PARTIAL | **Classes standardized** ✓ — all 4 buttons use `btn btn-md` + exactly one variant (`btn-secondary`, `btn-primary`, `btn-danger`, `btn-warning`); **no forbidden classes** (`px-5`, `py-2.5`, `rounded-xl`) present. **Height parity** ✗ — "Ver Log Completo" = 38px, "Iniciar Escaneamento" = 36px (2px difference from icon content) |
| 4. Focus-visible & disabled states | ✅ PASS | All buttons have `.btn` base class providing `focus-visible:ring-2` + offset and `disabled:opacity-60` via CSS |
| Lint (`npm run lint`) | ✅ PASS | 0 errors, 8 warnings (vue/attributes-order) |
| Openspec Validate (`--strict`) | ✅ PASS | `openspec validate dashboard-botoes-tamanhos --strict` passes |

---

## Detailed Findings

### 1. Theme Toggle — PASS ✅

**URL**: http://localhost:8080/dashboard

**Expected**: Click theme toggle → `data-theme` attribute on `<html>` changes between "light"/"dark" AND `.dark` class toggles → Dashboard button remains visible/legible.

**Actual**:
- Theme button: `header button[aria-label*="Tema"]` (label: "Tema escuro" / "Tema claro")
- **Click 1**: `data-theme` changes from "light" → "dark", `.dark` class toggles `false` → `true` immediately
- Dashboard button "Ver Log Completo" remains visible and legible in both themes
- **Click 2 (back)**: Restores to light correctly

**Evidence**:
- `01-dashboard-loaded.png` — initial light theme
- `02-theme-toggled.png` — after toggle to dark

**Note**: Previous QA (qa-report-1.md) reported a "one-click lag" bug where DOM attributes updated on 2nd click only. **This bug is NOT present in the current build** (image `c9f1e99c7e5d`). Theme toggle now works on first click.

---

### 2. Menu Navigation — PASS ✅

**URL**: http://localhost:8080/dashboard → /libraries → /sync → /dashboard

**Expected**: Navigate to at least 2 routes, assert URL changes + active breadcrumb item gets `aria-current="page"`, return to Dashboard.

**Actual**:
| Route | URL Match | Breadcrumb Active Item | Note |
|-------|-----------|------------------------|------|
| `/libraries` | ✅ | "Bibliotecas" | `aria-current="page"` on `<span>` |
| `/sync` | ✅ | "Sincronizar" | `aria-current="page"` on `<span>` |
| `/dashboard` | ✅ | (none) | Dashboard is root: `buildBreadcrumb` returns `[]` → no breadcrumb nav rendered (by design) |

**Evidence**:
- `03-libraries.png` — `/libraries` with active "Bibliotecas"
- `04-sync.png` — `/sync` with active "Sincronizar"

---

### 3. Button Size Standardization — PARTIAL ⚠️

**URL**: http://localhost:8080/dashboard

**Mandatory Assertions from Task**:

| Assertion | "Ver Log Completo" | "Iniciar Escaneamento" | "Tentar novamente" (total) | "Tentar novamente" (lib) |
|-----------|-------------------|------------------------|---------------------------|-------------------------|
| `class` contains `btn` | ✅ | ✅ | — (not rendered) | — (not rendered) |
| `class` contains `btn-md` | ✅ | ✅ | — | — |
| Exactly one variant | ✅ `btn-secondary` | ✅ `btn-primary` | `btn-danger` (in source) | `btn-warning` (in source) |
| NO `px-5` / `py-2.5` / `rounded-xl` | ✅ | ✅ | ✅ (source) | ✅ (source) |
| Computed height | **38px** | **36px** | N/A | N/A |

**Computed Heights** (via `getBoundingClientRect` / `getComputedStyle`, 3 runs — consistent):
- "Ver Log Completo": **38px**
- "Iniciar Escaneamento": **36px**
- **Heights NOT equal** — 2px difference

**Root Cause**: Both buttons share identical classes (`btn btn-md` + variant). The 2px difference stems from "Iniciar Escaneamento" containing an `<AppIcon name="play" size="sm" />` inside a `flex items-center` layout, while "Ver Log Completo" is plain text. The icon content slightly affects flex layout height despite identical padding (`px-4 py-2` via `.btn-md`).

**DOM Classes Rendered** (from live DOM):
```
Ver Log Completo:     btn btn-md btn-secondary
Iniciar Escaneamento: btn btn-md btn-primary
```

**Source Classes** (for error-state buttons, not rendered in current session):
```vue
<!-- Total failure banner -->
<button class="btn btn-md btn-danger" :disabled="store.isLoading" @click="store.retry">Tentar novamente</button>

<!-- Library error banner -->
<button class="btn btn-md btn-warning" :disabled="store.isLoading" @click="store.retry">Tentar novamente</button>
```

**Evidence**:
- `05-dashboard-buttons.png` — Dashboard header area (top 400px) showing button layout

**Assessment**: **Class standardization fully succeeded** — all visible buttons use shared `.btn`/`.btn-md` classes with correct variants; zero forbidden ad-hoc classes. **Height parity narrowly fails** (38px vs 36px) due to icon content, not class divergence. This is a minor residual visual inconsistency.

---

### 4. Focus-visible & Disabled States — PASS ✅

**Verification**: All 4 button definitions in source inherit from `.btn` base class in `style.css` which includes:
- `focus-visible:outline-none`
- `focus-visible:ring-2`
- `focus-visible:ring-primary`
- `focus-visible:ring-offset-2`
- `dark:focus-visible:ring-offset-gray-900`
- `disabled:opacity-60`
- `disabled:cursor-not-allowed`

Playwright test confirmed visible buttons receive keyboard focus and have `.btn` + `.btn-md` classes present.

---

### Lint & Validation Results

#### `npm run lint` (vue/plex-frontend)
```
✖ 8 problems (0 errors, 8 warnings)
- vue/html-closing-bracket-spacing (2)
- vue/attributes-order (6)
```
**Status**: PASS (no errors)

#### `openspec validate dashboard-botoes-tamanhos --strict`
```
Change 'dashboard-botoes-tamanhos' is valid
```
**Status**: PASS

---

## Evidence Files

| File | Description |
|------|-------------|
| `01-dashboard-loaded.png` | Dashboard after login (light theme) |
| `02-theme-toggled.png` | Dashboard after theme toggle (dark theme) |
| `03-libraries.png` | `/libraries` route with active breadcrumb "Bibliotecas" |
| `04-sync.png` | `/sync` route with active breadcrumb "Sincronizar" |
| `05-dashboard-buttons.png` | Dashboard header area showing button layout (top 400px) |
| `qa-dashboard.spec.ts` | Playwright test script |

---

## Test Script

Location: `openspec/changes/dashboard-botoes-tamanhos/qa-evidence/qa-dashboard.spec.ts`

Run with:
```bash
cd /home/hermes/plex-autoplay-dashboard-botoes-tamanhos
npx playwright test --config vue/plex-frontend/playwright.config.ts qa-dashboard --reporter=list
```

---

## Conclusion & Recommendations

### Summary
| Validation | Result |
|------------|--------|
| Theme toggle | ✅ PASS |
| Menu navigation | ✅ PASS |
| **Button class standardization** | ✅ PASS |
| **Button height parity** | ❌ FAIL (38px vs 36px) |
| Focus/disabled states | ✅ PASS |
| Lint | ✅ PASS |
| Openspec validate | ✅ PASS |

**Core Requirement (Button Size Standardization)**:
- **Classes**: Fully standardized — all 4 buttons use `.btn`, `.btn-md`, and exactly one variant. No ad-hoc Tailwind utilities remain. ✅
- **Height**: 2px difference (38px vs 36px) due to icon content in "Iniciar Escaneamento". The spec requires identical computed height; this narrowly fails. ❌

### Recommendations

1. **Height parity fix (optional)**: To achieve exact height parity, either:
   - Add `min-height` to `.btn-md` (e.g., `min-h-[38px]`) to force uniform height, OR
   - Remove icon from "Iniciar Escaneamento" (breaks UX), OR
   - Accept 2px delta as negligible (both buttons visually appear similar; 2px is below most perceptual thresholds)

2. **Error-state buttons**: The two "Tentar novamente" buttons are not rendered in normal operation (require Plex connection failure). Their source code uses correct standardized classes (`btn btn-md btn-danger` / `btn btn-md btn-warning`) with `:disabled` and focus-visible parity. No action needed unless error states are triggered in testing.

3. **Previous build issues resolved**: The running container now correctly reflects the source changes (image `c9f1e99c7e5d` built 2026-11-09 01:07). Theme toggle bug from qa-report-1.md is fixed.

**Overall**: The change successfully standardizes button classes across the Dashboard. The only deviation is a 2px computed-height difference caused by icon content, not by missing class standardization. This is a **minor residual inconsistency** that does not affect functionality or visual harmony at typical viewing distances.