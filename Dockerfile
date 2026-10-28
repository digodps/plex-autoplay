# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Install ffmpeg for marker detection and openssl for self-signed certificate generation
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg openssl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PlexAutoplay.slnx", "./"]
COPY ["src/PlexAutoplay.Domain/PlexAutoplay.Domain.csproj", "src/PlexAutoplay.Domain/"]
COPY ["src/PlexAutoplay.Application/PlexAutoplay.Application.csproj", "src/PlexAutoplay.Application/"]
COPY ["src/PlexAutoplay.Infrastructure/PlexAutoplay.Infrastructure.csproj", "src/PlexAutoplay.Infrastructure/"]
COPY ["src/PlexAutoplay.Web/PlexAutoplay.Web.csproj", "src/PlexAutoplay.Web/"]
RUN dotnet restore "PlexAutoplay.slnx"
COPY . .
WORKDIR "/src/src/PlexAutoplay.Web"
RUN dotnet build "PlexAutoplay.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlexAutoplay.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 443
EXPOSE 8081
ENV ASPNETCORE_URLS=https://+:443
ENTRYPOINT ["dotnet", "PlexAutoplay.Web.dll"]
