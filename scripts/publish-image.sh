#!/usr/bin/env bash
#
# publish-image.sh - cria tag semver (patch por padrão) e envia branch + tag.
# O push da tag dispara .github/workflows/publish-container.yml, que publica
# a imagem no GHCR com as tags X.Y.Z, X.Y e latest.
#
# Uso: ./scripts/publish-image.sh [--patch|--minor|--major] [--dry-run] [--help]
#
# Exit codes: 1 erro git/push, 2 uso inválido, 3 tag já existe, 4 sem remote origin.
set -euo pipefail

usage() {
  cat <<'EOF'
Uso: scripts/publish-image.sh [--patch|--minor|--major] [--dry-run] [--help]

  --patch   incrementa Z (padrão)
  --minor   incrementa Y e zera Z
  --major   incrementa X e zera Y.Z
  --dry-run mostra a versão calculada e os comandos, sem criar tag nem push
  --help    exibe esta ajuda

A tag git vX.Y.Z é a única fonte de versão. A imagem publicada recebe
X.Y.Z, X.Y e latest (latest apenas em push de tag).
EOF
}

die() {
  local msg="$1" code="${2:-1}"
  echo "erro: $msg" >&2
  exit "$code"
}

current_version() {
  git tag --list 'v*' |
    grep -E '^v[0-9]+\.[0-9]+\.[0-9]+$' |
    sed 's/^v//' |
    sort -V |
    tail -n1
}

bump_version() { # $1=versão atual (vazia se não houver), $2=patch|minor|major
  local current="$1" kind="$2" major=0 minor=0 patch=0

  if [[ -z "$current" ]]; then
    [[ "$kind" == "major" ]] && echo "1.0.0" || echo "0.1.0"
    return
  fi

  IFS=. read -r major minor patch <<<"$current"
  case "$kind" in
    major) echo "$((major + 1)).0.0" ;;
    minor) echo "$major.$((minor + 1)).0" ;;
    patch) echo "$major.$minor.$((patch + 1))" ;;
  esac
}

bump=""
dry_run=false
while [[ $# -gt 0 ]]; do
  case "$1" in
    --patch | --minor | --major)
      [[ -n "$bump" && "$bump" != "${1#--}" ]] && die "flags de bump conflitantes" 2
      bump="${1#--}"
      ;;
    --dry-run) dry_run=true ;;
    --help | -h)
      usage
      exit 0
      ;;
    *)
      usage >&2
      die "opção desconhecida: $1" 2
      ;;
  esac
  shift
done
bump="${bump:-patch}"

git rev-parse --git-dir >/dev/null 2>&1 || die "não é um repositório git"
git remote get-url origin >/dev/null 2>&1 || die "remote 'origin' não configurado" 4

branch="$(git symbolic-ref -q --short HEAD)" || die "HEAD detached; use um branch"
git fetch --tags --quiet origin

current="$(current_version || true)"
version="$(bump_version "$current" "$bump")"
tag="v$version"
[[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || die "versão inválida calculada: $version"

if $dry_run; then
  echo "versão atual: $(current_version || true)"
  echo "bump: $bump"
  echo "tag: $tag"
  echo "comandos: git tag -a $tag -m 'release $tag'; git push origin $branch; git push origin $tag"
  exit 0
fi

[[ -n "$(git status --porcelain)" ]] && die "working tree sujo; commite ou faça stash antes de publicar"

if git rev-parse -q --verify "refs/tags/$tag" >/dev/null ||
  git ls-remote --tags --exit-code origin "refs/tags/$tag" >/dev/null 2>&1; then
  die "tag $tag já existe" 3
fi

git tag -a "$tag" -m "release $tag"
if ! git push origin "$branch"; then
  git tag -d "$tag" >/dev/null
  die "push de $branch falhou; tag local $tag removida"
fi
git push origin "$tag" || die "push da tag $tag falhou (tag local mantida)"

echo "✅ $tag publicado. Workflow publish-container.yml vai publicar ghcr.io/<owner>/<repo>:$version, :${version%.*}, :latest"
