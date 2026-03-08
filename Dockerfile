# -----------------------------------------------------------------------
# QuickReserve API - Multi-stage Dockerfile (Production Ready)
# -----------------------------------------------------------------------

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (better layer caching)
COPY QuickReserve.sln .
COPY Directory.Build.props .
COPY global.json .
COPY src/QuickReserve.Domain/QuickReserve.Domain.csproj src/QuickReserve.Domain/
COPY src/QuickReserve.Application/QuickReserve.Application.csproj src/QuickReserve.Application/
COPY src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj src/QuickReserve.Infrastructure/
COPY src/QuickReserve.API/QuickReserve.API.csproj src/QuickReserve.API/
COPY tests/QuickReserve.Tests/QuickReserve.Tests.csproj tests/QuickReserve.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else and publish
COPY . .
WORKDIR /src/src/QuickReserve.API
RUN dotnet publish -c Release --no-restore -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install wget for healthcheck (curl not available in aspnet image)
USER root
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*

# Security: run as non-root user
RUN useradd --no-create-home --shell /bin/false appuser
USER appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=30s \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/api/appointments || exit 1

ENTRYPOINT ["dotnet", "QuickReserve.API.dll"]
