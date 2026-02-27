# -----------------------------------------------------------------------
# QuickReserve API - Multi-stage Dockerfile
# -----------------------------------------------------------------------

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (better layer caching)
COPY QuickReserve.sln .
COPY Directory.Build.props .
COPY src/QuickReserve.Domain/QuickReserve.Domain.csproj src/QuickReserve.Domain/
COPY src/QuickReserve.Application/QuickReserve.Application.csproj src/QuickReserve.Application/
COPY src/QuickReserve.Infrastructure/QuickReserve.Infrastructure.csproj src/QuickReserve.Infrastructure/
COPY src/QuickReserve.API/QuickReserve.API.csproj src/QuickReserve.API/
COPY tests/QuickReserve.Tests/QuickReserve.Tests.csproj tests/QuickReserve.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /src/src/QuickReserve.API
RUN dotnet build -c Release --no-restore -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release --no-restore -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Security: run as non-root user
RUN adduser --disabled-password --gecos "" appuser
USER appuser

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=10s \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "QuickReserve.API.dll"]
