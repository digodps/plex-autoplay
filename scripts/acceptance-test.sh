#!/usr/bin/env bash
#
# acceptance-test.sh - Teste de aceitação da stack real do Plex Autoplay.
#
# Sobe a aplicação com scripts/start.sh e exercita as funções do sistema uma a
# uma. Funções de interface são exercitadas por navegador real (Playwright);
# contratos sem UI usam curl. Qualquer erro de start, conexão, status HTTP,
# contrato ou passo do navegador reprova a tarefa.
#
# Uso: ./scripts/acceptance-test.sh
# Variáveis opcionais:
#   ACCEPTANCE_BASE_URL  (padrão: https://localhost:8443)
#   ACCEPTANCE_WS_URL    (padrão: https://localhost:8081/ws)
#   ACCEPTANCE_TIMEOUT   timeout por requisição em segundos (padrão: 30)
#
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$PROJECT_ROOT"

BASE_URL="${ACCEPTANCE_BASE_URL:-https://localhost:8443}"
WS_URL="${ACCEPTANCE_WS_URL:-https://localhost:8081/ws}"
HTTP_TIMEOUT="${ACCEPTANCE_TIMEOUT:-30}"

TMP="$(mktemp -d)"
BODY="$TMP/body"
HEADERS="$TMP/headers"
CURL_ERR="$TMP/curl.err"
TOKEN=""
RATING_KEY=""

PASS=0
SKIP=0
FAILED=0

pass() { printf 'PASS  %s\n' "$*"; PASS=$((PASS + 1)); }
skip() { printf 'SKIP  %s\n' "$*"; SKIP=$((SKIP + 1)); }
fail() { printf 'FAIL  %s\n' "$*" >&2; FAILED=$((FAILED + 1)); }

cleanup() {
  if [[ -n "$TOKEN" && -n "$RATING_KEY" ]]; then
    curl -sk -o /dev/null -X DELETE \
      -H "Authorization: Bearer $TOKEN" \
      "$BASE_URL/api/markers/$RATING_KEY/intro" 2>/dev/null || true
  fi
  rm -rf "$TMP"
}
trap cleanup EXIT

json_field() {
  python3 - "$BODY" "$1" <<'PY' 2>/dev/null
import json, sys
value = json.load(open(sys.argv[1]))
for part in sys.argv[2].split("."):
    value = value[int(part)] if isinstance(value, list) else value[part]
print(value)
PY
}

json_array_size() {
  python3 - "$BODY" <<'PY' 2>/dev/null
import json, sys
data = json.load(open(sys.argv[1]))
print(len(data) if isinstance(data, list) else -1)
PY
}

request() {
  local method="$1" path="$2" expected="$3"
  shift 3
  local code
  code=$(curl -skS --max-time "$HTTP_TIMEOUT" -D "$HEADERS" -o "$BODY" \
    -w '%{http_code}' -X "$method" "$@" "$BASE_URL$path" 2>"$CURL_ERR") || {
    fail "$method $path — erro de conexão: $(tr '\n' ' ' <"$CURL_ERR")"
    return 1
  }
  if [[ "$code" != "$expected" ]]; then
    fail "$method $path — esperado HTTP $expected, recebido $code: $(head -c 300 "$BODY")"
    return 1
  fi
  return 0
}

assert_field() {
  local description="$1" field="$2" expected="$3" actual
  actual="$(json_field "$field")"
  if [[ "$actual" == "$expected" ]]; then
    pass "$description ($field=$expected)"
  else
    fail "$description — $field esperado '$expected', obtido '$actual'"
  fi
}

report() {
  echo
  echo "PASS=$PASS SKIP=$SKIP FAIL=$FAILED"
  if ((FAILED > 0)); then
    echo "RESULTADO: REPROVADO"
    return 1
  fi
  echo "RESULTADO: APROVADO"
  return 0
}

