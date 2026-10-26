#!/usr/bin/env bash
# --------------------------------------------------------------
# logs.sh  –  follow the live output of the stack (app + redis)
# --------------------------------------------------------------
cd "$(dirname "$0")"
docker compose logs -f
