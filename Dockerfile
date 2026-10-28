FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg openssl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PlexAutoplay.slnx .
COPY src/PlexAutoplay.Domain/ ./src/PlexAutoplay.Domain/
COPY src/PlexAutoplay.Application/ ./src/PlexAutoplay.Application/
COPY src/PlexAutoplay.Infrastructure/ ./src/PlexAutoplay.Infrastructure/
COPY src/PlexAutoplay.Web/ ./src/PlexAutoplay.Web/
COPY tests/unit/ ./tests/unit/
RUN dotnet restore PlexAutoplay.slnx
RUN dotnet publish src/PlexAutoplay.Web/PlexAutoplay.Web.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "PlexAutoplay.Web.dll"]