# --- 1. Pré-condições e start ---
if [[ ! -f .env ]]; then
  fail "start.sh — arquivo .env não encontrado"
  report
  exit 1
fi

set -a
# shellcheck disable=SC1091
. ./.env
set +a

ADMIN_PASSWORD="${ADMIN_PASSWORD:-admin}"

echo "=============================================="
echo "  Teste de aceitação — Plex Autoplay"
echo "=============================================="

if ./scripts/start.sh >"$TMP/start.log" 2>&1; then
  pass "start.sh — aplicação iniciada e respondendo"
else
  tail -n 40 "$TMP/start.log" >&2
  fail "start.sh — falha ao iniciar a aplicação"
  report
  exit 1
fi

# --- 2. Autenticação ---
request GET /auth/login 200 && pass "GET /auth/login"
request GET /api/status 401 && pass "GET /api/status sem token retorna 401"

request POST /auth/login 401 \
  -H 'Content-Type: application/json' \
  --data '{"password":"__acceptance_wrong_password__"}' \
  && pass "POST /auth/login com senha incorreta retorna 401"

python3 -c 'import json,os; print(json.dumps({"password": os.environ["ADMIN_PASSWORD"]}))' >"$TMP/login.req"
if request POST /auth/login 200 \
  -c "$TMP/cookies" \
  -H 'Content-Type: application/json' \
  --data-binary "@$TMP/login.req"; then
  TOKEN="$(json_field token)"
  if [[ -z "$TOKEN" ]]; then
    fail "POST /auth/login — token JWT ausente"
  else
    pass "POST /auth/login com senha correta retorna token JWT"
  fi
  if grep -q 'auth_token' "$TMP/cookies"; then
    pass "deploy do cookie auth_token"
  else
    fail "deploy do cookie auth_token — cookie ausente"
  fi
fi

if [[ -z "$TOKEN" ]]; then
  fail "autenticação — sem token JWT, demais testes abortados"
  report
  exit 1
fi

AUTH=(-H "Authorization: Bearer $TOKEN")

# --- 3. Monitoramento ---
if request GET /api/status 200 "${AUTH[@]}"; then
  assert_field "GET /api/status" service "plex-autoplay"
fi

if request GET /metrics 200 "${AUTH[@]}"; then
  if grep -q '^plex_autoplay_info' "$BODY"; then
    pass "GET /metrics — métricas Prometheus presentes"
  else
    fail "GET /metrics — métrica plex_autoplay_info ausente"
  fi
fi

if request GET /api/observability/summary 200 "${AUTH[@]}"; then
  assert_field "GET /api/observability/summary" service "plex-autoplay"
fi

# --- 4. Configuração e tema ---
if request GET /config 200 "${AUTH[@]}"; then
  assert_field "GET /config" note "Configuration is read exclusively from environment variables. app-config.json is not used."
fi

request POST /config 200 "${AUTH[@]}" && pass "POST /config — validação de configuração"

if request GET /api/theme 200 "${AUTH[@]}"; then
  if json_field mode >/dev/null 2>&1; then
    pass "GET /api/theme"
  else
    fail "GET /api/theme — corpo sem campo mode"
  fi
fi

request PUT /api/theme 204 "${AUTH[@]}" \
  -H 'Content-Type: application/json' \
  --data '{"AccentColor":"#e5a00d","Mode":"dark","UserId":"admin"}' \
  && pass "PUT /api/theme válido retorna 204"

request PUT /api/theme 400 "${AUTH[@]}" \
  -H 'Content-Type: application/json' \
  --data '{"AccentColor":"#000000","Mode":"invalid","UserId":"admin"}' \
  && pass "PUT /api/theme inválido retorna 400"

# --- 5. CRUD de marcadores ---
RATING_KEY="acceptance-$(date +%s)-$$"

if request GET /api/markers 200 "${AUTH[@]}"; then
  if [[ "$(json_array_size)" =~ ^[0-9]+$ ]]; then
    pass "GET /api/markers — lista retornada"
  else
    fail "GET /api/markers — resposta não é uma lista"
  fi
