# Build .NET only. Vue assets pré-buildados em src/PlexAutoplay.Web/wwwroot (dist/).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PlexAutoplay.slnx ./
COPY src/PlexAutoplay.Domain/ ./src/PlexAutoplay.Domain/
COPY src/PlexAutoplay.Application/ ./src/PlexAutoplay.Application/
COPY src/PlexAutoplay.Infrastructure/ ./src/PlexAutoplay.Infrastructure/
COPY src/PlexAutoplay.Web/ ./src/PlexAutoplay.Web/
RUN dotnet publish src/PlexAutoplay.Web/PlexAutoplay.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
ARG GIT_COMMIT=local
LABEL org.opencontainers.image.revision=$GIT_COMMIT
RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80 \
    DISABLE_HTTP=false \
    WEB_PORT=80 \
    LOG_LEVEL=Information
ENTRYPOINT ["dotnet","PlexAutoplay.Web.dll"]
