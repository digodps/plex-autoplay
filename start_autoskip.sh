#!/bin/bash
set -e

CONFIG_DIR="/config/plexautoskip"
CONFIG_FILE="${CONFIG_DIR}/config.ini"

mkdir -p "${CONFIG_DIR}"

# Derive address and port from PLEX_URL (e.g., http://192.168.18.3:32400)
PLEX_URL="${PLEX_URL:-http://127.0.0.1:32400}"
# strip scheme
URL_NO_SCHEME="${PLEX_URL#*://}"
ADDRESS="${URL_NO_SCHEME%%:*}"
PORT="${URL_NO_SCHEME##*:}"
if [ "${PORT}" = "${ADDRESS}" ]; then
  PORT=32400
fi
SSL="False"
TOKEN="${PLEX_TOKEN:-}"

# Always (re)generate config.ini
cat > "${CONFIG_FILE}" <<EOF
[PlexServer]
plex_url = ${PLEX_URL}
plex_token = ${TOKEN}

[General]
log_level = INFO

[Custom]
custom_file = ${CONFIG_DIR}/custom.json

[Plex.tv]
username = 
password = 
token = ${TOKEN}
servername = 

[Server]
address = ${ADDRESS}
ssl = ${SSL}
port = ${PORT}

[Security]
ignore-certs = False

[Skip]
mode = skip
tags = intro, commercial, advertisement, credits
types = movie, episode
ignored-libraries = 
last-chapter = 0.0
unwatched = False
first-episode-series = Always
first-episode-season = Always
first-safe-tags = 
last-episode-series = Watched
last-episode-season = Always
last-safe-tags = 
next = False

[Binge]
ignore-skip-for = 0
safe-tags = 
same-show-only = False
skip-next-max = 0

[Offsets]
start = 0
end = 1000
command = 500
tags = intro

[Volume]
low = 0
high = 100
EOF

# Execute PlexAutoSkip main.py
exec python /opt/plexautoskip/main.py --config "${CONFIG_FILE}"