fi

if request POST "/api/markers?RatingKey=$RATING_KEY&Kind=intro&StartMs=1000&EndMs=90000" 201 "${AUTH[@]}"; then
  pass "POST /api/markers — marcador criado"
  request POST "/api/markers?RatingKey=$RATING_KEY&Kind=intro&StartMs=1000&EndMs=90000" 409 "${AUTH[@]}" \
    && pass "POST /api/markers duplicado retorna 409"
fi

if request GET "/api/markers/$RATING_KEY" 200 "${AUTH[@]}"; then
  assert_field "GET /api/markers/$RATING_KEY" 0.kind "intro"
fi

if request PUT "/api/markers/$RATING_KEY/intro" 200 "${AUTH[@]}" \
  -H 'Content-Type: application/json' \
  --data '{"StartMs":2000,"EndMs":95000}'; then
  assert_field "PUT /api/markers/$RATING_KEY/intro" startMs "2000"
fi

request PUT "/api/markers/acceptance-missing/intro" 404 "${AUTH[@]}" \
  -H 'Content-Type: application/json' \
  --data '{"StartMs":1,"EndMs":2}' \
  && pass "PUT em marcador inexistente retorna 404"

request DELETE "/api/markers/$RATING_KEY/intro" 200 "${AUTH[@]}" \
  && pass "DELETE /api/markers/$RATING_KEY/intro"

if request GET "/api/markers/$RATING_KEY" 200 "${AUTH[@]}"; then
  if [[ "$(json_array_size)" == "0" ]]; then
    pass "marcador removido — lista vazia"
  else
    fail "marcador removido — ainda presente na listagem"
  fi
fi

# --- 6. Integração com Plex ---
LIB_KEYS=()
SERIES_KEY=""
SERIES_THUMB=""

