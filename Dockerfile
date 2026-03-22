# ══════════════════════════════════════════════════════
# Multi-stage build: SDK para compilar, runtime minimal para producción
# ══════════════════════════════════════════════════════

# ── Build stage ──
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar cache de capas en restore
COPY Asambleas/Asambleas.csproj Asambleas/
RUN dotnet restore Asambleas/Asambleas.csproj

# Copiar todo el código y compilar en modo Release
COPY . .
WORKDIR /src/Asambleas
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ──
# Usar imagen chiseled (sin shell, sin package manager, sin root user)
# Es la imagen más segura disponible: superficie de ataque mínima
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app

# Copiar artefactos compilados (sin SDK, sin código fuente, sin herramientas de debug)
COPY --from=build /app/publish .

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# La imagen chiseled ya corre como non-root (user 'app', UID 1654)
# No es necesario crear usuario ni usar USER explícitamente

# Solo exponer el puerto necesario
EXPOSE 8080

# Healthcheck: la imagen chiseled no tiene curl/wget, así que no se puede hacer
# un health check HTTP nativo desde Docker. Usar el healthcheck desde
# docker-compose.yml o el orquestador (Kubernetes liveness probe, etc.)
# El endpoint /health/live está disponible en la app ASP.NET.
HEALTHCHECK NONE

ENTRYPOINT ["dotnet", "Asambleas.dll"]
