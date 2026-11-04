#!/usr/bin/env bash
# Build frontend + publish .NET + rebuild image + restart container.
set -euo pipefail
cd "$(dirname "$0")"

export PATH="/app/hermes-agent-src/tools/node-26.12.1-linux-x64/bin:$PATH"

[ -f docker-compose.override.yml ] && echo "AVISO: docker-compose.override.yml existe e pode duplicar a porta 8080"

echo "==> frontend"
cd vue/plex-frontend
[ -d node_modules ] || npm ci
npm run build
cd ../..

echo "==> wwwroot"
rm -rf src/PlexAutoplay.Web/wwwroot
cp -r vue/plex-frontend/dist src/PlexAutoplay.Web/wwwroot
echo "bundle: $(grep -o 'index-[A-Za-z0-9_-]*\.js' src/PlexAutoplay.Web/wwwroot/index.html | head -1)"

echo "==> docker build"
docker compose build

echo "==> restart"
docker rm -f plex-autoplay >/dev/null 2>&1 || true
docker compose up -d

echo "==> health"
for _ in $(seq 1 20); do
  code=$(curl -s -o /dev/null -w '%{http_code}' http://127.0.0.1:8080/settings || true)
  if [ "$code" = "200" ]; then echo "OK: http://192.168.18.5:8080/settings (200)"; exit 0; fi
  sleep 2
done
echo "FALHOU: /settings nao respondeu 200" >&2
docker logs plex-autoplay --tail 20 >&2
exit 1
