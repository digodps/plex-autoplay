# QA Report: Dashboard Button Size Standardization

**Change**: `dashboard-botoes-tamanhos`
**Date**: 2026-11-09
**Test URL**: http://localhost:8080 (Docker container `plex-autoplay`)
**Playwright Version**: 1.65.0
**Evidence Directory**: `openspec/changes/dashboard-botoes-tamanhos/qa-evidence/`

---

## Overall Result: **FAIL**

| Criterion | Status | Notes |
|-----------|--------|-------|
| 1. Theme Toggle (light/dark) | ❌ FAIL | Single click does not update `data-theme`/`.dark`; requires 2 clicks |
| 2. Menu Navigation | ✅ PASS | URLs change, breadcrumb active state works, return to dashboard works |
| 3. Button Size Standardization | ❌ FAIL | Running app uses old ad-hoc Tailwind classes; no `btn`/`btn-md` classes |
| Lint (`npm run lint`) | ✅ PASS | 0 errors, 8 warnings (vue/attributes-order) |
| Openspec Validate | ✅ PASS | `openspec validate dashboard-botoes-tamanhos --strict` passes |

---

## Detailed Findings

### 1. Theme Toggle — FAIL

**URL**: http://localhost:8080/dashboard

**Expected**: Click theme toggle button → `data-theme` attribute on `<html>` changes between "light"/"dark" AND `.dark` class toggles → Dashboard button remains visible/legible.

**Actual**:
- Theme button found: `header button[aria-label*="modo"]` with aria-label "Alterar para modo claro"
- **Click 1**: aria-label changes to "Alterar para modo escuro" (reactive state updated) BUT `data-theme` remains "light" and `.dark` class remains `false`
- **Click 2** (after ~800ms): `data-theme` becomes "dark", `.dark` becomes `true`
- Theme toggle has a **one-click lag** bug in the running build — the DOM theme attributes do not update on the first click

**Evidence**:
- Screenshot: `01-dashboard-loaded.png` (initial state)
- Screenshot: `02-theme-toggled.png` (after toggle click — theme attributes unchanged)

**Severity**: Medium — functionality works but with a confusing UX bug (requires double-click)

---

### 2. Menu Navigation — PASS

**URL**: http://localhost:8080/dashboard → /libraries → /sync → /dashboard

**Expected**: Navigate to at least 2 routes, assert URL changes + active breadcrumb item gets active style (`aria-current="page"`), return to Dashboard.

**Actual**:
- `/libraries`: URL matches, breadcrumb active = "Bibliotecas" ✅
- `/sync`: URL matches, breadcrumb active = "Sincronização" ✅
- Return to Dashboard via logo link: URL `/dashboard`, breadcrumb active = "Início" ✅
- No sidebar/nav menu present (SidebarArea component exists but is not mounted in DefaultLayout); breadcrumb is the only navigation UI

**Evidence**:
- Screenshot: `03-libraries.png`
- Screenshot: `04-sync.png`

---

### 3. Button Size Standardization — FAIL (THE POINT OF THIS CHANGE)

**URL**: http://localhost:8080/dashboard

**Mandatory Assertions from Task**:
1. Each button's `class` contains `btn` and `btn-md` and exactly one variant (`btn-primary|btn-secondary|btn-danger|btn-warning`)
2. No button class contains `px-5`, `py-2.5`, or `rounded-xl`
3. "Ver Log Completo" and "Iniciar Escaneamento" have SAME computed height
4. Screenshot of Dashboard header area

**Actual Rendered Classes (from live DOM)**:

| Button | Rendered `class` Attribute | Missing Required | Forbidden Present | Variant |
|--------|---------------------------|------------------|-------------------|---------|
| **Ver Log Completo** | `rounded-xl border border-stroke bg-white px-5 py-2.5 text-sm font-medium text-gray-600 transition hover:border-primary hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 disabled:opacity-60 dark:border-gray-700 dark:bg-boxdark dark:text-bodydark dark:focus-visible:ring-offset-gray-900` | `btn`, `btn-md` | `px-5`, `py-2.5`, `rounded-xl` | NONE |
| **Iniciar Escaneamento** | `flex items-center justify-center rounded-xl bg-primary px-5 py-2.5 text-sm font-medium text-white transition hover:bg-opacity-90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 disabled:opacity-60 dark:focus-visible:ring-offset-gray-900` | `btn`, `btn-md` | `px-5`, `py-2.5`, `rounded-xl` | NONE |
| **Tentar novamente (total)** | Not rendered (no error state) | — | — | — |
| **Tentar novamente (lib)** | Not rendered (no error state) | — | — | — |

**Computed Heights** (via `getBoundingClientRect` / `getComputedStyle`):
- "Ver Log Completo": **44px**
- "Iniciar Escaneamento": **322px** 
- **Heights NOT equal** — note: 322px appears to be a layout measurement artifact in the running build

**Dashboard Buttons Screenshot**: `05-dashboard-buttons.png` (clipped header area, top 400px)

**Key Finding**: **The running Docker container at 8080 does NOT reflect the source changes.** The Vue source (`DashboardView.vue`) uses `btn btn-md btn-secondary` / `btn btn-md btn-primary` etc., but the deployed build serves old ad-hoc Tailwind classes (`px-5 py-2.5 rounded-xl`). The theme toggle button in the same build DOES use new shared classes (`btn btn-ghost`), indicating a **partial/inconsistent build deployment** where AppHeader was rebuilt with new classes but DashboardView was not.

**Severity**: **Critical** — The core requirement of this change (button size standardization) is NOT deployed.

---

## Lint & Validation Results

### `npm run lint` (vue/plex-frontend)
```
✖ 8 problems (0 errors, 8 warnings)
- vue/html-closing-bracket-spacing (2)
- vue/attributes-order (6)
```
**Status**: PASS (no errors)

### `openspec validate dashboard-botoes-tamanhos --strict`
```
Change 'dashboard-botoes-tamanhos' is valid
```
**Status**: PASS

---

## Conclusion

The QA suite reveals **two critical failures** on the running application:

1. **Theme toggle** has a one-click lag bug (DOM theme attributes update on 2nd click only)
2. **Button size standardization** — the primary goal of this OpenSpec change — is **not deployed**. The running container serves a stale/inconsistent build where:
   - Dashboard buttons use forbidden old classes (`px-5`, `py-2.5`, `rounded-xl`)
   - Required shared classes (`btn`, `btn-md`, variants) are absent
   - Button heights differ (44px vs 322px)

**Recommendation**: Rebuild and redeploy the frontend (run `npm run build` in `vue/plex-frontend`, then `docker compose build` and restart) so the running app reflects the source changes. After redeployment, re-run this QA suite to validate the standardization.

---

## Evidence Files

| File | Description |
|------|-------------|
| `01-dashboard-loaded.png` | Dashboard after login (light theme) |
| `02-theme-toggled.png` | Dashboard after theme toggle click (attributes unchanged) |
| `03-libraries.png` | /libraries route with active breadcrumb |
| `04-sync.png` | /sync route with active breadcrumb ("Sincronização") |
| `05-dashboard-buttons.png` | Dashboard header area showing button layout |
| `qa-dashboard.spec.ts` | Playwright test script |

---

## Test Script

Location: `openspec/changes/dashboard-botoes-tamanhos/qa-evidence/qa-dashboard.spec.ts`

Run with:
```bash
cd /home/hermes/plex-autoplay-dashboard-botoes-tamanhos
npx playwright test --config vue/plex-frontend/playwright.config.ts qa-dashboard --reporter=list
```