if request GET /api/plex/libraries 200 "${AUTH[@]}"; then
  mapfile -t LIB_KEYS < <(python3 - "$BODY" <<'PY' 2>/dev/null
import json, sys
for item in json.load(open(sys.argv[1])):
    print(item.get("key", ""))
PY
)
  LIB_KEYS=("${LIB_KEYS[@]//[[:space:]]/}")
  LIB_KEYS=("${LIB_KEYS[@]:0:${#LIB_KEYS[@]}}")
  if (( ${#LIB_KEYS[@]} == 0 )); then
    skip "Plex sem bibliotecas de séries — séries/episódios/imagens não testados"
  else
    pass "GET /api/plex/libraries — ${#LIB_KEYS[@]} biblioteca(s)"
    LIB="${LIB_KEYS[0]}"

    request GET "/library/$LIB" 200 "${AUTH[@]}" \
      && pass "GET /library/$LIB — episódios da biblioteca"

    if request GET "/api/series/$LIB" 200 "${AUTH[@]}"; then
      pass "GET /api/series/$LIB — catálogo de séries"
      read -r SERIES_KEY SERIES_THUMB < <(python3 - "$BODY" <<'PY' 2>/dev/null
import json, sys
items = json.load(open(sys.argv[1])).get("items", [])
print((items[0].get("ratingKey") or "-") if items else "-", (items[0].get("thumb") or "-") if items else "-")
PY
)
      if [[ -z "$SERIES_KEY" || "$SERIES_KEY" == "-" ]]; then
        skip "biblioteca sem séries — episódios por série/imagem não testados"
      else
        request GET "/api/series/$LIB/$SERIES_KEY/episodes" 200 "${AUTH[@]}" \
          && pass "GET /api/series/$LIB/$SERIES_KEY/episodes"

        if [[ -n "$SERIES_THUMB" && "$SERIES_THUMB" != "-" ]]; then
          if request GET "$SERIES_THUMB" 200 "${AUTH[@]}"; then
            if grep -qi '^content-type: image/' "$HEADERS"; then
              pass "GET $SERIES_THUMB — proxy de imagem"
            else
              fail "GET $SERIES_THUMB — content-type não é imagem"
            fi
          fi
        else
          skip "série sem thumb — proxy de imagem não testado"
        fi
      fi
    fi
  fi
fi

# --- 7. Scan e acompanhamento de job ---
if request POST /api/plex/scan 200 "${AUTH[@]}"; then
  JOB_ID="$(json_field job_id)"
  if [[ -z "$JOB_ID" ]]; then
    fail "POST /api/plex/scan — job_id ausente"
  else
    pass "POST /api/plex/scan — job iniciado"

    if request GET "/api/sync/status/$JOB_ID" 200 "${AUTH[@]}"; then
      status="$(json_field status)"
      errors="$(json_field errors)"
      if [[ "$status" == "failed" ]]; then
        fail "GET /api/sync/status/$JOB_ID — job falhou: $(json_field error)"
      elif [[ "${errors:-0}" != "0" ]]; then
        fail "GET /api/sync/status/$JOB_ID — job com $errors erro(s)"
      else
        pass "GET /api/sync/status/$JOB_ID ($status)"
      fi
    fi

    curl -skN --max-time 5 "${AUTH[@]}" \
      "$BASE_URL/api/sync/log/stream/$JOB_ID" -o "$TMP/sse" 2>/dev/null || true
    if grep -q '^data:' "$TMP/sse"; then
      pass "GET /api/sync/log/stream/$JOB_ID — SSE respondeu"
    else
      fail "GET /api/sync/log/stream/$JOB_ID — SSE sem evento data:"
    fi

    code=$(curl -sk --max-time "$HTTP_TIMEOUT" -o "$BODY" -w '%{http_code}' \
      -X POST "${AUTH[@]}" "$BASE_URL/api/sync/cancel/$JOB_ID" 2>/dev/null || echo 000)
    case "$code" in
      200)
        pass "POST /api/sync/cancel/$JOB_ID — job cancelado"
        ;;
      404)
        # Job terminou antes do cancelamento: aceitável, desde que não tenha falhado.
        if request GET "/api/sync/status/$JOB_ID" 200 "${AUTH[@]}"; then
          status="$(json_field status)"
          errors="$(json_field errors)"
          if [[ "$status" == "failed" ]]; then
            fail "POST /api/sync/cancel/$JOB_ID — job terminou com failed: $(json_field error)"
          elif [[ "${errors:-0}" != "0" ]]; then
            fail "POST /api/sync/cancel/$JOB_ID — job terminou com $errors erro(s)"
          else
            pass "POST /api/sync/cancel/$JOB_ID — job já finalizado ($status)"
          fi
        fi
        ;;
      *)
        fail "POST /api/sync/cancel/$JOB_ID — esperado 200 ou 404, recebido $code"
        ;;
    esac
  fi
fi

request GET /api/sync/status/acceptance-unknown 404 "${AUTH[@]}" \
  && pass "GET /api/sync/status desconhecido retorna 404"
request POST /api/sync/cancel/acceptance-unknown 404 "${AUTH[@]}" \
  && pass "POST /api/sync/cancel desconhecido retorna 404"

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
    ACCEPTANCE_BASE_URL="$BASE_URL" npx playwright test --config=acceptance.config.js
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

# --- 9. WebSocket do PlexAutoSkip ---
curl -sk --http1.1 --max-time 3 -D "$TMP/ws.headers" -o /dev/null \
  -H 'Connection: Upgrade' \
  -H 'Upgrade: websocket' \
  -H 'Sec-WebSocket-Version: 13' \
  -H 'Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==' \
  "$WS_URL" 2>/dev/null || true
if grep -q '101' "$TMP/ws.headers"; then
  pass "WebSocket $WS_URL — handshake 101"
else
  fail "WebSocket $WS_URL — handshake não retornou 101"
fi

# --- 10. Resultado ---
if ! report; then
  exit 1
fi
