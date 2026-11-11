#!/usr/bin/env bash
#
# acceptance-test.sh - Teste de aceitação da stack real do Plex Autoplay.
#
# Sobe a aplicação com scripts/start.sh e exercita as funções do sistema uma a
# --- 8. Interface em navegador real (Playwright) ---
# Login, dashboard, configurações, bibliotecas/séries/editor e scan são
# exercitados como o usuário faz: Chromium real contra BASE_URL.
PW_LOG="$TMP/playwright.log"
if ! command -v npx >/dev/null 2>&1; then
  fail "Playwright — npx não encontrado; instale Node.js para testar a interface"
else
  (
    cd tests/e2e || exit 1
    if [[ ! -d node_modules ]]; then
      npm install --silent
    fi
    ACCEPTANCE_BASE_URL="$BASE_URL" npx playwright test --config=acceptance.config.cjs
  ) >"$PW_LOG" 2>&1
  PW_STATUS=$?

  if grep -qi "executable doesn't exist" "$PW_LOG"; then
    fail "Playwright — navegador Chromium ausente; rode: (cd tests/e2e && npx playwright install chromium)"
  elif [[ "$PW_STATUS" -ne 0 ]] && ! grep -qE '^\s+(✓|✘|✗)' "$PW_LOG"; then
    tail -n 20 "$PW_LOG" >&2
    fail "Playwright — execução da suíte de navegador falhou"
  else
    while IFS= read -r line; do
      name="${line#"${line%%[![:space:]]*}"}"
      case "$name" in
        ✓*) pass "UI ${name#✓ }" ;;
        ✘*|✗*) fail "UI ${name#??}" ;;
        -*) skip "UI ${name#- }" ;;
      esac
    done < <(grep -E '^\s+(✓|✘|✗|-)' "$PW_LOG")
  fi

  if [[ -d tests/e2e/test-results ]]; then
    echo "  (artefatos do navegador: tests/e2e/test-results)"
  fi
fi
