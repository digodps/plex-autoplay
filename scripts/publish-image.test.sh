#!/usr/bin/env bash
# Teste assert-based do cálculo de versão e do --dry-run de publish-image.sh.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SCRIPT="$ROOT/scripts/publish-image.sh"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

git init -q --bare "$TMP/origin.git"
git clone -q "$TMP/origin.git" "$TMP/repo" 2>/dev/null
cd "$TMP/repo"
git config user.email test@example.com
git config user.name Test
echo x >file
git add file
git commit -qm init
git branch -M main
git push -q origin main

dry() { bash "$SCRIPT" --dry-run "$@"; }
has() { grep -qx -- "$2" <<<"$1" || { echo "FALHOU: esperado '$2' em: $1" >&2; exit 1; }; }

has "$(dry)" "tag: v0.1.0"
git tag -a v1.4.2 -m "release v1.4.2"
git push -q origin v1.4.2

has "$(dry)" "tag: v1.4.3"
has "$(dry --patch)" "tag: v1.4.3"
has "$(dry --minor)" "tag: v1.5.0"
has "$(dry --major)" "tag: v2.0.0"

bash "$SCRIPT" --help | grep -q "Uso:"

if bash "$SCRIPT" --foo >/dev/null 2>&1; then
  echo "FALHOU: flag inválida aceita" >&2
  exit 1
fi
if bash "$SCRIPT" --minor --major >/dev/null 2>&1; then
  echo "FALHOU: flags conflitantes aceitas" >&2
  exit 1
fi

bash "$SCRIPT" >/dev/null
git tag --list | grep -qx v1.4.3
has "$(dry)" "tag: v1.4.4"

touch untracked.txt
if bash "$SCRIPT" >/dev/null 2>&1; then
  echo "FALHOU: publicou com working tree sujo" >&2
  exit 1
fi
rm untracked.txt

git init -q "$TMP/noorigin"
cd "$TMP/noorigin"
rc=0
bash "$SCRIPT" --dry-run >/dev/null 2>&1 || rc=$?
[[ "$rc" -eq 4 ]] || { echo "FALHOU: sem remote retornou $rc (esperado 4)" >&2; exit 1; }

msg="$(bash "$SCRIPT" --foo 2>&1 || true)"
[[ "$msg" == *desconhecida* && "$msg" != *": 2" ]] || { echo "FALHOU: mensagem de erro: $msg" >&2; exit 1; }

echo "ok"
