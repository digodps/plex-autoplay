#!/usr/bin/env bash
# Deploy do branch atual do projeto: git update + frontend + solução .NET + imagem Docker + health.
set -euo pipefail

ROOT=$(git rev-parse --show-toplevel 2>/dev/null) || {
  echo "ERRO: diretório atual não pertence a um repositório git." >&2
  exit 1
}
cd "$ROOT"
echo "==> projeto $ROOT"

BRANCH=$(git rev-parse --abbrev-ref HEAD)

DIRTY=$(git status --porcelain --untracked-files=no -- . ':(exclude)src/PlexAutoplay.Web/wwwroot')
if [ -n "$DIRTY" ] && [ "${ALLOW_DIRTY:-0}" != "1" ]; then
  echo "ERRO: working tree tem alterações rastreadas não commitadas em '$BRANCH'." >&2
  echo "Commit antes do deploy ou rode com ALLOW_DIRTY=1 para publicar o estado local." >&2
  printf '%s\n' "$DIRTY" >&2
  exit 1
fi

echo "==> git update"
git fetch --prune origin
UPSTREAM=$(git rev-parse --abbrev-ref --symbolic-full-name '@{u}' 2>/dev/null || true)
if [ -n "$UPSTREAM" ]; then
  git merge --ff-only "$UPSTREAM"
else
  echo "AVISO: branch '$BRANCH' sem upstream; usando estado local."
fi

BRANCH=$(git rev-parse --abbrev-ref HEAD)
GIT_COMMIT=$(git rev-parse HEAD)
COMMIT=$(git rev-parse --short HEAD)

echo "==> branch $BRANCH @ $COMMIT"
export GIT_COMMIT

export PATH="/app/hermes-agent-src/tools/node-26.12.1-linux-x64/bin:$PATH"

if [ -f docker-compose.override.yml ]; then
  echo "AVISO: docker-compose.override.yml existe e pode duplicar a porta 8080"
fi

echo "==> frontend"
cd vue/plex-frontend
npm ci
npm run build
cd ../..

echo "==> wwwroot"
rm -rf src/PlexAutoplay.Web/wwwroot
cp -r vue/plex-frontend/dist src/PlexAutoplay.Web/wwwroot
echo "bundle: $(grep -o 'index-[A-Za-z0-9_-]*\.js' src/PlexAutoplay.Web/wwwroot/index.html | head -1)"

echo "==> solução"
dotnet build PlexAutoplay.slnx -c Release --nologo

echo "==> docker build"
docker compose build

IMAGE_REV=$(docker inspect --format '{{ index .Config.Labels "org.opencontainers.image.revision" }}' plex-autoplay:latest)
if [ "$IMAGE_REV" != "$GIT_COMMIT" ]; then
  echo "ERRO: imagem publicada com revisão '$IMAGE_REV', esperado '$GIT_COMMIT'." >&2
  exit 1
fi
echo "imagem: plex-autoplay:latest @ ${IMAGE_REV:0:7}"

echo "==> restart"
docker compose down --remove-orphans
docker compose up -d --no-build
docker ps --filter name=plex-autoplay --format 'container: {{.Names}} {{.Status}}'

echo "==> health"
HEALTH_URL=${DEPLOY_HEALTH_URL:-http://127.0.0.1:8080/settings}
for _ in $(seq 1 20); do
  code=$(curl -sL -o /dev/null -w '%{http_code}' "$HEALTH_URL" || true)
  if [ "$code" = "200" ]; then
    echo "OK: $HEALTH_URL (200)"
    PORT=$(docker port plex-autoplay 8081/tcp 2>/dev/null | head -1 | sed 's/.*://')
    IP=$(ip -4 route get 1.1.1.1 2>/dev/null | awk '{for (i = 1; i < NF; i++) if ($i == "src") { print $(i + 1); exit }}')
    [ -n "$IP" ] || IP=$(hostname -I 2>/dev/null | awk '{print $1}')
    echo "URL: http://${IP:-127.0.0.1}:${PORT:-8080}"
    exit 0
  fi
  sleep 2
done
echo "FALHOU: $HEALTH_URL não respondeu 200" >&2
docker logs plex-autoplay --tail 20 >&2
exit 1
