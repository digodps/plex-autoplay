# --------------------------------------------------------------
# Dockerfile (multi-stage not needed – single stage with supervisor)
# --------------------------------------------------------------
FROM python:3.12-slim AS base

# ---------- build args ----------
ARG PLEX_TOKEN
ARG PLEX_URL

# ---------- system dependencies ----------
RUN apt-get update && apt-get install -y --no-install-recommends \
        ffmpeg \
        redis-server \
        supervisor \
        git \
    && rm -rf /var/lib/apt/lists/*

# ---------- non‑root user ----------
RUN useradd -m -u 1000 appuser
WORKDIR /app
COPY --chown=appuser:appuser requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# ---------- application code ----------
COPY --chown=appuser:appuser . .

# ---------- supervisord config ----------
COPY --chown=appuser:appuser supervisord.conf /etc/supervisor/conf.d/supervisord.conf

# ---------- environment ----------
ENV PYTHONUNBUFFERED=1 \
    REDIS_URL=redis://127.0.0.1:6379/0 \
    PLEX_TOKEN=${PLEX_TOKEN} \
    PLEX_URL=${PLEX_URL:-http://host.docker.internal:32400}

# Run as root so supervisord can spawn child processes as different users (redis as appuser, app as root)
# USER appuser
EXPOSE 8080

# ---------- entrypoint ----------
CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]