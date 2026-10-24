FROM python:3.11-slim

# Evita criação de arquivos .pyc e ativa buffer de logs sem atraso
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1

WORKDIR /app

ENV CONFIG_DIR=/config \
    MEDIA_ROOT=/media-root \
    WEB_PORT=8080

VOLUME ["/config"]
EXPOSE 8080

# Instala curl para healthcheck se necessário
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    ffmpeg \
    openssl \
    && rm -rf /var/lib/apt/lists/*

# Instala dependências Python
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copia código fonte do projeto
COPY plex_autoplay/ ./plex_autoplay/
COPY markers.example.json .

# Ponto de entrada
CMD ["python", "-m", "plex_autoplay"]
