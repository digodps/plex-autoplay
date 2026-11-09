# QA Report 3 — Dashboard Button Size Standardization

**Change:** `dashboard-botoes-tamanhos`
**App:** http://localhost:8080 (container `plex-autoplay`, rev e84d9e7)
**Tool:** Playwright `npx playwright test openspec/changes/dashboard-botoes-tamanhos/qa-evidence/qa-dashboard.spec.ts`
**Evidence:** `openspec/changes/dashboard-botoes-tamanhos/qa-evidence/` (`.spec.ts` + `.png` screenshots)

---

## Criterion 1 — Theme toggle

**Requirement:** `<html>` gets/removes `.dark` class on theme toggle; button legible in both themes.

| Check | Result | Evidence |
|---|---|---|
| `.dark` class present before toggle | PASS | `01-dashboard-loaded.png` |
| `.dark` class toggled on click | PASS | `02-theme-toggled.png` |
| `.dark` class removed on toggle back | PASS | `02-theme-toggled.png` |
| Button legible in toggled theme | PASS | `02-theme-toggled.png` |

**Status:** **PASS**

---

## Criterion 2 — Menus

**Requirement:** Navigate to `/libraries`, back to `/dashboard` via breadcrumb/navigate; assert URL + active highlight.

| Check | Result | Evidence |
|---|---|---|
| URL contains `/libraries` | PASS | `03-libraries.png` |
| `aria-current="page"` on "Bibliotecas" | PASS | `03-libraries.png` |
| URL contains `/dashboard` after return | PASS | `01-dashboard-loaded.png` |
| Active highlight present | PASS | `03-libraries.png` |

**Status:** **PASS**

---

## Criterion 3 — Button size standardization (CORE)

**Requirement:** On Dashboard the 4 buttons each contain `btn` + `btn-md` + exactly one variant; no `px-5`, `py-2.5`, `rounded-xl`; "Ver Log Completo" and "Iniciar Escaneamento" computed heights EQUAL.

| Button | btn | btn-md | exactly-1 variant | no bad classes | Height |
|---|---|---|---|---|---|
| Ver Log Completo | ✓ | ✓ | ✓ (`btn-secondary`) | ✓ | **40 px** |
| Iniciar Escaneamento | ✓ | ✓ | ✓ (`btn-primary`) | ✓ | **40 px** |
| Tentar novamente (total) | N/A (not rendered) | — | — | — | N/A |
| Tentar novamente (lib) | N/A (not rendered) | — | — | — | N/A |

Height equality: **PASS** (Ver Log Completo = **40 px** vs Iniciar Escaneamento = **40 px**)

**Status:** **PASS**

---

## Lint

`npm run lint` → **PASS** (0 errors, 8 pre-existing warnings)

## OpenSpec validate

`openspec validate dashboard-botoes-tamanhos --strict` → **PASS**

---

## Overall

**Overall PASS/FAIL:** **PASS**
**Equal heights:** **40 px**